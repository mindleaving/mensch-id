using Mensch.Id.API.Models;

namespace Mensch.Id.API.ViewModels;

public class LoginProviderViewModel
{
    public LoginProviderViewModel(
        LoginProvider id,
        string displayName)
    {
        Id = id;
        DisplayName = displayName;
    }

    public LoginProvider Id { get; }
    public string DisplayName { get; }
}
