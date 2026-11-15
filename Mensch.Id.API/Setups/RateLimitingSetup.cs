using System;
using System.Linq;
using System.Net;
using System.Threading.RateLimiting;
using Mensch.Id.API.AccessControl;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Mensch.Id.API.Setups;

public class RateLimitingSetup : ISetup
{
    public const string CredentialsPolicy = "credentials";
    private const string UnknownClient = "unknown-client";

    public void Run(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddRateLimiter(
            rateLimiterOptions =>
            {
                rateLimiterOptions.RejectionStatusCode = (int)HttpStatusCode.TooManyRequests;
                rateLimiterOptions.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
                    httpContext => CreateDefaultPartition(httpContext, configuration));
                rateLimiterOptions.AddPolicy(
                    CredentialsPolicy,
                    httpContext => CreateCredentialsPartition(httpContext, configuration));
            });
    }

    public static string GetClientAddress(HttpContext httpContext)
    {
        return httpContext.Connection.RemoteIpAddress?.ToString() ?? UnknownClient;
    }

    private static RateLimitPartition<string> CreateDefaultPartition(
        HttpContext httpContext,
        IConfiguration configuration)
    {
        var personId = httpContext.User.Claims
            .FirstOrDefault(claim => claim.Type == MenschIdClaimTypes.PersonIdClaimName)
            ?.Value;
        if (personId != null)
            return RateLimitPartition.GetNoLimiter($"person:{personId}");

        var clientAddress = GetClientAddress(httpContext);
        return RateLimitPartition.GetTokenBucketLimiter(
            $"address:{clientAddress}",
            _ => new TokenBucketRateLimiterOptions
            {
                AutoReplenishment = true,
                ReplenishmentPeriod = TimeSpan.FromSeconds(configuration.GetValue("RateLimiter:PeriodInSeconds", 15)),
                TokensPerPeriod = configuration.GetValue("RateLimiter:TokensPerPeriod", 10),
                TokenLimit = configuration.GetValue("RateLimiter:TokenLimit", 100),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            });
    }

    private static RateLimitPartition<string> CreateCredentialsPartition(
        HttpContext httpContext,
        IConfiguration configuration)
    {
        var clientAddress = GetClientAddress(httpContext);
        return RateLimitPartition.GetFixedWindowLimiter(
            $"credentials:{clientAddress}",
            _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                Window = TimeSpan.FromSeconds(configuration.GetValue("RateLimiter:Credentials:WindowInSeconds", 60)),
                PermitLimit = configuration.GetValue("RateLimiter:Credentials:PermitLimit", 10),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            });
    }
}
