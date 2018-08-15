using System;

namespace RealSalt.Data
{
    public class Character
    {
        public int CharacterId { get; set; }
        public string Name { get; set; }

        public int TotalWins => Program._forbiddingManse.GetWins(this);
        public int TotalLosses => Program._forbiddingManse.GetLosses(this);

        public int Matches => TotalWins + TotalLosses;
        public double WinPercent
        {
            get
            {
                var percent=  (double)TotalWins / (double) Matches * 100;
                return Math.Round(percent, 1);
            }
        }

        public bool IsReliableData => Matches > 4;

        public new string ToString()
        {
            if (Matches == 0)
            {
                return $"0% {TotalWins}W-{TotalLosses}L";
            }

            return $"{WinPercent}% {TotalWins}W-{TotalLosses}L";
        }
    }
}
