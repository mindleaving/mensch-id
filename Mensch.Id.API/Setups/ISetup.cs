using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Mensch.Id.API.Setups
{
    public interface ISetup
    {
        void Run(IServiceCollection services, IConfiguration configuration);
    }
}
