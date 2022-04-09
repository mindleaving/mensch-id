using System;
using System.Linq;

namespace Mensch.Id.API.Workflow
{
    public class MenschIdGenerator
    {
        private const string CharacterSpace = "ACDEFGHJKLMNPQRSTUVWXYZ2345679";

        public string Generate(DateTime birthday)
        {
            var suffix = GenerateSuffix(5);
            return $"{birthday:yyyyMMdd}-{suffix}";
        }

        public string GenerateAnonymous()
        {
            var suffix = GenerateSuffix(8);
            return $"{suffix[..3]}-{suffix[4..]}";
        }

        private string GenerateSuffix(int suffixLength)
        {
            var rng = new Random();
            return new string(
                Enumerable.Range(0, suffixLength)
                .Select(_ => CharacterSpace[rng.Next(CharacterSpace.Length)])
                .ToArray()
            );
        }
    }
}
