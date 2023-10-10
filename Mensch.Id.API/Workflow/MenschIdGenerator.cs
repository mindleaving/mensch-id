using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Mensch.Id.API.Workflow
{
    public class MenschIdGenerator
    {
        private const string CharacterSpace = "ACDEFGHJKLMNPQRSTUVWXYZ2345679";
        private const int IdSuffixLength = 5;

        public string Generate(DateTime birthday)
        {
            var suffix = GenerateSuffix(IdSuffixLength);
            return $"{birthday:yyyyMMdd}-{suffix}";
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

        public static bool ValidateId(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return false;
            return Regex.IsMatch(id, $"[0-9]{{8}}-[{CharacterSpace}]{{{IdSuffixLength}}}");
        }
    }
}
