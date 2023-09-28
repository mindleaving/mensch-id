using Mensch.Id.API.Models;
using Mensch.Id.API.Models.AccessControl;
using Mensch.Id.API.Models.Shop;
using Mensch.Id.API.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace Mensch.Id.API.Setups
{
    public class StoreSetup : ISetup
    {
        public void Run(IServiceCollection services, IConfiguration configuration)
        {
            SetupMongoDB(services, configuration);
            SetupStores(services);
        }

        private void SetupMongoDB(IServiceCollection services, IConfiguration configuration)
        {
            ConventionRegistry.Register("EnumStringConvetion", new ConventionPack
            {
                new EnumRepresentationConvention(BsonType.String)
            }, type => true);
            var mongoSettings = new MongoClientSettings
            {
                Credential = MongoCredential.CreateCredential("admin", configuration["MongoDB:User"], configuration["MongoDB:Password"])
            };
            services.AddSingleton<IMongoClient>(new MongoClient(mongoSettings));
            services.AddSingleton<IMongoDatabase>(
                provider =>
                {
                    var databaseName = configuration["MongoDB:DatabaseName"];
                    return provider.GetService<IMongoClient>().GetDatabase(databaseName);
                });
        }

        private static void SetupStores(IServiceCollection services)
        {
            SetupTypeStores<Account>(services);
            services.AddScoped<IAccountStore, AccountStore>();
            SetupTypeStores<AssignerAccountRequest>(services);
            SetupTypeStores<AssignerControlledProfile>(services);
            services.AddScoped<IIdStore, IdStore>();
            SetupTypeStores<MenschIdChallenge>(services);
            SetupTypeStores<MenschId>(services);
            SetupTypeStores<Order>(services);
            SetupTypeStores<Person>(services);
            SetupTypeStores<Product>(services);
            SetupTypeStores<Verification>(services);
        }

        private static void SetupTypeStores<T>(IServiceCollection services) where T: IId
        {
            services.AddScoped<IReadonlyStore<T>, GenericReadonlyStore<T>>();
            services.AddScoped<IStore<T>, GenericStore<T>>();
        }
    }
}
