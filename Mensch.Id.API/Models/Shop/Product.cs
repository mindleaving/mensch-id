using System.ComponentModel.DataAnnotations;
using TypescriptGenerator.Attributes;

namespace Mensch.Id.API.Models.Shop;

public class Product : IId
{
    public Product() { }
    public Product(
        string id,
        string name,
        Money price,
        string category,
        string imageUrl = null,
        string downloadLink = null)
    {
        Id = id;
        Name = name;
        Price = price;
        Category = category;
        ImageUrl = imageUrl;
        DownloadLink = downloadLink;
    }

    [Required]
    public string Id { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    public Money Price { get; set; }

    [TypescriptIsOptional]
    public string ImageUrl { get; set; }

    public string Category { get; set; }

    [TypescriptIsOptional]
    public string DownloadLink { get; set; }
}