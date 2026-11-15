using System.Collections.Generic;
using System.Linq;
using Mensch.Id.API.AccessControl;
using Mensch.Id.API.Models;
using Mensch.Id.API.ViewModels;

namespace Mensch.Id.API.Workflow.ViewModelBuilders;

public class LoginProviderViewModelBuilder
{
    private readonly IOidcProviderRegistry providerRegistry;

    public LoginProviderViewModelBuilder(IOidcProviderRegistry providerRegistry)
    {
        this.providerRegistry = providerRegistry;
    }

    public List<LoginProviderViewModel> BuildAll()
    {
        return providerRegistry.Providers
            .Select(Build)
            .ToList();
    }

    private static LoginProviderViewModel Build(OidcProviderSettings provider)
    {
        var loginProvider = new LoginProvider(provider.Id);
        var displayName = string.IsNullOrWhiteSpace(provider.DisplayName) ? provider.Id : provider.DisplayName;
        return new LoginProviderViewModel(loginProvider, displayName);
    }
}
