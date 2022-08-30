using System.Linq;
using System.Threading.Tasks;
using Mensch.Id.API.Helpers;
using Mensch.Id.API.Models;
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
            var accountType = ClaimsHelpers.GetAccountType(context.User.Claims.ToList());
            if(accountType == AccountType.Assigner)
                return Task.CompletedTask;
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
    public class RegularUserRequirement : IAuthorizationRequirement {}
}
