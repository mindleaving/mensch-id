using System.ComponentModel.DataAnnotations;

namespace Mensch.Id.API.Models.Shop;

public class Product : IId
{
    [Required]
    public string Id { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    public Money Price { get; set; }

    public string Category { get; set; }
}