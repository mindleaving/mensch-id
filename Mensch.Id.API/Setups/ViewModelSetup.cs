using Mensch.Id.API.Workflow.ViewModelBuilders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Mensch.Id.API.Setups
{
    public class ViewModelSetup : ISetup
    {
        public void Run(
            IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddScoped<NewProfileViewModelBuilder>();
            services.AddScoped<ProfileViewModelBuilder>();
        }
    }
}
