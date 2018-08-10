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
        public int WinPercent => (Wins / Matches) * 100;
    }
}
