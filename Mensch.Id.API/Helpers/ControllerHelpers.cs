using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Mensch.Id.API.AccessControl;
using Mensch.Id.API.Models;
using Mensch.Id.API.Models.AccessControl;
using Microsoft.AspNetCore.Http;

namespace Mensch.Id.API.Helpers
{
    public static class ControllerHelpers
    {
        public static string GetUsername(IHttpContextAccessor httpContextAccessor)
        {
            var username = httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;
            if (username == null)
                return "anonymous";
            return UsernameNormalizer.Normalize(username);
        }

        public static List<Claim> GetClaims(IHttpContextAccessor httpContextAccessor)
        {
            return httpContextAccessor.HttpContext?.User.Claims.ToList() ?? new List<Claim>();
        }

        public static AccountType? GetAccountType(
            IHttpContextAccessor httpContextAccessor)
        {
            var claims = httpContextAccessor.HttpContext?.User.Claims.ToList();
            return ClaimsHelpers.GetAccountType(claims);
        }

        public static string GetAccountId(
            IHttpContextAccessor httpContextAccessor)
        {
            return httpContextAccessor.HttpContext?.User.Claims
                .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
