using System;
using System.Collections.Generic;
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
    private static readonly string[] TokenPermissions =
    [
        Permissions.Endpoints.Token,
        Permissions.GrantTypes.ClientCredentials
    ];
    private readonly IOpenIddictApplicationManager applicationManager;
    private readonly IOpenIddictTokenManager tokenManager;

    public ExternalServiceClientRegistrar(
        IOpenIddictApplicationManager applicationManager,
        IOpenIddictTokenManager tokenManager)
    {
        this.applicationManager = applicationManager;
        this.tokenManager = tokenManager;
    }

    public async Task<string> Register(ExternalService service)
    {
        var clientSecret = GenerateClientSecret();
        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = service.ClientId.ToString(),
            ClientSecret = clientSecret,
            ClientType = ClientTypes.Confidential,
            DisplayName = service.DisplayName
        };
        await applicationManager.CreateAsync(descriptor);
        return clientSecret;
    }

    public async Task Enable(ExternalService service)
    {
        var application = await FindApplication(service);
        var descriptor = await LoadDescriptor(application);
        descriptor.Permissions.UnionWith(TokenPermissions);
        await applicationManager.UpdateAsync(application, descriptor);
    }

    public async Task Disable(ExternalService service)
    {
        var application = await FindApplication(service);
        var descriptor = await LoadDescriptor(application);
        descriptor.Permissions.ExceptWith(TokenPermissions);
        await applicationManager.UpdateAsync(application, descriptor);
        var applicationId = await applicationManager.GetIdAsync(application);
        await RevokeTokens(applicationId);
    }

    public async Task<string> RotateSecret(ExternalService service)
    {
        var application = await FindApplication(service);
        var clientSecret = GenerateClientSecret();
        var descriptor = await LoadDescriptor(application);
        descriptor.ClientSecret = clientSecret;
        await applicationManager.UpdateAsync(application, descriptor);
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

    private async Task<object> FindApplication(ExternalService service)
    {
        var clientId = service.ClientId.ToString();
        return await applicationManager.FindByClientIdAsync(clientId)
               ?? throw new InvalidOperationException($"No client application is registered for service '{service.Id}'");
    }

    private async Task<OpenIddictApplicationDescriptor> LoadDescriptor(object application)
    {
        var descriptor = new OpenIddictApplicationDescriptor();
        await applicationManager.PopulateAsync(descriptor, application);
        return descriptor;
    }

    private async Task RevokeTokens(string applicationId)
    {
        var tokens = new List<object>();
        await foreach (var token in tokenManager.FindByApplicationIdAsync(applicationId))
        {
            tokens.Add(token);
        }
        foreach (var token in tokens)
        {
            await tokenManager.TryRevokeAsync(token);
        }
    }

    private static string GenerateClientSecret()
    {
        var secretBytes = RandomNumberGenerator.GetBytes(ClientSecretByteCount);
        return Base64UrlTextEncoder.Encode(secretBytes);
    }
}
