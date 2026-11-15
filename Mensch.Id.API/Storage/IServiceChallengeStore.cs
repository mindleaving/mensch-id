using System;
using System.Threading.Tasks;
using Mensch.Id.API.Models.ExternalServices;

namespace Mensch.Id.API.Storage;

public interface IServiceChallengeStore : IStore<ServiceChallenge>
{
    Task<bool> TryAnswer(
        string challengeId,
        string menschId,
        ServiceChallengeState answer,
        DateTime answeredTimestamp,
        string resultToken);
}
