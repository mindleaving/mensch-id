using Mensch.Id.API.Models;
using Mensch.Id.API.Models.RequestParameters;
using Mensch.Id.API.Models.Shop;
using Mensch.Id.API.Workflow.FilterExpressionBuilders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Mensch.Id.API.Setups
{
    public class FilterSetup : ISetup
    {
        public void Run(
            IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddScoped<IFilterExpressionBuilder<AssignerControlledProfile,AssignedProfilesRequestParameters>, AssignedProfileFillterExpressionBuilder>();
            services.AddScoped<IFilterExpressionBuilder<Product,ProductRequestParameters>, ProductFilterExpressionBuilder>();
        }
    }
}
