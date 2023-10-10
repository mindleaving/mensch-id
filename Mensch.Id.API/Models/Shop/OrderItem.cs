using System.ComponentModel.DataAnnotations;

namespace Mensch.Id.API.Models.Shop;

public class OrderItem
{
    [Required]
    public Product Product { get; set; }

    [Required]
    [Range(1,int.MaxValue)]
    public int Amount { get; set; }
}