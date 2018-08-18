using System;
using ChromiumConsole;
using ChromiumConsole.EventArguments;
using RealSalt.Data;
using Serilog;

namespace RealSalt.BettingEngine
{
    class WinRateAdjusted : IBettingEngine
    {
        private ForbiddingManse _forbiddingManse;

        public WinRateAdjusted()
        {
            _forbiddingManse = Program.ForbiddingManse;
        }

        public BettingPlan PlaceBet(MatchStartEventArgs matchArgs)
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
                    betSalt += AdditionalBetAmount(redPlayer, betSalt);
                }
                else
                {
                    betCharacter = SaltyConsole.Players.BluePlayer;                    
                    betSalt += AdditionalBetAmount(bluePlayer, betSalt);
                }

                betSymbol = "=";

                Log.Verbose("Better - Winrate: {RedPlayer} Winrate {RedWinrate}%. {BluePlayer} Winrate {BlueWinrate}%.",
                    redPlayer.Name,
                    redPlayer.WinPercent,
                    bluePlayer.Name,
                    bluePlayer.WinPercent);
            }
            else if (redPlayer.IsReliableData)
            {
                if (redPlayer.WinPercent > 50)
                {
                    betCharacter = SaltyConsole.Players.RedPlayer;
                    betSalt += AdditionalBetAmount(redPlayer, betSalt);
                }
                else
                {
                    betCharacter = SaltyConsole.Players.BluePlayer;
                }

                betSymbol = "-";

                Log.Verbose("Better - Winrate: {RedPlayer} Winrate {RedWinrate}. {BluePlayer} Winrate is unreliable.",
                    redPlayer.Name,
                    redPlayer.WinPercent,
                    bluePlayer.Name);
                
            }
            else if (bluePlayer.IsReliableData)
            {
                if (bluePlayer.WinPercent > 50)
                {
                    betCharacter = SaltyConsole.Players.BluePlayer;
                    betSalt += AdditionalBetAmount(bluePlayer,betSalt);
                }
                else
                {
                    betCharacter = SaltyConsole.Players.RedPlayer;
                }

                betSymbol = "-";

                Log.Verbose("Better - Winrate: {RedPlayer} Winrate is unreliable. {Blueplayer} Winrate is {BlueWinrate}.",
                    redPlayer.Name,
                    bluePlayer.Name,
                    bluePlayer.WinPercent);
            }

            return new BettingPlan
            {
                Symbol = betSymbol,
                Character = betCharacter,
                Salt = betSalt
            };
        }

        private int BaseBetAmount(int salt)
        {
            var digits = Math.Floor(Math.Log10(salt) + 1);
            var targetDigits = (int)digits - 3;

            return (int)Math.Pow(10, targetDigits);
        }

        private int AdditionalBetAmount(Character player,int startingAmount)
        {
            if (player.WinPercent < 51 || player.Matches == 0)
            {
                return 0;
            }

            var modifier = (player.WinPercent - 50) / 10;

            return (int)(startingAmount * modifier);
        }
    }
}
