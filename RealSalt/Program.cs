using System;
using System.IO;
using System.Linq;
using ChromiumConsole;
using ChromiumConsole.EventArguments;
using RealSalt.Data;
using Serilog;

namespace RealSalt
{
    class Program
    {
        private static SaltyConsole _saltyBetConsole;
        private static Configuration _saltyConfiguration;
        private static ForbiddingManse _forbiddingManse;


        static void Main(string[] args)
        {            
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()                
                .WriteTo.Console()
                .CreateLogger();

            LoadConfig();

            _saltyBetConsole = new SaltyConsole();
            _forbiddingManse = new ForbiddingManse();
            
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


        private static void SaltyBetConsoleOnLoginSuccess(object sender, EventArgs e)
        {           
            Log.Information($"Logged in to SaltyBet.");
        }

        private static void ConsoleOnMatchStart(object sender, EventArgs eventArgs)
        {            
            var matchStartArgs = (MatchStartEventArgs) eventArgs;

            if (matchStartArgs.RedPlayer == "null" ||
                matchStartArgs.BluePlayer == "null")
            {
                Log.Information("Match already in progress.");
                return;
            }


            _saltyBetConsole.PlaceBet(SaltyConsole.Players.RedPlayer,10);

            Log.Information("Match Started : {RedPlayer} vs {BluePlayer}. Betting {SaltAmount}$ on {RedPlayer}.",
                matchStartArgs.RedPlayer,
                matchStartArgs.BluePlayer,
                10);
        }

        private static void SaltyBetConsoleOnMatchEnded(object sender, EventArgs eventArgs)
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
            if(matchEndArgs.SaltBalanceChange > 0)
            {
                balanceSymbol = "+";
            }

            Log.Information("Match Ended: {WinningPlayer} won. Balance {Salt}[{BalanceSymbol}{SaltDifference}].",
                matchEndArgs.WinningPlayerName,
                matchEndArgs.Salt,
                balanceSymbol,
                matchEndArgs.SaltBalanceChange);

            _forbiddingManse.RegisterMatchResult(matchEndArgs.WinningPlayerName,matchEndArgs.LoosingPlayerName);
        }
    }
}
