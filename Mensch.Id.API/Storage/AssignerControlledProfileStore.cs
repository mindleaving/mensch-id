using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Mensch.Id.API.Models;
using Mensch.Id.API.Models.RequestParameters;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Mensch.Id.API.Storage;

public class AssignerControlledProfileStore : GenericStore<AssignerControlledProfile, MenschId>, IAssignerControlledProfileStore
{
    private const string IdFieldName = "_id";
    private const string BirthDateFormat = "yyyyMMdd";

    public AssignerControlledProfileStore(IMongoDatabase mongoDatabase)
        : base(mongoDatabase)
    {
    }

    public IAsyncEnumerable<AssignerControlledProfile> SearchAssignedProfiles(
        string assignerAccountId,
        AssignedProfilesRequestParameters parameters)
    {
        var filter = BuildFilter(assignerAccountId, parameters);
        var sort = BuildSort(parameters);
        return collection
            .Find(filter)
            .Sort(sort)
            .Skip(parameters.Skip)
            .Limit(parameters.Count)
            .ToAsyncEnumerable();
    }

    public Task<long> CountForAssigner(string assignerAccountId)
    {
        return collection.CountDocumentsAsync(x => x.AssignerAccountId == assignerAccountId);
    }

    private static FilterDefinition<AssignerControlledProfile> BuildFilter(
        string assignerAccountId,
        AssignedProfilesRequestParameters parameters)
    {
        var filterBuilder = Builders<AssignerControlledProfile>.Filter;
        var filters = new List<FilterDefinition<AssignerControlledProfile>>
        {
            filterBuilder.Eq(x => x.AssignerAccountId, assignerAccountId)
        };
        if (!string.IsNullOrWhiteSpace(parameters.SearchText))
        {
            var searchPattern = new BsonRegularExpression(Regex.Escape(parameters.SearchText.Trim()), "i");
            filters.Add(new BsonDocument(IdFieldName, searchPattern));
        }
        if (parameters.CreationTimeRangeStart.HasValue)
            filters.Add(filterBuilder.Gte(x => x.CreationDate, parameters.CreationTimeRangeStart.Value));
        if (parameters.CreationTimeRangeEnd.HasValue)
            filters.Add(filterBuilder.Lt(x => x.CreationDate, parameters.CreationTimeRangeEnd.Value));
        if (parameters.BirthDateTimeRangeStart.HasValue)
        {
            var birthDateStart = parameters.BirthDateTimeRangeStart.Value.ToString(BirthDateFormat);
            filters.Add(new BsonDocument(IdFieldName, new BsonDocument("$gte", birthDateStart)));
        }
        if (parameters.BirthDateTimeRangeEnd.HasValue)
        {
            var birthDateEnd = parameters.BirthDateTimeRangeEnd.Value.AddDays(1).ToString(BirthDateFormat);
            filters.Add(new BsonDocument(IdFieldName, new BsonDocument("$lt", birthDateEnd)));
        }
        return filterBuilder.And(filters);
    }

    private static SortDefinition<AssignerControlledProfile> BuildSort(AssignedProfilesRequestParameters parameters)
    {
        var sortBuilder = Builders<AssignerControlledProfile>.Sort;
        var isAscending = parameters.OrderDirection == OrderDirection.Ascending;
        if (parameters.OrderBy?.ToLower() == "id")
            return isAscending ? sortBuilder.Ascending(IdFieldName) : sortBuilder.Descending(IdFieldName);
        return isAscending ? sortBuilder.Ascending(x => x.CreationDate) : sortBuilder.Descending(x => x.CreationDate);
    }
}
