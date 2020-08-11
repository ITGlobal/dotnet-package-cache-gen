using System;
using System.Collections.Generic;
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

            // Load Sdk(s)
            var sdks = GetSdks(projectRootElement);
            foreach (var sdk in sdks)
            {
                Log.WriteLine($"{csprojPath}: found SDK \"{sdk}\"");
                builder.AddSdk(sdk);
            }

            // Load RuntimeIdentifier(s)
            var runtimeIdentifiers = GetRuntimeIdentifiers(projectRootElement);
            foreach (var runtimeIdentifier in runtimeIdentifiers)
            {
                Log.WriteLine($"{csprojPath}: found RID \"{runtimeIdentifier}\"");
                builder.AddRuntimeIdentifier(runtimeIdentifier);
            }

            // Load package references
            var defaultTargetFrameworks = GetDefaultTargetFrameworks(projectRootElement);
            LoadPackageReferences(csprojPath, projectRootElement, defaultTargetFrameworks, builder);
        }

        private static string[] GetSdks(XElement projectRootElement)
        {
            return GetSdksIterator()
                .Where(x => !string.IsNullOrEmpty(x))
                .Distinct()
                .ToArray();

            IEnumerable<string> GetSdksIterator()
            {
                var defaultSdk = projectRootElement.Attribute("Sdk")?.Value;
                yield return defaultSdk;

                // Scan <Import Project="Sdk.props" Sdk="Microsoft.NET.Sdk"/> elements
                foreach (var importElement in projectRootElement.Elements("Import"))
                {
                    if (importElement.Attribute("Project")?.Value != "Sdk.props")
                    {
                        continue;
                    }

                    var sdk = importElement.Attribute("Sdk")?.Value;
                    yield return sdk;
                }

                // Scan <Sdk Name="Microsoft.NET.Sdk" /> elements
                foreach (var sdkElement in projectRootElement.Elements("Sdk"))
                {
                    var sdk = sdkElement.Attribute("Name")?.Value;
                    yield return sdk;
                }
            }
        }

        private static string[] GetRuntimeIdentifiers(XElement projectRootElement)
        {
            var defaultRuntimeIdentifier = projectRootElement
                                               .Elements("PropertyGroup")
                                               .Elements("RuntimeIdentifier")
                                               .FirstOrDefault()
                                               ?.Value ??
                                           "";
            var defaultRuntimeIdentifiers = projectRootElement
                                                .Elements("PropertyGroup")
                                                .Elements("RuntimeIdentifiers")
                                                .FirstOrDefault()
                                                ?.Value ??
                                            "";

            var runtimeIdentifiers = new[] {defaultRuntimeIdentifier}
                .Concat(defaultRuntimeIdentifiers.Split(";"))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToArray();

            return runtimeIdentifiers;
        }

        private static string[] GetDefaultTargetFrameworks(XElement projectRootElement)
        {
            var defaultTargetFramework = projectRootElement
                                             .Elements("PropertyGroup")
                                             .Elements("TargetFramework")
                                             .FirstOrDefault()
                                             ?.Value ??
                                         "";
            var defaultTargetFrameworks = projectRootElement
                                              .Elements("PropertyGroup")
                                              .Elements("TargetFrameworks")
                                              .FirstOrDefault()
                                              ?.Value ??
                                          "";

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

        private static void LoadPackageReferences(
            string csprojPath,
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

                    itemGroupTargetFrameworks ??=
                        ResolveTargetFrameworks(csprojPath, itemGroup) ?? defaultTargetFrameworks;

                    foreach (var targetFramework in itemGroupTargetFrameworks)
                    {
                        Log.WriteLine(
                            $"{csprojPath}: found package \"{packageId}\" (v{packageVersion}, for {targetFramework})"
                        );
                        builder.AddPackage(targetFramework, packageId, packageVersion);
                    }
                }
            }
        }

        private static string[] ResolveTargetFrameworks(string csprojPath, XElement itemGroup)
        {
            var condition = itemGroup.Attribute("Condition")?.Value;
            if (string.IsNullOrEmpty(condition))
            {
                return null;
            }

            var targetFrameworks = MsbuildConditionEvaluator.EvaluateTargetFrameworks(csprojPath, condition);
            return targetFrameworks;
        }

    }
}