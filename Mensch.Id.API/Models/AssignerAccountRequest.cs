using System;
using System.ComponentModel.DataAnnotations;

namespace Mensch.Id.API.Models
{
    public class AssignerAccountRequest : IId<Guid>
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string ContactPersonName { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(254)]
        public string Email { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int ExpectedAssignmentsPerYear { get; set; }

        [MaxLength(2000)]
        public string Note { get; set; }
    }
}
