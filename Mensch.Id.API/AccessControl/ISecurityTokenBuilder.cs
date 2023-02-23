using System.Collections.Generic;
using System.Security.Claims;

namespace Mensch.Id.API.AccessControl
{
    public interface ISecurityTokenBuilder
    {
        string Build(IEnumerable<Claim> claims);
    }
}