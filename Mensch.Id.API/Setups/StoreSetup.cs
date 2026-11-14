using System;
using Mensch.Id.API.Models;
using Mensch.Id.API.Models.ExternalServices;
using Mensch.Id.API.Models.AccessControl;
using Mensch.Id.API.Models.Shop;
using Mensch.Id.API.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace Mensch.Id.API.Setups
{
    public class StoreSetup : ISetup
    {
        public void Run(IServiceCollection services, IConfiguration configuration)
        {
            SetupMongoDB(services, configuration);
            SetupStores(services, configuration);
        }

        private void SetupMongoDB(IServiceCollection services, IConfiguration configuration)
        {
            ConventionRegistry.Register("EnumStringConvetion", new ConventionPack
            {
                new EnumRepresentationConvention(BsonType.String)
            }, type => true);
            BsonSerializer.TryRegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
            //BsonSerializer.RegisterSerializer(new DateOnlySerializer()); // Necessary, now that MongoDB supports DateOnly?
            var mongoSettings = new MongoClientSettings
            {
                Server = MongoServerAddress.Parse(configuration["MongoDB:Server"] ?? "localhost:27017")
            };
            var replicaSetName = configuration["MongoDB:ReplicaSetName"];
            if (!string.IsNullOrWhiteSpace(replicaSetName))
                mongoSettings.ReplicaSetName = replicaSetName;
            var user = configuration["MongoDB:User"];
            if (!string.IsNullOrWhiteSpace(user))
                mongoSettings.Credential = MongoCredential.CreateCredential("admin", user, configuration["MongoDB:Password"]);
            services.AddSingleton<IMongoClient>(new MongoClient(mongoSettings));
            services.AddSingleton<IMongoDatabase>(
                provider =>
                {
                    var databaseName = configuration["MongoDB:DatabaseName"];
                    return provider.GetService<IMongoClient>().GetDatabase(databaseName);
                });
        }

        private static void SetupStores(
            IServiceCollection services,
            IConfiguration configuration)
        {
            SetupTypeStores<Account>(services);
            services.AddScoped<IAccountStore, AccountStore>();
            SetupTypeStores<AssignerAccountRequest, Guid>(services);
            services.AddScoped<IAssignerControlledProfileStore, AssignerControlledProfileStore>();
            services.AddScoped<IStore<AssignerControlledProfile, MenschId>>(provider => provider.GetRequiredService<IAssignerControlledProfileStore>());
            services.AddScoped<IReadonlyStore<AssignerControlledProfile, MenschId>>(provider => provider.GetRequiredService<IAssignerControlledProfileStore>());
            services.Configure<FileStoreSettings>(configuration.GetSection(FileStoreSettings.AppSettingsSectionName));
            services.AddScoped<IFilesStore, FilesStore>();
            services.AddScoped<IIdStore, IdStore>();
            SetupTypeStores<MenschIdChallenge>(services);
            SetupTypeStores<MenschIdReservation>(services);
            SetupTypeStores<Order>(services);
            SetupTypeStores<Person>(services);
            SetupTypeStores<Product>(services);
            SetupTypeStores<Verification>(services);
            SetupTypeStores<ExternalService, Guid>(services);
            SetupTypeStores<WebSession, Guid>(services);
            services.AddScoped<IServiceChallengeStore, ServiceChallengeStore>();
            services.AddScoped<IStore<ServiceChallenge>>(provider => provider.GetRequiredService<IServiceChallengeStore>());
            services.AddScoped<IReadonlyStore<ServiceChallenge>>(provider => provider.GetRequiredService<IServiceChallengeStore>());
            services.AddHostedService<MongoIndexInitializer>();
        }

        private static void SetupTypeStores<T, TKey>(IServiceCollection services) where T : IId<TKey>
        {
            services.AddScoped<IReadonlyStore<T, TKey>, GenericReadonlyStore<T, TKey>>();
            services.AddScoped<IStore<T, TKey>, GenericStore<T, TKey>>();
        }

        private static void SetupTypeStores<T>(IServiceCollection services) where T: IId
        {
            services.AddScoped<IReadonlyStore<T>, GenericReadonlyStore<T>>();
            services.AddScoped<IStore<T>, GenericStore<T>>();
        }
    }
}
