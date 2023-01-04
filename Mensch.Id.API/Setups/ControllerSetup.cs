using Mensch.Id.API.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Mensch.Id.API.Setups
{
    public class ControllerSetup : ISetup
    {
        public void Run(
            IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddControllers()
                .AddNewtonsoftJson(
                    options =>
                    {
                        options.SerializerSettings.Converters.Add(new StringEnumConverter());
                        options.SerializerSettings.Formatting = Formatting.None;
                    });;
            services.AddHttpContextAccessor();
            services.Configure<AccountInjectionSettings>(configuration.GetSection(AccountInjectionSettings.AppSettingsSectionName));
        }
    }
}
