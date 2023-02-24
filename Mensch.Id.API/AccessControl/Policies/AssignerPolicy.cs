using System.Linq;
using System.Threading.Tasks;
using Mensch.Id.API.Helpers;
using Mensch.Id.API.Models;
using Microsoft.AspNetCore.Authorization;

namespace Mensch.Id.API.AccessControl.Policies
{
    public class AssignerPolicy : AuthorizationHandler<AssignerRequirement>
    {
        public const string PolicyName = nameof(AssignerPolicy);

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            AssignerRequirement requirement)
        {
            var claims = context.User.Claims.ToList();
            if (!claims.Any())
                return Task.CompletedTask;
            var accountType = ClaimsHelpers.GetAccountType(claims);
            if(accountType != AccountType.Assigner)
                return Task.CompletedTask;
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
    public class AssignerRequirement : IAuthorizationRequirement {}

}
