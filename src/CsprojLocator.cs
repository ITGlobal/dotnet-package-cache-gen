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
            foreach (var subDirectory in Directory.EnumerateDirectories(directory))
            {
                var name = Path.GetFileName(subDirectory);
                if (!string.Equals(name, "bin", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(name, "obj", StringComparison.OrdinalIgnoreCase))
                {
                    FindProjectFiles(subDirectory, projectFiles);
                }
            }

            foreach (var filepath in Directory.EnumerateFiles(directory, "*.csproj"))
            {
                projectFiles.Add(filepath);
            }
        }
    }
}