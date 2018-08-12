using System.Data.Entity;
using System.Linq;
using Serilog;
using Serilog.Core;

namespace RealSalt.Data
{
    public class ForbiddingManse : DbContext
    {
        public DbSet<Character> Characters { get; set; }
        private Logger _scrollOfHeroes;


        public ForbiddingManse() : base("ForbiddingManse")
        {
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<ForbiddingManse>());

            _scrollOfHeroes = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.File("Data/Scroll Of Heroes.log")
                .CreateLogger();
        }

        public void RegisterMatchResult(string winningCharacterName, string loosingCharacterName)
        {
            var winningCharacter = GetOrCreateCharacter(winningCharacterName);
            var loosingCharacter = GetOrCreateCharacter(loosingCharacterName);

            winningCharacter.Wins++;
            loosingCharacter.Losses++;

            _scrollOfHeroes.Information("Match: {winner} vs {loser}. {winner} won.",
                winningCharacter.Name,
                loosingCharacter.Name);

            SaveChanges();
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
    }
}
