using System;
using System.Collections.Generic;
using System.IO;

namespace ITGlobal.DotNetPackageCacheGenerator
{
    public static class CsprojLocator
    {
        public static IEnumerable<string> Locate(string rootPath)
        {
            var projectFiles = new List<string>();
            FindProjectFiles(rootPath, projectFiles);
            return projectFiles;
        }

        private static void FindProjectFiles(string directory, List<string> projectFiles)
        {
            var directoryName = Path.GetFileName(directory);
            switch (directoryName)
            {
                case ".vs":
                case ".git":
                    Log.WriteLine($"Won't scan directory \"{directory}\" - it's in a skip-list");
                    return;
            }

            Log.WriteLine($"Scanning directory \"{directory}\"");

            foreach (var subDirectory in Directory.EnumerateDirectories(directory))
            {
                var name = Path.GetFileName(subDirectory);
                if (!string.Equals(name, "bin", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(name, "obj", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(name, "node_modules", StringComparison.OrdinalIgnoreCase))
                {
                    FindProjectFiles(subDirectory, projectFiles);
                }
            }

            foreach (var filepath in Directory.EnumerateFiles(directory, "*.csproj"))
            {
                Log.WriteLine($"Found project \"{filepath}\"");
                projectFiles.Add(filepath);
            }
        }
    }
}