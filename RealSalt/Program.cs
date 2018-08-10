using System;
using System.IO;
using ChromiumConsole;
using ChromiumConsole.EventArguments;
using Serilog;

namespace RealSalt
{
    class Program
    {
        private static SaltyConsole _saltyBetConsole;
        private static Configuration _saltyConfiguration;

        static void Main(string[] args)
        {            
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()                
                .WriteTo.Console()
                .CreateLogger();

            LoadConfig();

            _saltyBetConsole = new SaltyConsole();
            
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
            
           _saltyBetConsole.PlaceBet(SaltyConsole.Players.RedPlayer,10);

            Log.Information($"Match Started : {matchStartArgs.RedPlayer} vs {matchStartArgs.BluePlayer}. Betting 10$ on {matchStartArgs.RedPlayer}.");
        }

        private static void SaltyBetConsoleOnMatchEnded(object sender, EventArgs eventArgs)
        {
            var matchEndArgs = (MatchEndEventArgs) eventArgs;

            Log.Information($"Match Ended: {matchEndArgs.WinningPlayerName} won. Balance {matchEndArgs.Salt}[{matchEndArgs.SaltBalanceChange}].");
        }
    }
}
