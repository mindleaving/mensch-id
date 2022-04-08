using System;
using System.Linq;

namespace Mensch.Id.API.Workflow
{
    public class MenschIdGenerator
    {
        private const string CharacterSpace = "ACDEFGHJKLMNPQRSTUVWXYZ2345679";
        private const int SuffixLength = 5;

        public string Generate(DateTime birthday)
        {
            var suffix = GenerateSuffix();
            return $"{birthday:yyyyMMdd}-{suffix}";
        }

        private string GenerateSuffix()
        {
            var rng = new Random();
            return new string(
                Enumerable.Range(0, SuffixLength)
                .Select(_ => CharacterSpace[rng.Next(CharacterSpace.Length)])
                .ToArray()
            );
        }
    }
}
