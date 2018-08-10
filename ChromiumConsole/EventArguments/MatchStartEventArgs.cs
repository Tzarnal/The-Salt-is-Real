using System;

namespace ChromiumConsole.EventArguments
{
    public class MatchStartEventArgs : EventArgs
    {
        public int Salt { get; set; }
        public string BluePlayer { get; set; }
        public string RedPlayer { get; set; }
        public bool Tournament { get; set; }
        public int TournamentPlayersRemaining { get; set; }
    }
}
