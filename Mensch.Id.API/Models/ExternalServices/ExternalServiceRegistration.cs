using System.ComponentModel.DataAnnotations;

namespace Mensch.Id.API.Models.ExternalServices;

public class ExternalServiceRegistration
{
    [Required]
    [MaxLength(100)]
    public string DisplayName { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(254)]
    public string Email { get; set; }

    [MaxLength(1000)]
    public string ContactInformation { get; set; }
}
