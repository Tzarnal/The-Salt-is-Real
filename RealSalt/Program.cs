using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using ChromiumConsole;
using ChromiumConsole.EventArguments;
using RealSalt.Data;
using RealSalt.BettingEngine;
using Serilog;

namespace RealSalt
{
    class Program
    {
        private static SaltyConsole _saltyBetConsole;
        private static Configuration _saltyConfiguration;
        private static ForbiddingManse _forbiddingManse;

        private static SessionResults _sessionResults;
        private static SessionResults _tournamentResults;

        private static IBettingEngine _bettingEngine;
        private static IBettingEngine _tournamentBettingEngine;
        private static IBettingEngine _bettingEngineBackup;

        private static bool _lastMatchWasTournament;

        #region MyRegion
        // Declare the SetConsoleCtrlHandler function
        // as external and receiving a delegate.

        [DllImport("Kernel32")]
        public static extern bool SetConsoleCtrlHandler(HandlerRoutine handler, bool add);

        // A delegate type to be used as the handler routine
        // for SetConsoleCtrlHandler.
        public delegate bool HandlerRoutine(CtrlTypes ctrlType);

        // An enumerated type for the control messages
        // sent to the handler routine.
        public enum CtrlTypes
        {
            CtrlCEvent = 0,
            CtrlBreakEvent,
            CtrlCloseEvent,
            CtrlLogoffEvent = 5,
            CtrlShutdownEvent
        }
        #endregion

        static void Main()
        {
            //Set ctrl-c handler
            SetConsoleCtrlHandler(ConsoleCtrlCheck, true);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()                
                .WriteTo.Console()
                .CreateLogger();

            LoadConfig();

            _saltyBetConsole = new SaltyConsole();
            _forbiddingManse = new ForbiddingManse();

            _sessionResults = new SessionResults();
            _tournamentResults = new SessionResults();

            _bettingEngine = new WinRateAdjusted(_forbiddingManse);
            _tournamentBettingEngine = new TournamentBet(_forbiddingManse);
            _bettingEngineBackup = new RandomBet();

            Log.Information("Database contains {CharacterCount} Characters.",
                +_forbiddingManse.Characters.Count());

            _saltyBetConsole.LoginSuccess += SaltyBetConsoleOnLoginSuccess;
            _saltyBetConsole.MatchStart += ConsoleOnMatchStart;
            _saltyBetConsole.MatchEnded += SaltyBetConsoleOnMatchEnded;

            _saltyBetConsole.Start(_saltyConfiguration.SaltyAccount, _saltyConfiguration.SaltyAccountPassword);
        }


        private static void LoadConfig()
        {
            if (File.Exists(Configuration.FullPath))
            {
                _saltyConfiguration = Configuration.Load();
            }
            else
            {
                _saltyConfiguration = new Configuration();
                _saltyConfiguration.Save();
            }
        }

        private static bool ConsoleCtrlCheck(CtrlTypes ctrlType)
        {
            _forbiddingManse.SaveChanges();

            _sessionResults.DisplayResult();

            SaltyConsole.exit = true;
            return true;
        }

        private static void SaltyBetConsoleOnLoginSuccess(object sender, EventArgs e)
        {           
            Log.Information("Logged in to SaltyBet.");
        }

        private static void ConsoleOnMatchStart(object sender, EventArgs eventArgs)
        {            
            var matchStartArgs = (MatchStartEventArgs) eventArgs;
                    
            int betSalt;
            SaltyConsole.Players betCharacter;
            string betSymbol;

            //Don't try to bet on matches already in progress
            if (matchStartArgs.RedPlayer == "null" ||
                matchStartArgs.BluePlayer == "null")
            {
                Log.Information("Match already in progress.");
                return;
            }

            //Store some session information
            if (_sessionResults.StartingSalt == 0)
            {
                _sessionResults.StartingSalt = matchStartArgs.Salt;
            }

            Tuple<string, int, SaltyConsole.Players> betPlan;

            if (matchStartArgs.Tournament)
            {
                //For Tournaments

                if (!_lastMatchWasTournament)
                {
                    _tournamentResults = new SessionResults {StartingSalt = matchStartArgs.Salt};
                }
                betPlan = _tournamentBettingEngine.PlaceBet(matchStartArgs);
            }
            else
            {
                //Regular betting engine
                betPlan = _bettingEngine.PlaceBet(matchStartArgs);
            }
                        
            //In case no bet was placed, fallback to random
            if (betPlan.Item3 == SaltyConsole.Players.Unknown)
            {
                betPlan = _bettingEngineBackup.PlaceBet(matchStartArgs);
            }

            betCharacter = betPlan.Item3;
            betSalt = betPlan.Item2;
            betSymbol = betPlan.Item1;

            //Place and report bet.
            _saltyBetConsole.PlaceBet(betCharacter,betSalt);

            var betCharacterName = "Unknown";
            if (betCharacter == SaltyConsole.Players.BluePlayer)
            {
                betCharacterName = matchStartArgs.BluePlayer;
            }
            else
            {
                betCharacterName = matchStartArgs.RedPlayer;
            }

            var bluePlayer = _forbiddingManse.GetOrCreateCharacter(matchStartArgs.BluePlayer);
            var redPlayer = _forbiddingManse.GetOrCreateCharacter(matchStartArgs.RedPlayer);

            var reportString =
                "Match Start: [{BetSymbol}] {RedPlayer}({RedStats}) vs {BluePlayer}({BlueStats}). Betting {SaltAmount}$ on {BetPlayer}.";

            if (matchStartArgs.Tournament)
            {
                reportString =
                    "Tournament Match Start: [{BetSymbol}] {RedPlayer}({RedStats}) vs {BluePlayer}({BlueStats}). Betting {SaltAmount}$ on {BetPlayer}.";
            }

            Log.Information(reportString,
                betSymbol,
                matchStartArgs.RedPlayer,
                redPlayer.ToString(),
                matchStartArgs.BluePlayer,
                bluePlayer.ToString(),
                betSalt,
                betCharacterName);
        }

