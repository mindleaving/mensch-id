namespace Mensch.Id.API.Workflow.Models;

public class AccountToken
{
    public AccountToken(
        string token,
        string nonce)
    {
        Token = token;
        Nonce = nonce;
    }

    public string Token { get; }
    public string Nonce { get; }
}
