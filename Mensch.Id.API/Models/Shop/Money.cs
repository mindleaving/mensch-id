using System.ComponentModel.DataAnnotations;

namespace Mensch.Id.API.Models.Shop;

public class Money
{
    public Money() { }

    public Money(
        Currency currency,
        double value)
    {
        Currency = currency;
        Value = value;
    }
    public Money(
        double value,
        Currency currency)
    {
        Value = value;
        Currency = currency;
    }

    [Required]
    public Currency Currency { get; set; }

    [Required]
    [Range(0,double.MaxValue)]
    public double Value { get; set; }
}