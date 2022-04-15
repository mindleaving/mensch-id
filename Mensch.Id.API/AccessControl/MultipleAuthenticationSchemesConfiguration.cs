using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;

namespace Mensch.Id.API.AccessControl
{
    public static class MultipleAuthenticationSchemesConfiguration
    {
        public static void AddMultiIdentityAuthentication(
            this IApplicationBuilder app)
        {
            app.Use(
                async (
                    context,
                    next) =>
                {
                    var principal = new ClaimsPrincipal();
                    var jwtIdentity = await context.AuthenticateAsync(JwtBearerDefaults.AuthenticationScheme);
                    if (jwtIdentity.Succeeded && jwtIdentity.Principal != null)
                    {
                        principal.AddIdentities(jwtIdentity.Principal.Identities);
                    }

                    var cookieIdentity = await context.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    if (cookieIdentity.Succeeded && cookieIdentity.Principal != null)
                    {
                        principal.AddIdentities(cookieIdentity.Principal.Identities);
                    }

                    context.User = principal;

                    await next();
                });
        }
    }
}
