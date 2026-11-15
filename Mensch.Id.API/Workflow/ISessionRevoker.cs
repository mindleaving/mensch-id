using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mensch.Id.API.Workflow;

public interface ISessionRevoker
{
    Task RevokeAll(IReadOnlyCollection<string> accountIds);
}
