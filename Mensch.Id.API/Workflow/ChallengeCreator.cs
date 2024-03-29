﻿using System;
using System.Threading.Tasks;
using Mensch.Id.API.Models;
using Mensch.Id.API.Storage;

namespace Mensch.Id.API.Workflow
{
    public class ChallengeCreator
    {
        private readonly IReadonlyStore<MenschIdChallenge> challengeStore;

        public ChallengeCreator(
            IReadonlyStore<MenschIdChallenge> challengeStore)
        {
            this.challengeStore = challengeStore;
        }

        public async Task<MenschIdChallenge> Create(
            string menschId,
            int challengeLength)
        {
            if (challengeLength < 1)
                throw new ArgumentOutOfRangeException(nameof(challengeLength), "Challenge length must be greater than 0");
            var challengeShortId = await GetUniqueChallengeIdForPerson(menschId);
            var challenge = new MenschIdChallenge
            {
                Id = Guid.NewGuid().ToString(),
                MenschId = menschId,
                ChallengeShortId = challengeShortId,
                ChallengeSecret = ChallengeSecretGenerator.Generate(challengeLength),
                CreatedTimestamp = DateTime.UtcNow
            };
            return challenge;
        }

        private async Task<string> GetUniqueChallengeIdForPerson(string menschId)
        {
            do
            {
                var candidate = ChallengeSecretGenerator.Generate();
                var existingChallenge = await challengeStore.FirstOrDefaultAsync(x => x.MenschId == menschId && x.ChallengeShortId == candidate);
                if (existingChallenge == null)
                    return candidate;
            } while (true);
        }
    }
}
