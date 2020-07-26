using System;
using System.Linq;
using System.Xml.Linq;

namespace ITGlobal.DotNetPackageCacheGenerator
{
    public static class CsprojParser
    {
        public static void Parse(string csprojPath, PackageReferenceModelBuilder builder)
        {
            var csproj = XDocument.Load(csprojPath);
            var projectRootElement = csproj.Element("Project") ?? throw new Exception("Malformed csproj");

            // Load RuntimeIdentifier(s)
            var runtimeIdentifiers = GetRuntimeIdentifiers(projectRootElement);
            foreach (var runtimeIdentifier in runtimeIdentifiers)
            {
                builder.AddRuntimeIdentifier(runtimeIdentifier);
            }

            // Load package references
            var defaultTargetFrameworks = GetDefaultTargetFrameworks(projectRootElement);
            LoadPackageReferences(projectRootElement, defaultTargetFrameworks, builder);
        }

        private static string[] GetDefaultTargetFrameworks(XElement projectRootElement)
        {
            var defaultTargetFramework = projectRootElement
                .Elements("PropertyGroup")
                .Elements("TargetFramework")
                .FirstOrDefault()
                ?.Value ?? "";
            var defaultTargetFrameworks = projectRootElement
                .Elements("PropertyGroup")
                .Elements("TargetFrameworks")
                .FirstOrDefault()
                ?.Value ?? "";

            var targetFrameworks = new[] {defaultTargetFramework}
                .Concat(defaultTargetFrameworks.Split(";"))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToArray();

            if (targetFrameworks.Length == 0)
            {
                throw new Exception("Unable to detect target frameworks");
            }

            return targetFrameworks;
        }

        private static string[] GetRuntimeIdentifiers(XElement projectRootElement)
        {
            var defaultRuntimeIdentifier = projectRootElement
                .Elements("PropertyGroup")
                .Elements("RuntimeIdentifier")
                .FirstOrDefault()
                ?.Value ?? "";
            var defaultRuntimeIdentifiers = projectRootElement
                .Elements("PropertyGroup")
                .Elements("RuntimeIdentifiers")
                .FirstOrDefault()
                ?.Value ?? "";

            var runtimeIdentifiers = new[] {defaultRuntimeIdentifier}
                .Concat(defaultRuntimeIdentifiers.Split(";"))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToArray();

            return runtimeIdentifiers;
        }

        private static void LoadPackageReferences(
            XElement projectRootElement,
            string[] defaultTargetFrameworks,
            PackageReferenceModelBuilder builder)
        {
            foreach (var itemGroup in projectRootElement.Elements("ItemGroup"))
            {
                string[] itemGroupTargetFrameworks = null;
                foreach (var packageReference in itemGroup.Elements("PackageReference"))
                {
                    var packageId = packageReference.Attribute("Include")!.Value;
                    var packageVersion = packageReference.Attribute("Version")?.Value;
                    if (string.IsNullOrEmpty(packageVersion))
                    {
                        continue;
                    }

                    itemGroupTargetFrameworks ??= ResolveTargetFrameworks(itemGroup) ?? defaultTargetFrameworks;

                    foreach (var targetFramework in itemGroupTargetFrameworks)
                    {
                        builder.AddPackage(targetFramework, packageId, packageVersion);
                    }
                }
            }
        }

        private static string[] ResolveTargetFrameworks(XElement itemGroup)
        {
            var condition = itemGroup.Attribute("Condition")?.Value;
            if (string.IsNullOrEmpty(condition))
            {
                return null;
            }

            var targetFrameworks = MsbuildConditionEvaluator.EvaluateTargetFrameworks(condition);
            return targetFrameworks;
        }
    }
}