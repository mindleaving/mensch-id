using Mensch.Id.API.Workflow;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Mensch.Id.API.Setups
{
    public class MiscSetup : ISetup
    {
        public void Run(
            IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddScoped<ChallengeCreator>();
        }
    }
}
