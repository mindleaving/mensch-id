using System;
using System.Linq;
using System.Net;
using System.Threading.RateLimiting;
using Mensch.Id.API.AccessControl;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Mensch.Id.API.Setups
{
    public class RateLimitingSetup : ISetup
    {
        public const string DefaultPolicy = "default";

        public void Run(
            IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddRateLimiter(
                rateLimiterOptions =>
                {
                    rateLimiterOptions.RejectionStatusCode = (int)HttpStatusCode.TooManyRequests;
                    rateLimiterOptions.AddPolicy(
                        DefaultPolicy,
                        httpContext =>
                        {
                            var personId = httpContext.User.Claims.FirstOrDefault(
                                claim => claim.Type == MenschIdClaimTypes.PersonIdClaimName && claim.Issuer == JwtSecurityTokenBuilder.Issuer)
                                ?.Value;
                            if (personId != null)
                            {
                                return RateLimitPartition.GetNoLimiter(personId);
                            }

                            return RateLimitPartition.GetTokenBucketLimiter(
                                string.Empty,
                                _ => new TokenBucketRateLimiterOptions
                                {
                                    AutoReplenishment = true,
                                    ReplenishmentPeriod = TimeSpan.FromSeconds(configuration.GetValue("RateLimiter:PeriodInSeconds", 15)),
                                    TokensPerPeriod = configuration.GetValue("RateLimiter:TokensPerPeriod", 10),
                                    TokenLimit = configuration.GetValue("RateLimiter:TokenLimit", 100),
                                    QueueLimit = 0,
                                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                                });
                        });
                });
        }
    }
}
