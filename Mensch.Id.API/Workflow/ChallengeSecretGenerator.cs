using System;
using System.Security.Cryptography;

namespace Mensch.Id.API.Workflow;

public static class ChallengeSecretGenerator
{
    private const string AllowedCharacters = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public static string Generate(int challengeLength = 6)
    {
        if (challengeLength <= 0)
            throw new ArgumentOutOfRangeException(nameof(challengeLength));
        return RandomNumberGenerator.GetString(AllowedCharacters, challengeLength);
    }
}
