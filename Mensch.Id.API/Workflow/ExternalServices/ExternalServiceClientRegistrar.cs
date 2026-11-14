using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Mensch.Id.API.Models.ExternalServices;
using Microsoft.AspNetCore.WebUtilities;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Mensch.Id.API.Workflow.ExternalServices;

public class ExternalServiceClientRegistrar
{
    private const int ClientSecretByteCount = 32;
    private readonly IOpenIddictApplicationManager applicationManager;

    public ExternalServiceClientRegistrar(IOpenIddictApplicationManager applicationManager)
    {
        this.applicationManager = applicationManager;
    }

    public async Task<string> RegisterWithNewSecret(ExternalService service)
    {
        var clientSecret = GenerateClientSecret();
        var descriptor = BuildDescriptor(service, clientSecret);
        var clientId = service.ClientId.ToString();
        var existingApplication = await applicationManager.FindByClientIdAsync(clientId);
        if (existingApplication == null)
            await applicationManager.CreateAsync(descriptor);
        else
            await applicationManager.UpdateAsync(existingApplication, descriptor);
        return clientSecret;
    }

    public async Task Unregister(ExternalService service)
    {
        var clientId = service.ClientId.ToString();
        var existingApplication = await applicationManager.FindByClientIdAsync(clientId);
        if (existingApplication == null)
            return;
        await applicationManager.DeleteAsync(existingApplication);
    }

    public async Task<bool> IsClientSecretValid(
        Guid clientId,
        string clientSecret)
    {
        if (string.IsNullOrEmpty(clientSecret))
            return false;
        var clientIdString = clientId.ToString();
        var application = await applicationManager.FindByClientIdAsync(clientIdString);
        if (application == null)
            return false;
        return await applicationManager.ValidateClientSecretAsync(application, clientSecret);
    }

    private static string GenerateClientSecret()
    {
        var secretBytes = RandomNumberGenerator.GetBytes(ClientSecretByteCount);
        return Base64UrlTextEncoder.Encode(secretBytes);
    }

    private static OpenIddictApplicationDescriptor BuildDescriptor(
        ExternalService service,
        string clientSecret)
    {
        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = service.ClientId.ToString(),
            ClientSecret = clientSecret,
            ClientType = ClientTypes.Confidential,
            DisplayName = service.DisplayName
        };
        descriptor.Permissions.UnionWith(
        [
            Permissions.Endpoints.Token,
            Permissions.GrantTypes.ClientCredentials
        ]);
        return descriptor;
    }
}
