using System.Collections.Generic;
using System.Threading.Tasks;
using Mensch.Id.API.Models;
using Mensch.Id.API.Models.RequestParameters;

namespace Mensch.Id.API.Storage;

public interface IAssignerControlledProfileStore : IStore<AssignerControlledProfile, MenschId>
{
    IAsyncEnumerable<AssignerControlledProfile> SearchAssignedProfiles(
        string assignerAccountId,
        AssignedProfilesRequestParameters parameters);
    Task<long> CountForAssigner(string assignerAccountId);
}
