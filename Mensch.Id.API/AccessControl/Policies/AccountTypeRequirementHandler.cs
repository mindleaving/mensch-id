using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Mensch.Id.API.Helpers;
using Mensch.Id.API.Models.AccessControl;
using Mensch.Id.API.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Mensch.Id.API.AccessControl.Policies;

public class AccountTypeRequirementHandler
    : AuthorizationHandler<AccountTypeRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AccountTypeRequirement requirement)
    {
        var claims = context.User.Claims.ToList();
        if (!claims.Any())
            return;
        var accountType = ClaimsHelpers.GetAccountType(claims);
        if(accountType != requirement.AccountType)
            return;
        if (accountType == AccountType.Assigner)
        {
            var isActivated = await IsAssignerActivated(context, claims);
            if (!isActivated)
                return;
        }
        context.Succeed(requirement);
    }

    private static async Task<bool> IsAssignerActivated(
        AuthorizationHandlerContext context,
        List<Claim> claims)
    {
        if (context.Resource is not HttpContext httpContext)
            return false;
        var accountId = ClaimsHelpers.GetAccountId(claims);
        if (accountId == null)
            return false;
        var accountStore = httpContext.RequestServices.GetRequiredService<IAccountStore>();
        var account = await accountStore.GetByIdAsync(accountId);
        return account is AssignerAccount { IsActivated: true };
    }
}

public class AccountTypeRequirement : IAuthorizationRequirement
{
    public const string AdminPolicyName = nameof(AdminPolicyName);
    public const string AssignerPolicyName = nameof(AssignerPolicyName);
    public const string ExternalServicePolicyName = nameof(ExternalServicePolicyName);

    public AccountTypeRequirement(
        AccountType accountType)
    {
        AccountType = accountType;
    }

    public AccountType AccountType { get; }
}