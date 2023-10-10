using System.Linq;
using System.Threading.Tasks;
using Commons.Extensions;
using Mensch.Id.API.Helpers;
using Mensch.Id.API.Models;
using Mensch.Id.API.Models.AccessControl;
using Microsoft.AspNetCore.Authorization;

namespace Mensch.Id.API.AccessControl.Policies
{
    public class RegularUserPolicy : AuthorizationHandler<RegularUserRequirement>
    {
        public const string PolicyName = nameof(RegularUserPolicy);

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            RegularUserRequirement requirement)
        {
            var claims = context.User.Claims.ToList();
            if(!claims.Any())
                return Task.CompletedTask;
            var accountType = ClaimsHelpers.GetAccountType(claims);
            if(!accountType.InSet(AccountType.External, AccountType.Local, AccountType.LocalAnonymous))
                return Task.CompletedTask;
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
    public class RegularUserRequirement : IAuthorizationRequirement {}
}
