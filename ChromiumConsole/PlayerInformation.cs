namespace ChromiumConsole
{
    class PlayerInformation
    {
        public static string Name { get; private set; } 
        public static int SaltAmount => DataExtractor.GetSaltBalanceNum();
        public static int Level { get; private set; }
        public static int BailOutAmount { get; set; } = 4000;

        public static void InitializePlayerInformation()
        {
            Name = DataExtractor.GetAccountName();
            BailOutAmount = 100;
        }

    }
}
