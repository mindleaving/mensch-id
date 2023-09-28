using System.ComponentModel.DataAnnotations;

namespace Mensch.Id.API.Models.Shop;

public class Money
{
    [Required]
    public Currency Currency { get; set; }

    [Required]
    [Range(0,double.MaxValue)]
    public double Value { get; set; }
}