using System;
using System.Threading.Tasks;
using Mensch.Id.API.Models.ExternalServices;
using Mensch.Id.API.Storage;

namespace Mensch.Id.API.Workflow.ExternalServices;

public class ExternalServiceAuthenticator
{
    private readonly IReadonlyStore<ExternalService, Guid> serviceStore;
    private readonly ExternalServiceClientRegistrar clientRegistrar;

    public ExternalServiceAuthenticator(
        IReadonlyStore<ExternalService, Guid> serviceStore,
        ExternalServiceClientRegistrar clientRegistrar)
    {
        this.serviceStore = serviceStore;
        this.clientRegistrar = clientRegistrar;
    }

    public async Task<ExternalService> Authenticate(ExternalServiceCredentials credentials)
    {
        if (credentials == null)
            return null;
        var service = await GetActiveServiceByClientId(credentials.ClientId);
        if (service == null)
            return null;
        var isClientSecretValid = await clientRegistrar.IsClientSecretValid(credentials.ClientId, credentials.ClientSecret);
        return isClientSecretValid ? service : null;
    }

    public async Task<ExternalService> GetActiveServiceByClientId(Guid clientId)
    {
        var service = await serviceStore.FirstOrDefaultAsync(x => x.ClientId == clientId);
        if (service == null || service.Status != ExternalServiceStatus.Active)
            return null;
        return service;
    }
}
