using System;

namespace ChromiumConsole.EventArguments
{
    public class MatchEndEventArgs : EventArgs
    {
        public int Salt { get; set; }
        public string BluePlayer { get; set; }
        public string RedPlayer { get; set; }

        public bool Tournament { get; set; }
        public int TournamentPlayersRemaining { get; set; }

        public SaltyConsole.Players WinningPlayer { get; set; }
        public string WinningPlayerName { get; set; }
        public int SaltBalanceChange { get; set; }
    }
}
