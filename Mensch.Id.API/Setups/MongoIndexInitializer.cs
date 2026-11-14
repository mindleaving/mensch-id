using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mensch.Id.API.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Mensch.Id.API.Setups;

public class MongoIndexInitializer : IHostedService
{
    public static readonly TimeSpan ChallengeLifetime = TimeSpan.FromMinutes(5);
    public const string ChallengeExpiryIndexName = "ChallengeExpiry";
    private readonly IMongoDatabase database;
    private readonly ILogger<MongoIndexInitializer> logger;

    public MongoIndexInitializer(
        IMongoDatabase database,
        ILogger<MongoIndexInitializer> logger)
    {
        this.database = database;
        this.logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await EnsureChallengeExpiryIndex(cancellationToken);
        }
        catch (MongoException exception)
        {
            logger.LogError(exception, "Could not create the expiry index for challenges. Challenges will not expire automatically");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task EnsureChallengeExpiryIndex(CancellationToken cancellationToken)
    {
        var collection = database.GetCollection<MenschIdChallenge>(nameof(MenschIdChallenge));
        var timestampField = nameof(MenschIdChallenge.CreatedTimestamp);
        using var cursor = await collection.Indexes.ListAsync(cancellationToken);
        var indexes = await cursor.ToListAsync(cancellationToken);
        var existingIndex = indexes.FirstOrDefault(x => IsSingleFieldIndexOn(x, timestampField));
        if (existingIndex != null)
        {
            if (HasExpectedExpiry(existingIndex))
                return;
            var existingIndexName = existingIndex["name"].AsString;
            logger.LogInformation("Replacing index {IndexName} on challenges to set an expiry of {Lifetime}", existingIndexName, ChallengeLifetime);
            await collection.Indexes.DropOneAsync(existingIndexName, cancellationToken);
        }
        var indexKeys = Builders<MenschIdChallenge>.IndexKeys.Ascending(x => x.CreatedTimestamp);
        var indexOptions = new CreateIndexOptions
        {
            Name = ChallengeExpiryIndexName,
            ExpireAfter = ChallengeLifetime
        };
        var indexModel = new CreateIndexModel<MenschIdChallenge>(indexKeys, indexOptions);
        await collection.Indexes.CreateOneAsync(indexModel, cancellationToken: cancellationToken);
    }

    private static bool IsSingleFieldIndexOn(
        BsonDocument index,
        string fieldName)
    {
        var keys = index.GetValue("key", new BsonDocument()).AsBsonDocument;
        return keys.ElementCount == 1 && keys.Names.Single() == fieldName;
    }

    private static bool HasExpectedExpiry(BsonDocument index)
    {
        var expiry = index.GetValue("expireAfterSeconds", BsonNull.Value);
        return expiry.IsNumeric && Math.Abs(expiry.ToDouble() - ChallengeLifetime.TotalSeconds) < 0.5;
    }
}
