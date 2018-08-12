using System;
using ChromiumConsole;
using ChromiumConsole.EventArguments;


namespace RealSalt.BettingEngine
{
    public interface IBettingEngine
    {
        Tuple<string, int, SaltyConsole.Players > PlaceBet(MatchStartEventArgs matchArgs);
    }
}
