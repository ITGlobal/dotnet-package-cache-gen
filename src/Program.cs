using System;
using System.IO;
using System.Threading.Tasks;

namespace ITGlobal.DotNetPackageCacheGenerator
{
    public static class Program
    {

        public static async Task Main(string[] args)
        {

            if (!TryParseCommandLine(args, out var rootPath, out var verbose, out var help))
            {
                PrintUsage();
                Environment.Exit(1);
                return;
            }

            if (help)
            {
                PrintUsage();
                PrintUsage();
                Environment.Exit(0);
                return;
            }

            Log.IsEnabled = verbose;

            rootPath = Path.GetFullPath(rootPath);
            var builder = new PackageReferenceModelBuilder(rootPath);
            var projectFiles = CsprojLocator.Locate(rootPath);

            foreach (var projectFile in projectFiles)
            {
                CsprojParser.Parse(projectFile, builder);
            }

            var model = builder.Build();
            PrintModelData(model);

            await CsprojGenerator.Generate(model, Console.Out);
        }

        private static bool TryParseCommandLine(string[] args, out string rootPath, out bool verbose, out bool help)
        {
            rootPath = default;
            verbose = default;
            help = default;

            foreach (var arg in args)
            {
                switch (arg)
                {
                    case "-h":
                        help = true;
                        break;
                    case "--help":
                        help = true;
                        break;
                    case "-v":
                        verbose = true;
                        break;
                    case "--verbose":
                        verbose = true;
                        break;
                    default:
                        if (rootPath != null)
                        {
                            return false;
                        }

                        rootPath = arg;
                        break;
                }
            }

            return help || rootPath != null;
        }

        private static void PrintUsage()
        {
            Console.Error.WriteLine("USAGE:");
            Console.Error.WriteLine("dotnet-package-cache-gen [-v|--verbose] [-h|--help] <path-to-directory>");
        }

        private static void PrintModelData(PackageReferenceModel model)
        {
            Console.Error.WriteLine("# Nuget package cache");
            Console.Error.WriteLine($"root_path: {model.RootPath}");
            Console.Error.WriteLine($"hash: {model.Hash}");
            Console.Error.WriteLine("target_frameworks:");
            foreach (var packageReferenceGroup in model.PackageReferenceGroups)
            {
                Console.Error.WriteLine($"  - framework: {packageReferenceGroup.TargetFramework}");
                Console.Error.WriteLine($"    packages:");
                foreach (var packageReference in packageReferenceGroup.PackageReferences)
                {
                    Console.Error.WriteLine($"    - id: {packageReference.Id}");
                    Console.Error.WriteLine($"      version: {packageReference.Version}");
                }
            }
            Console.Error.WriteLine("sdk:");
            foreach (var sdk in model.Sdks)
            {
                Console.Error.WriteLine($"  - {sdk}");
            }

            if (model.RuntimeIdentifiers.Length > 0)
            {
                Console.Error.WriteLine("runtimes:");
                foreach (var rid in model.RuntimeIdentifiers)
                {
                    Console.Error.WriteLine($"  - {rid}");
                }
            }
            Console.Error.WriteLine();
        }
    }
};