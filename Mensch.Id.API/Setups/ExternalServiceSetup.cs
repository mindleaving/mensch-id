using System.Text.Json.Serialization;
using Mensch.Id.API.Hubs;
using Mensch.Id.API.Workflow.ExternalServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Mensch.Id.API.Setups;

public class ExternalServiceSetup : ISetup
{
    public void Run(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<ExternalServiceClientRegistrar>();
        services.AddScoped<ExternalServiceAuthenticator>();
        services.AddSingleton<ServiceChallengeResultTokenIssuer>();
        services.AddScoped<ServiceChallengeNotifier>();
        services.AddSignalR()
            .AddJsonProtocol(options => options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
    }
}
