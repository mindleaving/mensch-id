using System.ComponentModel.DataAnnotations;

namespace Mensch.Id.API.Models.ExternalServices;

public class ExternalServiceRegistration
{
    [Required]
    [MaxLength(100)]
    public string DisplayName { get; set; }

    [Required]
    [MaxLength(1000)]
    public string ContactInformation { get; set; }

    [Required]
    [MaxLength(2000)]
    public string AcceptRedirectUri { get; set; }

    [Required]
    [MaxLength(2000)]
    public string RejectRedirectUri { get; set; }
}
