using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Mensch.Id.API.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Mensch.Id.API.Tools
{
    public class LocalizationStringSearcher
    {
        private readonly string FrontendDirectory = Path.Combine(Constants.GetRepositoryPath(), "mensch-id-frontend");

        [Test]
        public void CreateLocalizationIdDictionary()
        {
            
            var localizationOutputFile = Path.Combine(FrontendDirectory, "src", "localComponents", "resources", "translation.en.json");
            var tsFiles = Directory.GetFiles(Path.Combine(FrontendDirectory, "src"), "*.ts?", SearchOption.AllDirectories);
            var existingLocalizations = JObject.Parse(File.ReadAllText(localizationOutputFile));
            var existingResourceIds = existingLocalizations.Properties().Select(x => x.Name);
            var enumResources = GetEnumResourceIds();
            var resourceIds = new List<string>(existingResourceIds.Concat(enumResources.Keys));
            foreach (var tsFile in tsFiles)
            {
                var fileContent = File.ReadAllLines(tsFile);
                foreach (var line in fileContent)
                {
                    var matches = Regex.Matches(line, "resolveText\\([\"'](?<ResourceID>[^\"']+)[\"'](,.+)?\\)");
                    foreach (Match match in matches)
                    {
                        if(!match.Groups["ResourceID"].Success)
                            continue;
                        var resourceId = match.Groups["ResourceID"].Value;
                        resourceIds.Add(resourceId);
                    }
                }
            }

            var resourceDictionary = new JObject();
            foreach (var resourceId in resourceIds.Distinct().OrderBy(x => x))
            {
                var existingValue = existingLocalizations[resourceId]?.Value<string>();
                if(!string.IsNullOrEmpty(existingValue))
                    resourceDictionary[resourceId] = existingValue;
                else if (enumResources.ContainsKey(resourceId))
                    resourceDictionary[resourceId] = enumResources[resourceId];
                else
                    resourceDictionary[resourceId] = string.Empty;
                Console.WriteLine($"{resourceId}: {resourceDictionary[resourceId]}");
            }

            resourceDictionary = SortDictionary(resourceDictionary);
            File.WriteAllText(localizationOutputFile, JsonConvert.SerializeObject(resourceDictionary, Formatting.Indented));
        }

        [Test]
        [TestCase("de")]
        public void CreateLocalizationsIdsInSecondaryDictionaries(string language)
        {
            var primaryLanguage = "en";
            var primaryDictionaryFile = Path.Combine(
                FrontendDirectory,
                "src", "localComponents", "resources",
                $"translation.{primaryLanguage}.json");
            var secondaryDictionaryFile = Path.Combine(
                FrontendDirectory,
                "src", "localComponents", "resources",
                $"translation.{language}.json");
            var primaryJObject = JObject.Parse(File.ReadAllText(primaryDictionaryFile));
            var secondaryJObject = JObject.Parse(File.ReadAllText(secondaryDictionaryFile));
            foreach (var resourceId in primaryJObject.Properties().Select(x => x.Name))
            {
                if(secondaryJObject.ContainsKey(resourceId))
                    continue;
                secondaryJObject[resourceId] = "";
            }

            secondaryJObject = SortDictionary(secondaryJObject);
            File.WriteAllText(secondaryDictionaryFile, JsonConvert.SerializeObject(secondaryJObject, Formatting.Indented));
        }

        private static Dictionary<string,string> GetEnumResourceIds()
        {
            var enumResources = Assembly.GetAssembly(typeof(Person)).GetExportedTypes()
                .Where(t => t.Namespace.StartsWith(typeof(Person).Namespace) && t.IsEnum)
                .SelectMany(t => Enum.GetNames(t).Select(x => new
                {
                    ResourceId = $"{t.Name}_{x}",
                    Value = x
                }))
                .ToDictionary(kvp => kvp.ResourceId, kvp => kvp.Value);
            return enumResources;
        }

        private JObject SortDictionary(
            JObject jObject)
        {
            var properties = jObject.Properties().OrderBy(x => x.Name);
            return new JObject(properties);
        }
    }
}