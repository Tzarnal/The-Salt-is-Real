using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChromiumConsole
{
    class PlayerInformation
    {
        public static string Name { get; private set; } 
        public static int SaltAmount { get { return DataExtractor.GetSaltBalanceNum(); } }
        public static int Level { get; private set; }
        public static int BailOutAmount { get; set; } = 4000;

        public static void InitializePlayerInformation()
        {
            Name = DataExtractor.GetAccountName();
            BailOutAmount = DataExtractor.GetBailOutAmount();
        }

    }
}
