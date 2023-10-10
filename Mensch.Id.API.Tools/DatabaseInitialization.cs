using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mensch.Id.API.Models.Shop;
using NUnit.Framework;

namespace Mensch.Id.API.Tools;

public class DatabaseInitialization : DatabaseAccess
{
    [Test]
    public async Task AddProducts()
    {
        var products = new List<Product>
        {
            // ID-cards
            new(
                Guid.NewGuid().ToString(),
                "Blank ID-name-cards (print template)",
                new(0, Currency.EUR),
                "Print",
                "/img/shop/mensch-id-handwritten-name-card.png",
                "/docs/blank-mensch-id-cards.pdf"),
            new(
                Guid.NewGuid().ToString(),
                "Blank ID-name-cards (100x)",
                new(2.5, Currency.EUR),
                "Print",
                "/img/shop/mensch-id-handwritten-name-card.png"),
            new(
                Guid.NewGuid().ToString(),
                "Blank ID-name-cards (250x)",
                new(6.25, Currency.EUR),
                "Print",
                "/img/shop/mensch-id-handwritten-name-card.png"),
            new(
                Guid.NewGuid().ToString(),
                "Blank ID-name-cards (500x)",
                new(12.5, Currency.EUR),
                "Print",
                "/img/shop/mensch-id-handwritten-name-card.png"),
            new(
                Guid.NewGuid().ToString(),
                "Blank ID-name-cards (1000x)",
                new(25.0, Currency.EUR),
                "Print",
                "/img/shop/mensch-id-handwritten-name-card.png"),
            new(
                Guid.NewGuid().ToString(),
                "Blank ID-name-cards (2500x)",
                new(62.5, Currency.EUR),
                "Print",
                "/img/shop/mensch-id-handwritten-name-card.png"),
            new(
                Guid.NewGuid().ToString(),
                "Blank ID-name-cards (5000x)",
                new(125, Currency.EUR),
                "Print",
                "/img/shop/mensch-id-handwritten-name-card.png"),

            // Name list
            new(
                Guid.NewGuid().ToString(),
                "Name-ID-list (print template)",
                new(0, Currency.EUR),
                "Print",
                "/img/shop/mensch-id-namelist.png",
                "/docs/mensch-id-namelist.pdf"),
            new(
                Guid.NewGuid().ToString(),
                "Name-ID-list (form template)",
                new(0, Currency.EUR),
                "Print",
                "/img/shop/mensch-id-namelist-form.png",
                "/docs/mensch-id-namelist-form.pdf"),
        };
        var productCollection = GetCollection<Product>();
        await productCollection.InsertManyAsync(products);
    }
}