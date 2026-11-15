using Mensch.Id.API.Models;

namespace Mensch.Id.API.ViewModels;

public class AdminAssignerViewModel
{
    public string Id { get; set; }
    public string Username { get; set; }
    public string Name { get; set; }
    public Contact Contact { get; set; }
    public string LogoId { get; set; }
    public bool IsActivated { get; set; }
    public long AssignedProfileCount { get; set; }
}
