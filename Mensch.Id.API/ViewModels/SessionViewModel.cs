using System;

namespace Mensch.Id.API.ViewModels;

public class SessionViewModel
{
    public string Id { get; set; }
    public SessionType Type { get; set; }
    public string ClientName { get; set; }
    public string UserAgent { get; set; }
    public DateTime? CreatedTimestamp { get; set; }
    public DateTime? LastActivityTimestamp { get; set; }
    public bool IsCurrent { get; set; }
}
