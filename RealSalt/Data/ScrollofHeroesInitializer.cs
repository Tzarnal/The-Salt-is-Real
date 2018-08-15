using System;
using System.Data.Entity;

namespace RealSalt.Data
{
    class ScrollofHeroesInitializer : DropCreateDatabaseIfModelChanges<ForbiddingManse>
    {
        protected override void Seed(ForbiddingManse context)
        {
            var goku = new Character {Name = "Forbidding Manse Goku"};
            var sailorMoon = new Character {Name = "Fobidding Manse Sailor Moon"};

            context.Characters.Add(goku);
            context.Characters.Add(sailorMoon);

            var record = new MatchRecord
            {
                WinnerCharacterId = goku.CharacterId,
                LoserCharacterId = sailorMoon.CharacterId,

                WinnerSalt = -1,
                LoserSalt = -1,

                WinnerBetCount = -1,
                LoserBetCount = -1,

                Tier = "Unknown",

                MatchStart = DateTime.Now,
                MatchLength = TimeSpan.FromSeconds(1),

                MatchType = MatchType.Exhibition,
            };

            context.Matches.Add(record);
        }
    }
}
