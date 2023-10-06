using System;
using System.Collections.Generic;
using System.Security;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Mensch.Id.API.AccessControl;
using Mensch.Id.API.AccessControl.EventHandlers;
using Mensch.Id.API.AccessControl.Policies;
using Mensch.Id.API.Models.AccessControl;
using Mensch.Id.API.Workflow;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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

            services.AddSingleton<IClaimBuilder, ClaimBuilder>();
            var jwtPrivateKey = GetOrGenerateJwtPrivateKey(configuration);
            services.AddSingleton<ISecurityTokenBuilder>(_ => new JwtSecurityTokenBuilder(jwtPrivateKey));
            services.AddScoped<IAuthenticationModule, AuthenticationModule>();
            services.AddScoped<IAccountCreator, AccountCreator>();
            services.AddSingleton<IPasswordHasher<LocalAnonymousAccount>, PasswordHasher<LocalAnonymousAccount>>();
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
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = jwtPrivateKey,
                            ValidAlgorithms = new[] { "HS256" },
                            ValidTypes = new[] { "JWT" },
                            ValidAudience = JwtSecurityTokenBuilder.Audience,
                            ValidIssuer = JwtSecurityTokenBuilder.Issuer
                        };
                        // Events can be null
                        // ReSharper disable once ConstantNullCoalescingCondition
                        options.Events ??= new();
                        options.Events.OnMessageReceived = context => {

                            if (context.Request.Cookies.ContainsKey(JwtSecurityTokenBuilder.AccessTokenCookieName))
                            {
                                context.Token = context.Request.Cookies[JwtSecurityTokenBuilder.AccessTokenCookieName];
                            }

                            return Task.CompletedTask;
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

            services.AddSingleton<IAuthorizationHandler, AccountTypeRequirementHandler>();
            services.AddSingleton<IAuthorizationHandler, RegularUserPolicy>();
            services.AddAuthorization(
                options =>
                {
                    options.AddPolicy(AccountTypeRequirement.AssignerPolicyName, policy => policy.AddRequirements(new AccountTypeRequirement(AccountType.Assigner)));
                    options.AddPolicy(AccountTypeRequirement.AdminPolicyName, policy => policy.AddRequirements(new AccountTypeRequirement(AccountType.Admin)));
                    options.AddPolicy(RegularUserPolicy.PolicyName, policy => policy.AddRequirements(new RegularUserRequirement()));
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
