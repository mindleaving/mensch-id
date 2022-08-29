using System;
using System.Collections.Generic;
using System.Security;
using System.Security.Cryptography;
using Mensch.Id.API.AccessControl;
using Mensch.Id.API.AccessControl.EventHandlers;
using Mensch.Id.API.Workflow;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Mensch.Id.API.Setups
{
    public class AccessControlSetup : ISetup
    {
        public void Run(
            IServiceCollection services,
            IConfiguration configuration)
        {
            SetupExternalLoginObscurer(services, configuration);
            var jwtPrivateKey = GetOrGenerateJwtPrivateKey(configuration);
            services.AddScoped<ISecurityTokenBuilder>(_ => new JwtSecurityTokenBuilder(jwtPrivateKey, TimeSpan.FromMinutes(60)));
            services.AddScoped<IAuthenticationModule, AuthenticationModule>();
            services.AddScoped<IProfileCreator, ProfileCreator>();
            services.AddScoped<GoogleAuthenticationEvents>();
            services.AddScoped<TwitterAuthenticationEvents>();
            services.AddScoped<FacebookAuthenticationEvents>();
            services.AddScoped<MicrosoftAuthenticationEvents>();

            services.AddAuthentication(
                    options =>
                    {
                        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    })
                .AddCookie()
                .AddJwtBearer(
                    options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = jwtPrivateKey,
                            ValidAudience = "mensch.ID",
                            ValidIssuer = "mensch.ID"
                        };
                    })
                .AddGoogle(
                    options =>
                    {
                        options.ClientId = configuration["Authentication:Google:ClientID"];
                        options.ClientSecret = configuration["Authentication:Google:ClientSecret"];
                        options.CallbackPath = "/api/accounts/external-login/google";
                        options.EventsType = typeof(GoogleAuthenticationEvents);
                    })
                .AddTwitter(
                    options =>
                    {
                        options.ConsumerKey = configuration["Authentication:Twitter:APIKey"];
                        options.ConsumerSecret = configuration["Authentication:Twitter:APISecret"];
                        options.CallbackPath = "/api/accounts/external-login/twitter";
                        options.EventsType = typeof(TwitterAuthenticationEvents);
                    })
                .AddFacebook(
                    options =>
                    {
                        options.AppId = configuration["Authentication:Facebook:AppID"];
                        options.AppSecret = configuration["Authentication:Facebook:AppSecret"];
                        options.CallbackPath = "/api/accounts/external-login/facebook";
                        options.EventsType = typeof(FacebookAuthenticationEvents);
                    })
                .AddMicrosoftAccount(
                    options =>
                    {
                        options.ClientId = configuration["Authentication:Microsoft:ClientId"];
                        options.ClientSecret = configuration["Authentication:Microsoft:ClientSecret"];
                        options.CallbackPath = "/api/accounts/external-login/microsoft";
                        options.EventsType = typeof(MicrosoftAuthenticationEvents);
                    });
        }

        private static void SetupExternalLoginObscurer(
            IServiceCollection services,
            IConfiguration configuration)
        {
            var obscureSalt = configuration["ExternalLogins:ObscureSalt"];
            if (string.IsNullOrWhiteSpace(obscureSalt))
                throw new SecurityException("No salt provided for obscuring external logins");
            var obscureSecret = configuration["ExternalLogins:SecretPrefix"];
            if (string.IsNullOrWhiteSpace(obscureSecret))
                throw new SecurityException("No secret provided for obscuring external logins");
            services.AddSingleton<IExternalLoginObscurer>(new ExternalLoginObscurer(obscureSalt, obscureSecret));
        }

        private SymmetricSecurityKey GetOrGenerateJwtPrivateKey(IConfiguration configuration)
        {
            SymmetricSecurityKey privateKey;
            try
            {
                privateKey = new SymmetricSecurityKey(Convert.FromBase64String(configuration["Authentication:Jwt:PrivateKey"]));
            }
            catch (KeyNotFoundException)
            {
                var bytes = RandomNumberGenerator.GetBytes(32);
                privateKey = new SymmetricSecurityKey(bytes);
                Console.WriteLine($"JWT private key candidate: {Convert.ToBase64String(bytes)}. Store this as environment variable 'Authentication__Jwt__PrivateKey'.");
            }

            return privateKey;
        }
    }
}
