using System;
using System.IO;
using System.Reflection;
using Mensch.Id.API.Models;
using TypescriptGenerator;

namespace Mensch.Id.API.Setups
{
    public static class TypescriptGeneratorRunner
    {
        public static void Run()
        {
            var repositoryPath = Constants.GetRepositoryPath();
            TypescriptGenerator.TypescriptGenerator.Builder
                .IncludeAllInNamespace(Assembly.GetAssembly(typeof(Person)), "Mensch.Id.API.Models")
                //.IncludeAllInNamespace(Assembly.GetAssembly(typeof(ResourceViewModel)), "JanKIS.API.ViewModels")
                .ReactDefaults()
                .ConfigureNamespace("Mensch.Id.API.Models", options => options.Translation = "Models")
                .ConfigureNamespace("Mensch.Id.API.ViewModels", options =>
                {
                    options.Translation = "ViewModels";
                    options.Filename = "viewModels.d.ts";
                })
                .CustomizeType(x => x == typeof(Guid), _ => "string")
                .SetOutputDirectory(Path.Combine(repositoryPath, "mensch-id-frontend", "src", "localComponents", "types"))
                .Generate();
        }
    }
}
