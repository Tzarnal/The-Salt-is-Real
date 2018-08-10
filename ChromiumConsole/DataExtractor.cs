using System;
using CefSharp.OffScreen;

namespace ChromiumConsole
{
    static class DataExtractor
    {
        private static ChromiumWebBrowser frontPageBrowser = SaltyConsole.frontPageBrowser;

        public static string GetBetState()
        {
            return JavaScriptService.EvaluateJS("betstate", frontPageBrowser);
        }
               
        public static string GetBlueName()
        {
            return JavaScriptService.EvaluateJS("p2n", frontPageBrowser);
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
                                      
        public static string GetAccountName()
        {
            return JavaScriptService.EvaluateJS("", frontPageBrowser);
        }

               
        public static string GetPossibleWinningSalt()
        {
            return JavaScriptService.EvaluateJS("lastWager", frontPageBrowser);
        }

        
    }
}
