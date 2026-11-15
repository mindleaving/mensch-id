using System.Collections.Generic;
using Mensch.Id.API.AccessControl;
using Mensch.Id.API.Models;
using Mensch.Id.API.Workflow.ViewModelBuilders;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Mensch.Id.API.Test.Workflow;

public class LoginProviderViewModelBuilderTest
{
    public const string ClientSecret = "SuperSecretClientSecretValue";
    public const string ClientId = "VerySpecificClientIdValue";
    public const string Authority = "https://authority.example.org/tenant";

    public static List<OidcProviderSettings> CreateProviders()
    {
        return
        [
            new OidcProviderSettings
            {
                Id = "google",
                DisplayName = "Google",
                Authority = Authority,
                ClientId = ClientId,
                ClientSecret = ClientSecret
            },
            new OidcProviderSettings
            {
                Id = "eidas",
                Authority = Authority + "2",
                ClientId = ClientId + "2",
                ClientSecret = ClientSecret + "2"
            }
        ];
    }

    [Test]
    public void BuildsViewModelForEachProvider()
    {
        var registry = new OidcProviderRegistry(CreateProviders());
        var builder = new LoginProviderViewModelBuilder(registry);

        var viewModels = builder.BuildAll();

        Assert.That(viewModels, Has.Count.EqualTo(2));
        Assert.That(viewModels[0].Id, Is.EqualTo(new LoginProvider("google")));
        Assert.That(viewModels[0].DisplayName, Is.EqualTo("Google"));
        Assert.That(viewModels[1].Id, Is.EqualTo(new LoginProvider("eidas")));
        Assert.That(viewModels[1].DisplayName, Is.EqualTo("eidas"));
    }

    [Test]
    public void SerializedViewModelsDoNotExposeSecretsClientIdsOrAuthorities()
    {
        var registry = new OidcProviderRegistry(CreateProviders());
        var builder = new LoginProviderViewModelBuilder(registry);

        var viewModels = builder.BuildAll();
        var json = JsonConvert.SerializeObject(viewModels);

        Assert.That(json, Does.Not.Contain(ClientSecret));
        Assert.That(json, Does.Not.Contain(ClientId));
        Assert.That(json, Does.Not.Contain("authority.example.org"));
        Assert.That(json, Does.Not.Contain("Secret").IgnoreCase);
        Assert.That(json, Does.Not.Contain("Authority").IgnoreCase);
        Assert.That(json, Does.Not.Contain("ClientId").IgnoreCase);
    }

    [Test]
    public void ViewModelOnlyHasIdAndDisplayNameProperties()
    {
        var propertyNames = new List<string>();
        foreach (var property in typeof(Mensch.Id.API.ViewModels.LoginProviderViewModel).GetProperties())
        {
            propertyNames.Add(property.Name);
        }

        Assert.That(propertyNames, Is.EquivalentTo(new[] { "Id", "DisplayName" }));
    }
}
