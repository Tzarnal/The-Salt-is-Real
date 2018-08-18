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
        
        private static SessionResults _sessionResults;
        private static SessionResults _tournamentResults;

        private static IBettingEngine _bettingEngine;
        private static IBettingEngine _tournamentBettingEngine;
        private static IBettingEngine _bettingEngineBackup;
        
        public static ForbiddingManse ForbiddingManse;

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
            ForbiddingManse = new ForbiddingManse();

            _sessionResults = new SessionResults();
            _tournamentResults = new SessionResults();

            _bettingEngine = new WinRateAdjusted(ForbiddingManse);
            _tournamentBettingEngine = new TournamentBet(ForbiddingManse);
            _bettingEngineBackup = new RandomBet();

            try
            {
                Log.Information("Database contains {CharacterCount} Characters.",
                    +ForbiddingManse.Characters.Count());
            }
            catch (System.Data.Entity.ModelConfiguration.ModelValidationException e)
            {
                Log.Warning(e, "Database is empty.");
            }
            catch (Exception e)
            {
                Log.Error(e, "Problem with the database.");
            }

            
            _saltyBetConsole.LoginSuccess += SaltyBetConsoleOnLoginSuccess;
            _saltyBetConsole.MatchStart += ConsoleOnMatchStart;
            _saltyBetConsole.MatchEnded += ConsoleOnMatchEnded;

            _saltyBetConsole.TournamentMatchStart += ConsoleOnTournamentMatchStart;
            _saltyBetConsole.TournamentMatchEnded += ConsoleOnTournamentMatchEnded;
            _saltyBetConsole.TournamentEnded += SaltyBetConsoleOnTournamentEnded;
            
            _saltyBetConsole.ExhibitionMatchStart += SaltyBetConsoleOnExhibitionMatchStart;
            _saltyBetConsole.ExhibitionMatchEnded += SaltyBetConsoleOnExhibitionMatchEnded;

            _saltyBetConsole.TwitchLoginSuccess += SaltyBetConsoleOnTwitchLoginSuccess;

            _saltyBetConsole.Start(_saltyConfiguration.SaltyAccount, _saltyConfiguration.SaltyAccountPassword, _saltyConfiguration.TwitchAccount, _saltyConfiguration.TwitchToken);
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
            ForbiddingManse.SaveChanges();

            _sessionResults.DisplayResult();

            SaltyConsole.exit = true;
            return true;
        }

        private static void SaltyBetConsoleOnLoginSuccess(object sender, EventArgs e)
        {           
            Log.Information("Logged in to SaltyBet.");
        }


        private static void SaltyBetConsoleOnTwitchLoginSuccess(object sender, EventArgs eventArgs)
        {
            Log.Information("Logged in to Twitch.");
        }

        private static void ConsoleOnMatchStart(object sender, EventArgs eventArgs)
        {            
            var matchStartArgs = (MatchStartEventArgs) eventArgs;

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

            //Regular betting engine
            var betPlan = _bettingEngine.PlaceBet(matchStartArgs);
                        
            //In case no bet was placed, fallback to random
            if (betPlan.Item3 == SaltyConsole.Players.Unknown)
            {
                betPlan = _bettingEngineBackup.PlaceBet(matchStartArgs);
            }

            var betCharacter = betPlan.Item3;
            var betSalt = betPlan.Item2;
            var betSymbol = betPlan.Item1;

            //Place and report bet.
            _saltyBetConsole.PlaceBet(betCharacter,betSalt);

            var betCharacterName = betCharacter == SaltyConsole.Players.BluePlayer ? matchStartArgs.BluePlayer : matchStartArgs.RedPlayer;

            var bluePlayer = ForbiddingManse.GetOrCreateCharacter(matchStartArgs.BluePlayer);
            var redPlayer = ForbiddingManse.GetOrCreateCharacter(matchStartArgs.RedPlayer);
            
            Log.Information("Match Start: [{BetSymbol}] {RedPlayer}({RedStats}) vs {BluePlayer}({BlueStats}). Betting {SaltAmount:N0}$ on {BetPlayer}.",
                betSymbol,
                matchStartArgs.RedPlayer,
                redPlayer.ToString(),
                matchStartArgs.BluePlayer,
                bluePlayer.ToString(),
                betSalt,
                betCharacterName);
        }

        private static void ConsoleOnTournamentMatchStart(object sender, EventArgs eventArgs)
        {
            var matchStartArgs = (MatchStartEventArgs)eventArgs;

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

            if (_tournamentResults.StartingSalt == 0)
            {
                _tournamentResults.StartingSalt = matchStartArgs.Salt;
            }

            //For Tournaments
            var betPlan = _tournamentBettingEngine.PlaceBet(matchStartArgs);

            //In case no bet was placed, fallback to random
            if (betPlan.Item3 == SaltyConsole.Players.Unknown)
            {
                betPlan = _bettingEngineBackup.PlaceBet(matchStartArgs);
            }

            var betCharacter = betPlan.Item3;
            var betSalt = betPlan.Item2;
            var betSymbol = betPlan.Item1;

            //Place and report bet.
            _saltyBetConsole.PlaceBet(betCharacter, betSalt);

            var betCharacterName = betCharacter == SaltyConsole.Players.BluePlayer ? matchStartArgs.BluePlayer : matchStartArgs.RedPlayer;

            var bluePlayer = ForbiddingManse.GetOrCreateCharacter(matchStartArgs.BluePlayer);
            var redPlayer = ForbiddingManse.GetOrCreateCharacter(matchStartArgs.RedPlayer);


            Log.Information("Tournament Match Start: [{BetSymbol}] {RedPlayer}({RedStats}) vs {BluePlayer}({BlueStats}). Betting {SaltAmount:N0}$ on {BetPlayer}.",
                betSymbol,
                matchStartArgs.RedPlayer,
                redPlayer.ToString(),
                matchStartArgs.BluePlayer,
                bluePlayer.ToString(),
                betSalt,
                betCharacterName);
        }
        
        private static void ConsoleOnMatchEnded(object sender, EventArgs eventArgs)
        {
            var matchEndArgs = (MatchEndEventArgs) eventArgs;

            if (matchEndArgs.WinningPlayer == SaltyConsole.Players.Unknown ||
                matchEndArgs.RedPlayer == "null" ||
                matchEndArgs.BluePlayer == "null")
            {
                Log.Information("Unmonitored match has completed.");
                return;
            }

            var balanceSymbol = "";
            var resultSymbol = matchEndArgs.WinningPlayerName == matchEndArgs.PickedPlayerName ? "W" : "L";

            if (matchEndArgs.PickedPlayerName != matchEndArgs.BluePlayer &&
                matchEndArgs.PickedPlayerName != matchEndArgs.RedPlayer)
            {
                resultSymbol = " ";
            }

            if (matchEndArgs.SaltBalanceChange > 0)
            {
                balanceSymbol = "+";
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


            Log.Information("Match Ended: [{ResultSymbol}] {WinningPlayer} won. Balance {Salt:N0}[{BalanceSymbol}{SaltDifference:N0}].",
                resultSymbol,
                matchEndArgs.WinningPlayerName,
                matchEndArgs.Salt,
                balanceSymbol,
                matchEndArgs.SaltBalanceChange);

            ForbiddingManse.RegisterMatchResult(matchEndArgs, MatchType.MatchMaking);
        }

        private static void ConsoleOnTournamentMatchEnded(object sender, EventArgs eventArgs)
        {
            var matchEndArgs = (MatchEndEventArgs)eventArgs;

            if (matchEndArgs.WinningPlayer == SaltyConsole.Players.Unknown ||
                matchEndArgs.RedPlayer == "null" ||
                matchEndArgs.BluePlayer == "null")
            {
                Log.Information("Unmonitored tournament match has completed.");
                return;
            }

            var balanceSymbol = "";
            var resultSymbol = matchEndArgs.WinningPlayerName == matchEndArgs.PickedPlayerName ? "W" : "L";

            if (matchEndArgs.PickedPlayerName != matchEndArgs.BluePlayer &&
                matchEndArgs.PickedPlayerName != matchEndArgs.RedPlayer)
            {
                resultSymbol = " ";
            }

            if (matchEndArgs.SaltBalanceChange > 0)
            {
                balanceSymbol = "+";
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

            Log.Information("Tournament Match Ended: [{ResultSymbol}] {WinningPlayer} won. Balance {Salt:N0}[{BalanceSymbol}{SaltDifference:N0}].",
                resultSymbol,
                matchEndArgs.WinningPlayerName,
                matchEndArgs.Salt,
                balanceSymbol,
                matchEndArgs.SaltBalanceChange);

            ForbiddingManse.RegisterMatchResult(matchEndArgs, MatchType.Tournament);

        }

        private static void SaltyBetConsoleOnExhibitionMatchStart(object sender, EventArgs eventArgs)
        {
            var matchStartArgs = (MatchStartEventArgs)eventArgs;

            //Don't try to bet on matches already in progress
            if (matchStartArgs.RedPlayer == "null" ||
                matchStartArgs.BluePlayer == "null")
            {
                Log.Information("Exhibition match already in progress.");
                return;
            }

            //Store some session information
            if (_sessionResults.StartingSalt == 0)
            {
                _sessionResults.StartingSalt = matchStartArgs.Salt;
            }

            //Regular betting engine
            var betPlan = _bettingEngine.PlaceBet(matchStartArgs);

            //In case no bet was placed, fallback to random
            if (betPlan.Item3 == SaltyConsole.Players.Unknown)
            {
                betPlan = _bettingEngineBackup.PlaceBet(matchStartArgs);
            }

            var betCharacter = betPlan.Item3;
            var betSalt = betPlan.Item2;
            var betSymbol = betPlan.Item1;

            //Exhibitions are garbage, only bet half of normal
            betSalt = (int) (betSalt * 0.5);

            _saltyBetConsole.PlaceBet(betCharacter, betSalt);

            var betCharacterName = betCharacter == SaltyConsole.Players.BluePlayer ? matchStartArgs.BluePlayer : matchStartArgs.RedPlayer;

            var bluePlayer = ForbiddingManse.GetOrCreateCharacter(matchStartArgs.BluePlayer);
            var redPlayer = ForbiddingManse.GetOrCreateCharacter(matchStartArgs.RedPlayer);

            Log.Information("Exhibition Match Start: [{BetSymbol}] {RedPlayer}({RedStats}) vs {BluePlayer}({BlueStats}). Betting {SaltAmount:N0}$ on {BetPlayer}.",
                betSymbol,
                matchStartArgs.RedPlayer,
                redPlayer.ToString(),
                matchStartArgs.BluePlayer,
                bluePlayer.ToString(),
                betSalt,
                betCharacterName);
        }

        private static void SaltyBetConsoleOnExhibitionMatchEnded(object sender, EventArgs eventArgs)
        {
            var matchEndArgs = (MatchEndEventArgs)eventArgs;

            if (matchEndArgs.WinningPlayer == SaltyConsole.Players.Unknown ||
                matchEndArgs.RedPlayer == "null" ||
                matchEndArgs.BluePlayer == "null")
            {
                Log.Information("Unmonitored exhibition match has completed.");
                return;
            }

            var balanceSymbol = "";
            var resultSymbol = matchEndArgs.WinningPlayerName == matchEndArgs.PickedPlayerName ? "W" : "L";

            if (matchEndArgs.PickedPlayerName != matchEndArgs.BluePlayer &&
                matchEndArgs.PickedPlayerName != matchEndArgs.RedPlayer)
            {
                resultSymbol = " ";
            }

            if (matchEndArgs.SaltBalanceChange > 0)
            {
                balanceSymbol = "+";
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

            Log.Information("Exhibition Match Ended: [{ResultSymbol}] {WinningPlayer} won. Balance {Salt:N0}[{BalanceSymbol}{SaltDifference:N0}].",
                resultSymbol,
                matchEndArgs.WinningPlayerName,
                matchEndArgs.Salt,
                balanceSymbol,
                matchEndArgs.SaltBalanceChange);

            ForbiddingManse.RegisterMatchResult(matchEndArgs, MatchType.Exhibition);
        }

        private static void SaltyBetConsoleOnTournamentEnded(object sender, EventArgs eventArgs)
        {
            _tournamentResults.DisplayResult();
            _tournamentResults = new SessionResults();
        }
    }
}
