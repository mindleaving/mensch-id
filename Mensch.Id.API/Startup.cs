using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Mensch.Id.API.Setups;

namespace Mensch.Id.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var setups = new ISetup[]
            {
                new StoreSetup(),
                new ControllerSetup(),
                new AccessControlSetup(),
                new CorsSetup(),
                new OpenApiSetup(),
                new ViewModelSetup()
            };
            foreach (var setup in setups)
            {
                setup.Run(services, Configuration);
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                TypescriptGeneratorRunner.Run();
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mensch.Id.API v1"));

            app.UseRouting();
            if (env.IsDevelopment())
            {
                app.UseCors();
            }
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
