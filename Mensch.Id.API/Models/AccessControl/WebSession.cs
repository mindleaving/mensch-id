using System;

namespace Mensch.Id.API.Models.AccessControl;

public class WebSession : IId<Guid>
{
    public Guid Id { get; set; }
    public string AccountId { get; set; }
    public byte[] SerializedTicket { get; set; }
    public string UserAgent { get; set; }
    public DateTime CreatedTimestamp { get; set; }
    public DateTime LastActivityTimestamp { get; set; }
    public DateTime ExpiresAt { get; set; }
}
