using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CefSharp;
using System.Threading;
using CefSharp.OffScreen;
using System.Net;
using System.Drawing;
using System.IO;
using System.Diagnostics;

namespace ChromiumConsole
{
    class BetService
    {

        public static BetSettings BetSettings = new BetSettings();

        public static float LastCalculatedConfidence { get; private set; }

        public static void Bet(string player, int BetAmount)
        {
            if (BetAmount > PlayerInformation.SaltAmount)
            {
                BetAmount = PlayerInformation.SaltAmount;
            }

            bool successflag1, successflag2;

            JavaScriptService.ExecuteJS($"document.getElementById(\"wager\").value = \"{BetAmount}\"", Program.frontPageBrowser, out successflag1);
            JavaScriptService.ExecuteJS($"document.getElementById(\"{player}\").click()", Program.frontPageBrowser, out successflag2);

            if (!successflag1 || !successflag2)
            {
                Console.WriteLine("JS error!");
                MatchInformation.HasPlacedBet = false;
                return;
            }

            MatchInformation.SaltBettedOnMatch = BetAmount;
            MatchInformation.SaltBeforeMatch = PlayerInformation.SaltAmount;
            SessionStatistics.MatchesPlayed++;
            MatchInformation.currentBettedPlayer = player == "player1" ? MatchInformation.currentRedPlayer : MatchInformation.currentBluePlayer;
            MessageService.BetPlacedMessage(player, BetAmount);
            MatchInformation.HasPlacedBet = true;

        }

        public static int CalculateWager()
        {
            float p1WinRateDifference = (float)MatchInformation.p1WinRate / MatchInformation.p2WinRate;
            float p2WinRateDifference = (float)MatchInformation.p2WinRate / MatchInformation.p1WinRate;

            float p1Stability = 1 / (1 + (float)Math.Pow(0.9f, MatchInformation.p1TotalMatches - 5));
            float p2Stability = 1 / (1 + (float)Math.Pow(0.9f, MatchInformation.p2TotalMatches - 5));

            float p1WinChance = p1WinRateDifference * p1Stability;
            float p2WinChance = p2WinRateDifference * p2Stability;

            float winnerConfidence = p1WinChance > p2WinChance ? (p1WinChance - p2WinChance) : (p2WinChance - p1WinChance);

            LastCalculatedConfidence = winnerConfidence;

            if (RunRiskCheck(winnerConfidence))
            {
                return (int)(PlayerInformation.SaltAmount * (winnerConfidence * RiskMultiplier()) + PlayerInformation.BailOutAmount);
            }

            else
            {
                return 1000;
            }
        }

        private static float RiskMultiplier()
        {
            switch (BetSettings.Risk_Level)
            {
                case BetSettings.RISK_LEVEL.VERY_SAFE:
                    return 0.25f;
                case BetSettings.RISK_LEVEL.SAFE:
                    return 0.45f;
                case BetSettings.RISK_LEVEL.MODERATE:
                    return 0.75f;
                case BetSettings.RISK_LEVEL.RISKY:
                    return 0.9f;
                case BetSettings.RISK_LEVEL.VERY_RISKY:
                    return 1f;
            }
            return 0;
        }

        private static bool RunRiskCheck(float winnerConfidence)
        {
            switch (BetSettings.Risk_Level)
            {
                case BetSettings.RISK_LEVEL.VERY_SAFE:
                    if (winnerConfidence >= 1.2f)
                    {
                        return true;
                    }
                    break;
                case BetSettings.RISK_LEVEL.SAFE:
                    if (winnerConfidence >= 0.9f)
                    {
                        return true;
                    }
                    break;
                case BetSettings.RISK_LEVEL.MODERATE:
                    if (winnerConfidence >= 0.5f)
                    {
                        return true;
                    }
                    break;
                case BetSettings.RISK_LEVEL.RISKY:
                    if (winnerConfidence >= 0.3f)
                    {
                        return true;
                    }
                    break;
                case BetSettings.RISK_LEVEL.VERY_RISKY:
                    if (winnerConfidence >= 0.1f)
                    {
                        return true;
                    }
                    break;
            }
            return false;
        }
    }

    public class BetSettings
    {
        public RISK_LEVEL Risk_Level = RISK_LEVEL.MODERATE;

        public enum RISK_LEVEL
        {
            VERY_SAFE,
            SAFE,
            MODERATE,
            RISKY,
            VERY_RISKY
        }
    }
}
