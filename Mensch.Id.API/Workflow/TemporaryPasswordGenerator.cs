using System.Linq;

namespace Mensch.Id.API.Workflow
{
    public class TemporaryPasswordGenerator
    {
        public string AllowedCharacters { get; set; } = "abcdefghijkmnpqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ23456789!.,-_%&#?";

        public string Generate(int length = 16)
        {
            var chars = Enumerable.Range(0, length).Select(_ => GetRandomCharacter()).ToArray();
            return new string(chars);
        }

        private char GetRandomCharacter()
        {
            // Ideally a crypto-grade random number generate should be used,
            // but this is a temporary password that is printed on paper anyway
            return AllowedCharacters[StaticRandom.Rng.Next(AllowedCharacters.Length)];
        }
    }
}