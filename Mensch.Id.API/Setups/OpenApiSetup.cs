using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mensch.Id.API.Models;
using Microsoft.OpenApi;

namespace Mensch.Id.API.Setups
{
    public class OpenApiSetup : ISetup
    {
        public void Run(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Mensch.Id.API", Version = "v1" });
                c.MapType<MenschId>(() => new OpenApiSchema { Type = JsonSchemaType.String, Pattern = "^[0-9]{8}-[ACDEFGHJKLMNPQRSTUVWXYZ2345679]{5}$" });
                c.MapType<OwnershipSecret>(() => new OpenApiSchema { Type = JsonSchemaType.String, Pattern = "^[ACDEFGHJKLMNPQRSTUVWXYZ2345679]{30}$" });
                c.MapType<LoginProvider>(() => new OpenApiSchema { Type = JsonSchemaType.String, Pattern = "^[a-zA-Z0-9]+$" });
            });
        }
    }
}
