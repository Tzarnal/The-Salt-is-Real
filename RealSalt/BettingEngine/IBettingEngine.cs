using ChromiumConsole;
using ChromiumConsole.EventArguments;

namespace RealSalt.BettingEngine
{
    public class BettingPlan
    {
        public string Symbol;
        public int Salt;
        public SaltyConsole.Players Character;
    }

    public interface IBettingEngine
    {
        BettingPlan PlaceBet(MatchStartEventArgs matchArgs);
    }
}
