using System;
using System.IO;

namespace ITGlobal.DotNetPackageCacheGenerator
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.Error.WriteLine("USAGE:");
                Console.Error.WriteLine("dotnet-package-cache-gen <path-to-directory>");
                Environment.Exit(1);
                return;
            }

            var builder = new PackageReferenceModelBuilder();
            var rootPath = args[0];
            rootPath = Path.GetFullPath(rootPath);
            Console.Error.WriteLine($"RootPath: {rootPath}");
            var projectFiles = CsprojLocator.Locate(rootPath);

            foreach (var projectFile in projectFiles)
            {
                CsprojParser.Parse(projectFile, builder);
            }

            var model = builder.Build();
            CsprojGenerator.Generate(model, Console.Out);

            PrintModelData(model);
        }

        private static void PrintModelData(PackageReferenceModel model)
        {
            Console.Error.WriteLine("Nuget package cache:");
            Console.Error.WriteLine();
            foreach (var packageReferenceGroup in model.PackageReferenceGroups)
            {
                Console.Error.WriteLine($"For \"{packageReferenceGroup.TargetFramework}\":");
                foreach (var packageReference in packageReferenceGroup.PackageReferences)
                {
                    Console.Error.WriteLine($"  - {packageReference.Id} (version {packageReference.Version})");
                }
            }
            Console.Error.WriteLine();
            Console.Error.WriteLine("SDK(s):");
            foreach (var sdk in model.Sdks)
            {
                Console.Error.WriteLine($"  - {sdk}");
            }

            if (model.RuntimeIdentifiers.Length > 0)
            {
                Console.Error.WriteLine();
                Console.Error.WriteLine("RuntimeIdentifier(s):");
                foreach (var rid in model.RuntimeIdentifiers)
                {
                    Console.Error.WriteLine($"  - {rid}");
                }
            }
        }
    }
};