using System.Data.Entity;
using System.Linq;
using Serilog;

namespace RealSalt.Data
{
    public class ForbiddingManse : DbContext
    {
        public DbSet<Character> Characters { get; set; }

        public ForbiddingManse() : base("ForbiddingManse")
        {
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<ForbiddingManse>());
        }

        public void RegisterMatchResult(string winningCharacterName, string loosingCharacterName)
        {
            var winningCharacter = GetOrCreateCharacter(winningCharacterName);
            var loosingCharacter = GetOrCreateCharacter(loosingCharacterName);

            winningCharacter.Wins++;
            loosingCharacter.Losses++;

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
