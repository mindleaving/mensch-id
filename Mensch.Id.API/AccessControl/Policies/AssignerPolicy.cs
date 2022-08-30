using System;
using System.Security.Claims;
using System.Threading.Tasks;
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
            var accountTypeString = context.User.FindFirstValue(JwtSecurityTokenBuilder.AccountTypeClaimName);
            if(accountTypeString == null)
                return Task.CompletedTask;
            var accountType = Enum.Parse<AccountType>(accountTypeString);
            if(accountType != AccountType.Assigner)
                return Task.CompletedTask;
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
    public class AssignerRequirement : IAuthorizationRequirement {}

}
