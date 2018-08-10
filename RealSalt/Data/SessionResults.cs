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
        public int WinPercent => (Wins / Matches) * 100;

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

            Log.Information("Placed {TotalMatches} Bets with {Wins}[{WinPercent}%] win(s). Started at ${StartingSalt} with a ${CurrentSalt}[{BalanceSymbol}{BalanceChange}] final balance.",
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
