using System;
using System.IO;
using Newtonsoft.Json;
using Serilog;

namespace RealSalt
{
    class Configuration
    {
        public static string DataPath = "Data/";
        public static string DataFileName = "Config.json";
        public static string FullPath => DataPath + DataFileName;

        public string SaltyAccount;
        public string SaltyAccountPassword;

        public Configuration()
        {
            SaltyAccount = "Salty Account Name";
            SaltyAccountPassword = "ASuperSecretPasswordNobodyKnows";
        }

        public void Save()
        {
            var data = JsonConvert.SerializeObject(this, Formatting.Indented);

            if (!Directory.Exists(DataPath))
            {
                Directory.CreateDirectory(DataPath);
            }

            try
            {
                File.WriteAllText(FullPath, data);
            }
            catch (Exception e)
            {
                Log.Error(e, "Could not create Config.json");
            }

        }

        public static Configuration Load()
        {
            var data = File.ReadAllText(FullPath);
            return JsonConvert.DeserializeObject<Configuration>(data);
        }
    }
}
