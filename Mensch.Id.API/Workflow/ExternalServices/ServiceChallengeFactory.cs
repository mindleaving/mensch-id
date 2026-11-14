using System;
using Mensch.Id.API.Models.ExternalServices;

namespace Mensch.Id.API.Workflow.ExternalServices;

public static class ServiceChallengeFactory
{
    public const int IdLength = 52;
    public const string AppLinkPrefix = "menschid://service-challenge/";
    public static readonly TimeSpan ChallengeLifetime = TimeSpan.FromMinutes(5);

    public static ServiceChallenge Create(
        Guid serviceId,
        string menschId,
        ServiceChallengeCreationRequest request,
        DateTime createdTimestamp)
    {
        return new ServiceChallenge
        {
            Id = ChallengeSecretGenerator.Generate(IdLength),
            ServiceId = serviceId,
            MenschId = menschId,
            Description = request.Description.Trim(),
            ServiceState = request.State,
            State = ServiceChallengeState.Pending,
            CreatedTimestamp = createdTimestamp,
            ExpiresAt = createdTimestamp.Add(ChallengeLifetime)
        };
    }

    public static bool IsValidId(string challengeId)
    {
        return ChallengeSecretGenerator.IsValid(challengeId, IdLength);
    }

    public static string BuildAppLink(string challengeId)
    {
        return AppLinkPrefix + challengeId;
    }
}
