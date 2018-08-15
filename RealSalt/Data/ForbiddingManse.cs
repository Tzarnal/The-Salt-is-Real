using System;
using System.Data.Entity;
using System.Linq;
using Serilog;
using Serilog.Core;
using Serilog.Formatting.Json;

namespace RealSalt.Data
{
    public class ForbiddingManse : DbContext
    {
        public DbSet<Character> Characters { get; set; }        
        public DbSet<MatchRecord> Matches { get; set; }

        private Logger _scrollOfHeroes;


        public ForbiddingManse() : base("ForbiddingManse")
        {
            Database.SetInitializer(new ScrollofHeroesInitializer());

            _scrollOfHeroes = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.File(new JsonFormatter(),"Data/Scroll Of Heroes.log")
                .CreateLogger();
        }

        public void RegisterMatchResult(string winningCharacterName, string loosingCharacterName,MatchType type)
        {
            var winningCharacter = GetOrCreateCharacter(winningCharacterName);
            var loosingCharacter = GetOrCreateCharacter(loosingCharacterName);

            var matchRecord = new MatchRecord
            {
                WinnerCharacterId = winningCharacter.CharacterId,
                LoserCharacterId = loosingCharacter.CharacterId,

                WinnerSalt = -1,
                LoserSalt = -1,

                Tier = "Unknown",

                MatchStart = DateTime.Now,
                MatchLength = TimeSpan.FromSeconds(1),

                MatchType = type,
            };

            RegisterMatchResult(matchRecord);
        }

        public void RegisterMatchResult(MatchRecord matchRecord)
        {

            Matches.Add(matchRecord);

            _scrollOfHeroes.Information("{Winner} defeated {Loser}. {@MatchRecord}",
                GetCharacter(matchRecord.WinnerCharacterId).Name,
                GetCharacter(matchRecord.LoserCharacterId).Name,
                matchRecord);

            SaveChanges();
        }

        public Character GetCharacter(int characterId)
        {
            return Characters.First(c => c.CharacterId == characterId);
        }

        public Character GetOrCreateCharacter(string characterName)
        {
            if( Characters.Any( c => c.Name == characterName))
            {
                return Characters.First(c => c.Name == characterName);
            }

            var newCharacter = new Character {Name = characterName };
            Characters.Add(newCharacter);

            SaveChanges();

            return newCharacter;
        }

        public int GetWins(Character winner)
        {
            return Matches.Count(m => m.WinnerCharacterId == winner.CharacterId);
        }

        public int GetWins(Character winner, MatchType type)
        {
            return Matches.Count(m => m.WinnerCharacterId == winner.CharacterId && m.MatchType == type);
        }

        public int GetLosses(Character loser)
        {
            return Matches.Count(m => m.LoserCharacterId == loser.CharacterId);
        }

        public int GetLossess(Character loser, MatchType type)
        {
            return Matches.Count(m => m.LoserCharacterId == loser.CharacterId && m.MatchType == type);
        }
    }
}
