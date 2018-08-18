using System;
using ChromiumConsole;
using ChromiumConsole.EventArguments;
using Serilog;

namespace RealSalt.BettingEngine
{
    public class RandomBet : IBettingEngine
    {
        private static Random _genie;

        public RandomBet()
        {
            _genie = new Random();
        }

        public BettingPlan PlaceBet(MatchStartEventArgs matchArgs)
        {
            var betAmount = BaseBetAmount(matchArgs.Salt);
            var player = SaltyConsole.Players.RedPlayer;

            if (IsHeads())
            {
                player = SaltyConsole.Players.BluePlayer;
            }

            Log.Verbose("Better - Random: Randomly picked {player}.",
                player.ToString());

            return new BettingPlan
            {
                Symbol = "~",
                Character = player,
                Salt = betAmount
            };
           
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
