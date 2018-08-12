using System;
using ChromiumConsole;
using ChromiumConsole.EventArguments;

namespace RealSalt.BettingEngine
{
    public class RandomBet : IBettingEngine
    {
        private static Random _genie;

        public RandomBet()
        {
            _genie = new Random();
        }

        public Tuple<string, int, SaltyConsole.Players> PlaceBet(MatchStartEventArgs matchArgs)
        {
            var betAmount = BaseBetAmount(matchArgs.Salt);
            var player = SaltyConsole.Players.RedPlayer;

            if (IsHeads())
            {
                player = SaltyConsole.Players.BluePlayer;
            }

            return new Tuple<string, int, SaltyConsole.Players>("~", betAmount, player);
        }

        private  int BaseBetAmount(int salt)
        {
            var digits = Math.Floor(Math.Log10(salt) + 1);
            var targetDigits = (int)digits - 3;

            return (int) Math.Pow(10, targetDigits);
        }

        private  bool IsHeads()
        {
            var result = _genie.Next(0, 2);

            if (result == 1)
                return true;

            return false;
        }
    }
}
