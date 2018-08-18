using System;
using ChromiumConsole;
using ChromiumConsole.EventArguments;
using RealSalt.Data;
using Serilog;

namespace RealSalt.BettingEngine
{
    class TournamentBet : IBettingEngine
    {
        private ForbiddingManse _forbiddingManse;
        private static Random _genie;

        public TournamentBet(ForbiddingManse forbiddingManse)
        {
            _forbiddingManse = forbiddingManse;
            _genie = new Random();
        }

        public BettingPlan PlaceBet(MatchStartEventArgs matchArgs)
        {
            string betSymbol;

            var betSalt = BaseBetAmount(matchArgs.Salt,matchArgs.TournamentPlayersRemaining);
            var betCharacter = SaltyConsole.Players.Unknown;

            var bluePlayer = _forbiddingManse.GetOrCreateCharacter(matchArgs.BluePlayer);
            var redPlayer = _forbiddingManse.GetOrCreateCharacter(matchArgs.RedPlayer);

            if (redPlayer.IsReliableData && bluePlayer.IsReliableData)
            {
                //Ideal case, we have reliable information on both characters
                if (redPlayer.WinPercent > bluePlayer.WinPercent)
                {
                    betCharacter = SaltyConsole.Players.RedPlayer;
                }
                else
                {
                    betCharacter = SaltyConsole.Players.BluePlayer;
                }

                betSymbol = "=";

                Log.Verbose("Better - Winrate Tournament: {RedPlayer} Winrate {RedWinrate}%. {BluePlayer} Winrate {BlueWinrate}%.",
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
                }
                else
                {
                    betCharacter = SaltyConsole.Players.BluePlayer;
                }

                betSymbol = "-";

                Log.Verbose("Better - Winrate Tournament: {RedPlayer} Winrate {RedWinrate}. {BluePlayer} Winrate is unreliable.",
                    redPlayer.Name,
                    redPlayer.WinPercent,
                    bluePlayer.Name);
            }
            else if (bluePlayer.IsReliableData)
            {
                if (bluePlayer.WinPercent > 50)
                {
                    betCharacter = SaltyConsole.Players.BluePlayer;
                }
                else
                {
                    betCharacter = SaltyConsole.Players.RedPlayer;
                }

                betSymbol = "-";

                Log.Verbose("Better - Winrate Tournament: {RedPlayer} Winrate is unreliable. {Blueplayer} Winrate is {BlueWinrate}.",
                    redPlayer.Name,
                    bluePlayer.Name,
                    bluePlayer.WinPercent);
            }
            else
            {
                betSymbol = "~";
                betCharacter = SaltyConsole.Players.RedPlayer;
                if (IsHeads())
                {
                    betCharacter = SaltyConsole.Players.BluePlayer;
                }

                Log.Verbose("Better - Winrate Tournament: No reliable stats on either character. Randomly picked {player}.",
                    betCharacter.ToString());
            }

            return new BettingPlan
            {
                Symbol = betSymbol,
                Character = betCharacter,
                Salt = betSalt
            };
        }

        private bool IsHeads()
        {
            var result = _genie.Next(0, 2);

            if (result == 1)
                return true;

            return false;
        }

        private int BaseBetAmount(int salt, int playersRemaining)
        {
            int bet = 100;

            if (salt < 2000)
            {
                return salt;
            }

            if (playersRemaining > 8)
            {
                bet = (int)(salt * 0.3);
            }

            if (playersRemaining > 2)
            {
                bet = (int)(salt * 0.5);
            }

            if (playersRemaining == 2)
            {
                bet = (int)(salt * 0.75);
            }            

            if (salt - bet < 2000)
            {
                return salt;
            }

            return bet;
        }
    }
}