        private static void SaltyBetConsoleOnMatchEnded(object sender, EventArgs eventArgs)
        {
            var matchEndArgs = (MatchEndEventArgs) eventArgs;

            if (matchEndArgs.Tournament)
            {
                TournamentMatchEnded(matchEndArgs);

                //Report total tournament results at the end of the tournament
                if (matchEndArgs.TournamentPlayersRemaining <= 2)
                {
                    Log.Information("Tournament Results: ");

                    _tournamentResults.DisplayResult();
                    Console.WriteLine("");
                }

                return;
            }

            MatchEnded(matchEndArgs);            
        }

        private static void MatchEnded(MatchEndEventArgs matchEndArgs)
        {
            if (_lastMatchWasTournament)
            {
                //Zero out the balance change if we are coming from a tournament match
                matchEndArgs.SaltBalanceChange = 0;
            }

            if (matchEndArgs.WinningPlayer == SaltyConsole.Players.Unknown ||
                matchEndArgs.RedPlayer == "null" ||
                matchEndArgs.BluePlayer == "null")
            {
                Log.Information("Unmonitored match has completed.");
                return;
            }

            var balanceSymbol = "";
            var resultSymbol = " ";
            if (matchEndArgs.SaltBalanceChange > 0)
            {
                balanceSymbol = "+";
                resultSymbol = "W";
            }
            else if (matchEndArgs.SaltBalanceChange < 0)
            {
                resultSymbol = "L";
            }

            _sessionResults.CurrentSalt = matchEndArgs.Salt;
            if (matchEndArgs.PickedPlayerName == matchEndArgs.WinningPlayerName)
            {
                _sessionResults.Wins++;
            }
            else
            {
                _sessionResults.Losses++;
            }

            Log.Information("Match Ended: [{ResultSymbol}] {WinningPlayer} won. Balance {Salt}[{BalanceSymbol}{SaltDifference}].",
                resultSymbol,
                matchEndArgs.WinningPlayerName,
                matchEndArgs.Salt,
                balanceSymbol,
                matchEndArgs.SaltBalanceChange);

            _lastMatchWasTournament = false;

            _forbiddingManse.RegisterMatchResult(matchEndArgs.WinningPlayerName, matchEndArgs.LoosingPlayerName);
        }

        private static void TournamentMatchEnded(MatchEndEventArgs matchEndArgs)
        {
            if (!_lastMatchWasTournament)
            {
                //Zero out the balance change if we are coming from a non tournament match
                matchEndArgs.SaltBalanceChange = 0;
            }
            
            if (matchEndArgs.WinningPlayer == SaltyConsole.Players.Unknown ||
                matchEndArgs.RedPlayer == "null" ||
                matchEndArgs.BluePlayer == "null")
            {
                Log.Information("Unmonitored tournament match has completed.");
                return;
            }

            var balanceSymbol = "";
            var resultSymbol = " ";
            if (matchEndArgs.SaltBalanceChange > 0)
            {
                balanceSymbol = "+";
                resultSymbol = "W";
            }
            else if (matchEndArgs.SaltBalanceChange < 0)
            {
                resultSymbol = "L";
            }

            _tournamentResults.CurrentSalt = matchEndArgs.Salt;
            if (matchEndArgs.PickedPlayerName == matchEndArgs.WinningPlayerName)
            {
                _tournamentResults.Wins++;
            }
            else
            {
                _tournamentResults.Losses++;
            }

            Log.Information("Tournament Match Ended: [{ResultSymbol}] {WinningPlayer} won. Balance {Salt}[{BalanceSymbol}{SaltDifference}].",
                resultSymbol,
                matchEndArgs.WinningPlayerName,
                matchEndArgs.Salt,
                balanceSymbol,
                matchEndArgs.SaltBalanceChange);

            _lastMatchWasTournament = true;

            _forbiddingManse.RegisterMatchResult(matchEndArgs.WinningPlayerName, matchEndArgs.LoosingPlayerName);
        }
    }
}
