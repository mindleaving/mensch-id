using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Commons;
using Mensch.Id.API.Storage;

namespace Mensch.Id.API.Workflow
{
    public class MenschIdGenerator
    {
        public const string CharacterSpace = "ACDEFGHJKLMNPQRSTUVWXYZ2345679";
        public const int IdSuffixLength = 3;

        public string Generate(DateTime birthday)
        {
            var suffix = GenerateSuffix(IdSuffixLength);
            return $"{birthday:yyyyMMdd}-{suffix}";
        }

        public async Task<string> FindFreeIdUsingIdStore(
            DateTime birthDate,
            IIdStore idStore)
        {
            var suffix = string.Empty;
            var suffixLettersTried = new List<char>();
            while (suffix.Length < IdSuffixLength)
            {
                var suffixLetterCandidates = CharacterSpace.Except(suffixLettersTried).ToList();
                var suffixLetterCandidate = suffixLetterCandidates[StaticRandom.Rng.Next(suffixLetterCandidates.Count)];
                var remainingSuffixLength = IdSuffixLength - suffix.Length - 1;
                var maximumIdCountInSubspace = (int)Math.Pow(CharacterSpace.Length, remainingSuffixLength);
                var actualIdCountInSubspace = await idStore.CountIdsMatching(new Regex($"^{birthDate:yyyyMMdd}-{suffix}{suffixLetterCandidate}"));
                if (actualIdCountInSubspace == 0)
                {
                    suffix += suffixLetterCandidate + GenerateSuffix(remainingSuffixLength);
                    break;
                }
                if (actualIdCountInSubspace >= maximumIdCountInSubspace)
                {
                    suffixLettersTried.Add(suffixLetterCandidate);
                    if (suffixLettersTried.Count == CharacterSpace.Length)
                        throw new Exception("ID space exhausted");
                    continue;
                }

                suffix += suffixLetterCandidate;
                suffixLettersTried = new List<char>();
            }

            return $"{birthDate:yyyyMMdd}-{suffix}";
        }

        public string GenerateSuffix(
            int suffixLength)
        {
            if (suffixLength == 0)
                return string.Empty;
            if (suffixLength < 0)
                throw new ArgumentOutOfRangeException(nameof(suffixLength), "Must be non-negative");
            return new string(Enumerable.Range(0, suffixLength)
                .Select(_ => CharacterSpace[StaticRandom.Rng.Next(CharacterSpace.Length)])
                .ToArray());
        }

        public static bool ValidateId(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return false;
            return Regex.IsMatch(id, $"[0-9]{{8}}-[{CharacterSpace}]{{{IdSuffixLength}}}");
        }
    }
}
