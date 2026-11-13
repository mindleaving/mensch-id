using System;
using System.Collections.Generic;

namespace Mensch.Id.API
{
    public static class Constants
    {
        public static readonly Dictionary<string, string> RepositoryPaths = new()
        {
            {"stationary-win8", @"F:\Projects\mensch.id"},
            {"ubuntu-stationary", @"/mnt/data/Projects/mensch.id"},
            {"nebulacube", @"/home/jan/git/mensch-id"},
            {"desktop-5ptb3ir", @"C:\Users\Jan\git\mensch-id"}
        };
        public static string GetRepositoryPath()
        {
            return RepositoryPaths[Environment.MachineName.ToLowerInvariant()];
        }
    }
}