using Mensch.Id.API.Models;

namespace Mensch.Id.API.AccessControl;

public interface IExternalLoginObscurer
{
    string Obscure(
        LoginProvider loginProvider,
        string externalId);
}
