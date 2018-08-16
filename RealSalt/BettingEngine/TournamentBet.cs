using System;
using ChromiumConsole;
using ChromiumConsole.EventArguments;
using RealSalt.Data;

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

        public Tuple<string, int, SaltyConsole.Players> PlaceBet(MatchStartEventArgs matchArgs)
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
            }
            else
            {
                betSymbol = "~";
                betCharacter = SaltyConsole.Players.RedPlayer;
                if (IsHeads())
                {
                    betCharacter = SaltyConsole.Players.BluePlayer;
                }
            }

            return new Tuple<string, int, SaltyConsole.Players>(betSymbol, betSalt, betCharacter);
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
