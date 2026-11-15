using System.Collections.Generic;
using System.Security.Claims;
using Mensch.Id.API.Models.AccessControl;

namespace Mensch.Id.API.AccessControl;

public interface IClaimBuilder
{
    List<Claim> Build(Account account);
    ClaimsPrincipal BuildPrincipal(
        Account account,
        string authenticationType);
}

public class ClaimBuilder : IClaimBuilder
{
    public List<Claim> Build(Account account)
    {
        var claims = new List<Claim>
        {
            new(MenschIdClaimTypes.Subject, account.Id),
            new(MenschIdClaimTypes.AccountTypeClaimName, account.AccountType.ToString())
        };
        if (account is PersonAccount { PersonId: not null } personAccount)
            claims.Add(new Claim(MenschIdClaimTypes.PersonIdClaimName, personAccount.PersonId));
        return claims;
    }

    public ClaimsPrincipal BuildPrincipal(
        Account account,
        string authenticationType)
    {
        var claims = Build(account);
        var identity = new ClaimsIdentity(
            claims,
            authenticationType,
            MenschIdClaimTypes.Subject,
            null);
        return new ClaimsPrincipal(identity);
    }
}
