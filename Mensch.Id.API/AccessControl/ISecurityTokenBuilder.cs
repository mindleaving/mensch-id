using Mensch.Id.API.Models;

namespace Mensch.Id.API.AccessControl
{
    public interface ISecurityTokenBuilder
    {
        string BuildForLocalUser(LocalAccount account);
        string BuildForExternalLoginProvider(ExternalAccount account);
    }
}