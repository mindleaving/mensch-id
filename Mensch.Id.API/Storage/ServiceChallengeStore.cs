using System;
using System.Threading.Tasks;
using Mensch.Id.API.Models.ExternalServices;
using MongoDB.Driver;

namespace Mensch.Id.API.Storage;

public class ServiceChallengeStore : GenericStore<ServiceChallenge>, IServiceChallengeStore
{
    public ServiceChallengeStore(IMongoDatabase mongoDatabase)
        : base(mongoDatabase)
    {
    }

    public async Task<bool> TryAnswer(
        string challengeId,
        string menschId,
        ServiceChallengeState answer,
        DateTime answeredTimestamp,
        string resultToken)
    {
        var filter = Builders<ServiceChallenge>.Filter.Where(
            x => x.Id == challengeId
                 && x.MenschId == menschId
                 && x.State == ServiceChallengeState.Pending
                 && x.ExpiresAt > answeredTimestamp);
        var update = Builders<ServiceChallenge>.Update
            .Set(x => x.State, answer)
            .Set(x => x.AnsweredTimestamp, answeredTimestamp)
            .Set(x => x.ResultToken, resultToken);
        var result = await collection.UpdateOneAsync(filter, update);
        return result.ModifiedCount == 1;
    }
}
