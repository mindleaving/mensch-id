using System.ComponentModel.DataAnnotations;

namespace Mensch.Id.API.Models
{
    public class AssignerAccountRequest : IId
    {
        [Required]
        public string Id { get; set; }

        [Required]
        public string ContactPersonName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int ExpectedAssignmentsPerYear { get; set; }

        public string Note { get; set; }
    }
}
