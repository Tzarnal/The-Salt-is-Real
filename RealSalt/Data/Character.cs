using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealSalt.Data
{
    public class Character
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public int Wins { get; set; }
        public int Losses { get; set; }

        public int Matches => Wins + Losses;
        public int WinPercent => Wins / Matches * 100;

        public bool IsReliableData => Matches > 4;

        public new string ToString()
        {
            if (Matches == 0)
            {
                return $"0% {Wins}W-{Losses}L";
            }

            return $"{WinPercent}% {Wins}W-{Losses}L";
        }

        public int AdditionalBetAmount(int startingAmount)
        {
            if (WinPercent < 51 || Matches == 0)
            {
                return 0;
            }
                

            var modifier = (WinPercent - 50) / 10;

            return startingAmount * modifier;
        }
    }
}
