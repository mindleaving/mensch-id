using System.Linq;
using System.Threading.Tasks;
using Mensch.Id.API.Helpers;
using Mensch.Id.API.Models.AccessControl;
using Microsoft.AspNetCore.Authorization;

namespace Mensch.Id.API.AccessControl.Policies;

public class AccountTypeRequirementHandler
    : AuthorizationHandler<AccountTypeRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AccountTypeRequirement requirement)
    {
        var claims = context.User.Claims.ToList();
        if (!claims.Any())
            return Task.CompletedTask;
        var accountType = ClaimsHelpers.GetAccountType(claims);
        if(accountType != requirement.AccountType)
            return Task.CompletedTask;
        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}

public class AccountTypeRequirement : IAuthorizationRequirement
{
    public const string AdminPolicyName = nameof(AdminPolicyName);
    public const string AssignerPolicyName = nameof(AssignerPolicyName);

    public AccountTypeRequirement(
        AccountType accountType)
    {
        AccountType = accountType;
    }

    public AccountType AccountType { get; }
}