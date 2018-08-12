using System;
using ChromiumConsole;
using ChromiumConsole.EventArguments;
using RealSalt.Data;

namespace RealSalt.BettingEngine
{
    class WinRateAdjusted : IBettingEngine
    {
        private ForbiddingManse _forbiddingManse;

        public WinRateAdjusted(ForbiddingManse forbiddingManse)
        {
            _forbiddingManse = forbiddingManse;
        }

        public Tuple<string, int, SaltyConsole.Players> PlaceBet(MatchStartEventArgs matchArgs)
        {            
            var betSymbol = " ";
         
            var betSalt = BaseBetAmount(matchArgs.Salt);
            var betCharacter = SaltyConsole.Players.Unknown;

            var bluePlayer = _forbiddingManse.GetOrCreateCharacter(matchArgs.BluePlayer);
            var redPlayer = _forbiddingManse.GetOrCreateCharacter(matchArgs.RedPlayer);

            if (redPlayer.IsReliableData && bluePlayer.IsReliableData)
            {
                //Ideal case, we have reliable information on both characters
                if (redPlayer.WinPercent > bluePlayer.WinPercent)
                {
                    betCharacter = SaltyConsole.Players.RedPlayer;
                    betSalt += redPlayer.AdditionalBetAmount(betSalt);
                }
                else
                {
                    betCharacter = SaltyConsole.Players.BluePlayer;
                    betSalt += bluePlayer.AdditionalBetAmount(betSalt);
                }
                betSymbol = "=";
            }
            else if (redPlayer.IsReliableData)
            {
                if (redPlayer.WinPercent > 50)
                {
                    betCharacter = SaltyConsole.Players.RedPlayer;
                    betSalt += redPlayer.AdditionalBetAmount(betSalt);
                }
                else
                {
                    betCharacter = SaltyConsole.Players.BluePlayer;
                }
                betSymbol = "-";
            }
            else if (bluePlayer.IsReliableData)
            {
                if (bluePlayer.WinPercent > 50)
                {
                    betCharacter = SaltyConsole.Players.BluePlayer;
                    betSalt += bluePlayer.AdditionalBetAmount(betSalt);
                }
                else
                {
                    betCharacter = SaltyConsole.Players.RedPlayer;
                }
                betSymbol = "-";
            } 

            return new Tuple<string, int, SaltyConsole.Players>(betSymbol, betSalt, betCharacter);
        }

        private int BaseBetAmount(int salt)
        {
            var digits = Math.Floor(Math.Log10(salt) + 1);
            var targetDigits = (int)digits - 3;

            return (int)Math.Pow(10, targetDigits);
        }
    }
}
