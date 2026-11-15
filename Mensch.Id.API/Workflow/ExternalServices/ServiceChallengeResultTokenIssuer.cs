using System;
using System.Collections.Generic;
using System.Linq;
using Mensch.Id.API.AccessControl;
using Mensch.Id.API.Models.ExternalServices;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Server;

namespace Mensch.Id.API.Workflow.ExternalServices;

public class ServiceChallengeResultTokenIssuer
{
    public static readonly TimeSpan ResultTokenLifetime = TimeSpan.FromMinutes(5);
    private const string NonceClaimName = "nonce";
    private readonly IOptionsMonitor<OpenIddictServerOptions> serverOptions;

    public ServiceChallengeResultTokenIssuer(IOptionsMonitor<OpenIddictServerOptions> serverOptions)
    {
        this.serverOptions = serverOptions;
    }

    public string Issue(
        ServiceChallenge challenge,
        ExternalService service,
        ServiceChallengeState answer,
        DateTime answeredTimestamp)
    {
        var options = serverOptions.CurrentValue;
        var signingCredentials = options.SigningCredentials.First();
        var claims = BuildClaims(challenge, answer);
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = options.Issuer!.AbsoluteUri,
            Audience = service.ClientId.ToString(),
            IssuedAt = answeredTimestamp,
            NotBefore = answeredTimestamp,
            Expires = answeredTimestamp.Add(ResultTokenLifetime),
            Claims = claims,
            SigningCredentials = signingCredentials
        };
        var tokenHandler = new JsonWebTokenHandler();
        return tokenHandler.CreateToken(descriptor);
    }

    private static Dictionary<string, object> BuildClaims(
        ServiceChallenge challenge,
        ServiceChallengeState answer)
    {
        var claims = new Dictionary<string, object>
        {
            [MenschIdClaimTypes.Subject] = challenge.MenschId,
            [ServiceChallengeResultTokenClaims.ChallengeId] = challenge.Id,
            [ServiceChallengeResultTokenClaims.Result] = GetResultClaimValue(answer)
        };
        if (!string.IsNullOrEmpty(challenge.ServiceState))
            claims[NonceClaimName] = challenge.ServiceState;
        return claims;
    }

    private static string GetResultClaimValue(ServiceChallengeState answer)
    {
        return answer switch
        {
            ServiceChallengeState.Accepted => ServiceChallengeResultTokenClaims.Accepted,
            ServiceChallengeState.Rejected => ServiceChallengeResultTokenClaims.Rejected,
            _ => throw new ArgumentOutOfRangeException(nameof(answer), "Only accepted or rejected challenges have a result")
        };
    }
}
