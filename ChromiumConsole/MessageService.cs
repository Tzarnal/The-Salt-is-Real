using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChromiumConsole
{
    class MessageService
    {
        

        public static void OnLoginMessage()
        {
            Console.Clear();
            Console.WriteLine("Successfully logged in..." + Environment.NewLine);
        }

        public static void OnClosingBotMessage()
        {
            CultureInfo culture = (CultureInfo)CultureInfo.CreateSpecificCulture("en-US").Clone();
            culture.NumberFormat.CurrencyNegativePattern = 1;

            Console.WriteLine("\n\n");

            WriteLineInColor("BETTING BOT HAS CLOSED", ConsoleColor.DarkCyan);
            Console.WriteLine($"Number of bets in session: {SessionStatistics.MatchesPlayed}");
            Console.WriteLine($"Salt net sum after session: {SessionStatistics.NetEarnings.ToString("C0", culture)}");
            Console.WriteLine($"Biggest win: {SessionStatistics.BiggestWin}");
        }

        public static void DisplayNewMatchInformationMessage()
        {
            WriteLineInColor("NEW MATCH IS BEGINNING", ConsoleColor.DarkCyan);

            WriteInColor(MatchInformation.currentRedPlayer, ConsoleColor.Red);
            Console.Write(" VS ");
            WriteInColor(MatchInformation.currentBluePlayer, ConsoleColor.Blue);
            Console.WriteLine();
        }

        public static void BetPlacedMessage(string player, int amount)
        {
            if (PlayerInformation.SaltAmount <= amount)
            {
                Console.Write($"\nALL IN on ");
            }
            else
            {
                Console.Write($"\n{amount.ToString("C0", CultureInfo.CreateSpecificCulture("en-US"))} placed on ");
            }

            WriteInColor(player == "player1" ? DataExtractor.GetRedName() + "\n" : DataExtractor.GetBlueName() + "\n", player == "player1" ? ConsoleColor.Red : ConsoleColor.Blue);
            Console.WriteLine($"Confidence: {BetService.LastCalculatedConfidence.ToString("n3")}");

        }

        public static void OnMatchEndedMessage()
        {
            string matchOutcome = MatchInformation.SaltBeforeMatch > PlayerInformation.SaltAmount ? "lost" : "won";

            WriteInColor("\nMATCH ENDED\n", ConsoleColor.DarkCyan);
            WriteInColor($"{MatchInformation.currentBettedPlayer} ", MatchInformation.currentBettedPlayer == MatchInformation.currentRedPlayer ? ConsoleColor.Red : ConsoleColor.Blue);
            Console.WriteLine($"{matchOutcome} the match");
        }

        private static void WriteInColor(string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ForegroundColor = ConsoleColor.White;
        }
        private static void WriteLineInColor(string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ForegroundColor = ConsoleColor.White;
        }

    }
}
