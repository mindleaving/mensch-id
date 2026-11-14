using System;

namespace Mensch.Id.API.Models.ExternalServices;

public class ExternalService : IId<Guid>
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public string DisplayName { get; set; }
    public string ContactInformation { get; set; }
    public string AcceptRedirectUri { get; set; }
    public string RejectRedirectUri { get; set; }
    public ExternalServiceStatus Status { get; set; }
    public DateTime CreatedTimestamp { get; set; }
    public DateTime? ActivatedTimestamp { get; set; }
    public string ActivatedBy { get; set; }
}
