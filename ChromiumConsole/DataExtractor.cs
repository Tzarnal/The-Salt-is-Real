using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CefSharp;
using CefSharp.OffScreen;

namespace ChromiumConsole
{
    static class DataExtractor
    {
        private static ChromiumWebBrowser frontPageBrowser = Program.frontPageBrowser;

        public static string GetBetState()
        {
            return JavaScriptService.EvaluateJS("betstate", frontPageBrowser);
        }
               
        public static string GetBlueName()
        {
            return JavaScriptService.EvaluateJS("p2n", frontPageBrowser);
        }
               
        public static int GetBailOutAmount()
        {
            return 0;
        }
               
        public static int GetAccountLevel()
        {
            return 0;
        }
               
        public static string GetRedName()
        {
            return JavaScriptService.EvaluateJS("p1n", frontPageBrowser);
        }
               
        public static string GetRedPot()
        {
            return JavaScriptService.EvaluateJS("p1te", frontPageBrowser);
        }
               
        public static string GetBluePot()
        {
            return JavaScriptService.EvaluateJS("p2te", frontPageBrowser);
        }
               
        public static string GetBetStatus()
        {
            return JavaScriptService.EvaluateJS("betstate", frontPageBrowser);
        }
               
        public static int GetSaltBalanceNum()
        {
            int x = 0;
            if (Int32.TryParse(JavaScriptService.EvaluateJS("balance", frontPageBrowser), out x))
                return x;
            else
                return 0;
        }
               
        public static string GetStatusText()
        {
            return JavaScriptService.EvaluateJS("betstatus", frontPageBrowser);
        }
               
        public static string GetSalt()
        {
            return JavaScriptService.EvaluateJS("balance", frontPageBrowser);
        }
               
        public static int GetRedWinRate()
        {
            int x = 0;

            string winrateString = JavaScriptService.EvaluateJS("p1winrate", frontPageBrowser);
            winrateString = winrateString.Trim('%');

            //If group match
            if (winrateString.Contains("/"))
            {
                int index = winrateString.IndexOf('/');

                int redP1WinRate = 0;

                int.TryParse(winrateString.Substring(0, index), out redP1WinRate);

                int redP2WinRate = 0;

                int.TryParse(winrateString.Substring(index + 1), out redP2WinRate);

                return (int)(redP1WinRate + redP2WinRate) / 2;
            }
            
            if (int.TryParse(winrateString, out x))
            {
                return x;
            }

            return 0;
        }
               
        public static int GetBlueWinRate()
        {
            int x = 0;

            string winrateString = JavaScriptService.EvaluateJS("p2winrate", frontPageBrowser);
            winrateString = winrateString.Trim('%');

            if (winrateString.Contains("/"))
            {
               

                int index = winrateString.IndexOf('/');

                int blueP1WinRate = 0;

                int.TryParse(winrateString.Substring(0, index), out blueP1WinRate);

                int blueP2WinRate = 0;

                int.TryParse(winrateString.Substring(index + 1), out blueP2WinRate);

                return (int)(blueP1WinRate + blueP2WinRate) / 2;
            }

            if (int.TryParse(winrateString, out x))
            {
                return x;
            }

            return 0;
        }
               
        public static int GetRedTotalMatches()
        {
            int x = 0;

            string matchesTotal = JavaScriptService.EvaluateJS("p1totalmatches", frontPageBrowser);

            if (int.TryParse(matchesTotal, out x))
            {
                return x;
            }
            return 0;
        }
               
        public static int GetBlueTotalMatches()
        {
            int x = 0;

            string matchesTotal = JavaScriptService.EvaluateJS("p2totalmatches", frontPageBrowser);

            if (int.TryParse(matchesTotal, out x))
            {
                return x;
            }
            return 0;
        }
               
        public static string GetAccountName()
        {
            return JavaScriptService.EvaluateJS("", frontPageBrowser);
        }
               
        public static string GetP1WinRate()
        {
            return JavaScriptService.EvaluateJS("p1winrate", frontPageBrowser);
        }
               
        public static string GetPossibleWinningSalt()
        {
            return JavaScriptService.EvaluateJS("lastWager", frontPageBrowser);
        }

        
    }
}
