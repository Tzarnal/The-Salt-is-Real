using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChromiumConsole
{
    class MatchInformation
    {
        public static string currentBettedPlayer;

        public static string currentRedPlayer;
        public static string currentBluePlayer;

        public static int SaltBeforeMatch;
        public static int SaltBettedOnMatch;

        public static int MatchEarnings;

        public static int p1WinRate;
        public static int p2WinRate;
               
        public static int p1TotalMatches;
        public static int p2TotalMatches;
               
        public static bool statsBeenUpdated = false;
        public static bool HasPlacedBet = false;


        public static void UpdateFighterData()
        {
            if (!HasPlacedBet && !statsBeenUpdated)
            {
                UpdateData();
            }
        }

        public static void UpdateData()
        {
            currentRedPlayer = DataExtractor.GetRedName();
            currentBluePlayer = DataExtractor.GetBlueName();
            p1WinRate = DataExtractor.GetRedWinRate();
            p2WinRate = DataExtractor.GetBlueWinRate();
            p1TotalMatches = DataExtractor.GetRedTotalMatches();
            p2TotalMatches = DataExtractor.GetBlueTotalMatches();
        }

        public static bool IsBetStateOpen()
        {
            //if bet has already been placed
            if (HasPlacedBet)
            {
                return false;
            }

            string betState = DataExtractor.GetBetState();

            //Check if betstate is open
            if (betState == "open")
            {
                MessageService.DisplayNewMatchInformationMessage();
                return true;
            }

            return false;
        }

        public static bool IsWinRatesOver0()
        {
            if (p1WinRate > 0)
            {
                if (p2WinRate > 0)
                {
                    return true;
                }
                return true;
            }
            return false;
        }

        public static void UpdateMatchEarnings()
        {
            //If the bet was won
            if (PlayerInformation.SaltAmount > SaltBeforeMatch)
            {
                MatchEarnings = (PlayerInformation.SaltAmount - SaltBettedOnMatch);
            }
            else
            {
                MatchEarnings = -SaltBettedOnMatch;
            }

            if (SessionStatistics.BiggestWin < MatchEarnings)
            {
                SessionStatistics.BiggestWin = MatchEarnings;
            }

            SessionStatistics.NetEarnings += MatchEarnings;
        }
    }
}
