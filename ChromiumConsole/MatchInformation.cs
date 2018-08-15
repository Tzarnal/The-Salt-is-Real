namespace ChromiumConsole
{
    class MatchInformation
    {
        public static string currentBettedPlayer;
        public static string winningplayer;
        public static string currentRedPlayer;
        public static string currentBluePlayer;

        public static string Tier;

        public static int BlueSalt;
        public static int RedSalt;

        public static int SaltBeforeMatch;
        public static int SaltBettedOnMatch;

        public static int MatchEarnings;
               
        public static bool statsBeenUpdated = false;
        public static bool HasPlacedBet = false;
        public static bool HasOfferedBet = false;

        public static bool Tournament = false;
        public static int BracketCount;

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
