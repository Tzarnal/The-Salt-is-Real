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

        public static string GetFooterText()
        {
            return JavaScriptService.EvaluateJS("$(\"#footer-alert\").text()", frontPageBrowser);
        }

        public static int GetPayout()
        {
            var payout = JavaScriptService.EvaluateJS("payout", frontPageBrowser);

            return int.TryParse(payout, out var count) ? count : 0;
        }

        public static bool GetTournamentActive()
        {
            var result =  JavaScriptService.EvaluateJS("$(\"html body div#wrapper.locked div#bottomcontent form#fightcard div#balancewrapper span#tournament-note\").text()", frontPageBrowser);

            return result == "(Tournament Balance)";
        }

        public static int GetBracketCount()
        {
            var countText = JavaScriptService.EvaluateJS("$(\"#footer-alert\").text()", frontPageBrowser);

            if (string.IsNullOrWhiteSpace(countText))
                return 0;

            var chars = countText.Substring(0,2).Trim();

            return int.TryParse(chars, out var count) ? count : 0;
        }
    }
}
