using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mensch.Id.API.Models;
using Mensch.Id.API.Models.RequestParameters;
using Mensch.Id.API.Storage;

namespace Mensch.Id.API.Test.TestInfrastructure;

public class InMemoryAssignerControlledProfileStore : InMemoryStore<AssignerControlledProfile, MenschId>, IAssignerControlledProfileStore
{
    private const string BirthDateFormat = "yyyyMMdd";

    public async IAsyncEnumerable<AssignerControlledProfile> SearchAssignedProfiles(
        string assignerAccountId,
        AssignedProfilesRequestParameters parameters)
    {
        var matches = items.Values.Where(x => x.AssignerAccountId == assignerAccountId);
        if (!string.IsNullOrWhiteSpace(parameters.SearchText))
        {
            var searchText = parameters.SearchText.Trim();
            matches = matches.Where(x => x.Id.Value.Contains(searchText, System.StringComparison.OrdinalIgnoreCase));
        }
        if (parameters.CreationTimeRangeStart.HasValue)
            matches = matches.Where(x => x.CreationDate >= parameters.CreationTimeRangeStart.Value);
        if (parameters.CreationTimeRangeEnd.HasValue)
            matches = matches.Where(x => x.CreationDate < parameters.CreationTimeRangeEnd.Value);
        if (parameters.BirthDateTimeRangeStart.HasValue)
        {
            var birthDateStart = parameters.BirthDateTimeRangeStart.Value.ToString(BirthDateFormat);
            matches = matches.Where(x => string.CompareOrdinal(x.Id.Value, birthDateStart) >= 0);
        }
        if (parameters.BirthDateTimeRangeEnd.HasValue)
        {
            var birthDateEnd = parameters.BirthDateTimeRangeEnd.Value.AddDays(1).ToString(BirthDateFormat);
            matches = matches.Where(x => string.CompareOrdinal(x.Id.Value, birthDateEnd) < 0);
        }
        var isAscending = parameters.OrderDirection == OrderDirection.Ascending;
        var isOrderedById = parameters.OrderBy?.ToLower() == "id";
        matches = isOrderedById
            ? isAscending ? matches.OrderBy(x => x.Id.Value) : matches.OrderByDescending(x => x.Id.Value)
            : isAscending ? matches.OrderBy(x => x.CreationDate) : matches.OrderByDescending(x => x.CreationDate);
        matches = matches.Skip(parameters.Skip);
        if (parameters.Count.HasValue)
            matches = matches.Take(parameters.Count.Value);
        foreach (var match in matches.ToList())
        {
            yield return match;
        }
        await Task.CompletedTask;
    }

    public Task<long> CountForAssigner(string assignerAccountId)
    {
        var count = items.Values.LongCount(x => x.AssignerAccountId == assignerAccountId);
        return Task.FromResult(count);
    }
}
