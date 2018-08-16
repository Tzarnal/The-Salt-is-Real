using System;
using Serilog;

namespace RealSalt.Data
{
    class SessionResults
    {
        public int Wins { get; set; }
        public int Losses { get; set; }

        public int StartingSalt { get; set; }
        public int CurrentSalt { get; set; }

        public int Matches => Wins + Losses;
        public double WinPercent
        {
            get
            {
                var percent = (double)Wins / (double)Matches * 100;
                return Math.Round(percent, 1);
            }
        }

        public void DisplayResult()
        {
            Console.WriteLine();

            if (Matches == 0)
            {
                Log.Information("No matches logged, cannot display session results.");
                return;
            }

            var saltBalanceChange = CurrentSalt - StartingSalt;

            var balanceSymbol = "";
            if (saltBalanceChange > 0)
            {
                balanceSymbol = "+";
            }

            Log.Information("Placed {TotalMatches} Bets with {Wins}[{WinPercent}%] win(s). Started at ${StartingSalt:N0} with a ${CurrentSalt:N0}[{BalanceSymbol}{BalanceChang:N0}] final balance.",
                Matches,
                Wins,
                WinPercent,
                StartingSalt,
                CurrentSalt,
                balanceSymbol,
                saltBalanceChange);
        }
    }
}
