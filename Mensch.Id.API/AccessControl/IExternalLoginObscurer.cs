namespace Mensch.Id.API.AccessControl;

public interface IExternalLoginObscurer
{
    string Obscure(string accountId);
}