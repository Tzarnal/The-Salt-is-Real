using System;


namespace RealSalt.Data
{
    public enum MatchType
    {
        MatchMaking,
        Tournament,
        Exhibition
    }

    public class MatchRecord
    {        
        public int MatchRecordId { get; set; }

        public int WinnerCharacterId { get; set; }
        public int LoserCharacterId { get; set; }

        public int WinnerSalt { get; set; }
        public int LoserSalt { get; set; }

        public string Tier { get; set; }

        public DateTime MatchStart { get; set; }
        public TimeSpan MatchLength { get; set; }

        public MatchType MatchType { get; set; }
    }
}
