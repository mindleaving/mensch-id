using System;
using System.IO;
using System.Reflection;
using Mensch.Id.API.Models;
using Mensch.Id.API.ViewModels;
using TypescriptGenerator;

namespace Mensch.Id.API.Setups
{
    public static class TypescriptGeneratorRunner
    {
        public static void Run()
        {
            var repositoryPath = Constants.GetRepositoryPath();
            Run(repositoryPath);
        }

        public static void Run(string repositoryPath)
        {
            var outputDirectories = new[]
            {
                Path.Combine(repositoryPath, "mensch-id-frontend", "src", "localComponents", "types"),
                Path.Combine(repositoryPath, "mensch-id-app", "src", "types")
            };
            foreach (var outputDirectory in outputDirectories)
            {
                Generate(outputDirectory);
            }
        }

        private static void Generate(string outputDirectory)
        {
            Directory.CreateDirectory(outputDirectory);
            TypescriptGenerator.TypescriptGenerator.Builder
                .IncludeAllInNamespace(Assembly.GetAssembly(typeof(Person)), "Mensch.Id.API.Models")
                .IncludeAllInNamespace(Assembly.GetAssembly(typeof(NewProfileViewModel)), "Mensch.Id.API.ViewModels")
                .Exclude<LoginProvider>()
                .Exclude<MenschId>()
                .Exclude<OwnershipSecret>()
                .ReactDefaults()
                .SetDefaultFilenameForEnums("enums.ts")
                .ConfigureNamespace("Mensch.Id.API.Models", options => options.Translation = "Models")
                .ConfigureNamespace("Mensch.Id.API.ViewModels", options =>
                {
                    options.Translation = "ViewModels";
                    options.Filename = "viewModels.d.ts";
                })
                .CustomizeType(x => x == typeof(Guid), _ => "string")
                .CustomizeType(x => x == typeof(LoginProvider), _ => "string")
                .CustomizeType(x => x == typeof(MenschId) || x == typeof(MenschId?), _ => "string")
                .CustomizeType(x => x == typeof(OwnershipSecret) || x == typeof(OwnershipSecret?), _ => "string")
                .CustomizeType(x => x == typeof(DateTime), _ => "string")
                .SetOutputDirectory(outputDirectory)
                .Generate();
        }
    }
}
