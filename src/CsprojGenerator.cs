using System.IO;
using System.Linq;

namespace ITGlobal.DotNetPackageCacheGenerator
{
    public static class CsprojGenerator
    {
        public static void Generate(PackageReferenceModel model, TextWriter writer)
        {
            var targetFrameworks = model.PackageReferenceGroups.Select(_ => _.TargetFramework).ToArray();

            writer.WriteLine($"<!-- NuGet package cache -->");
            writer.WriteLine($"<Project Sdk=\"Microsoft.NET.Sdk.Web\">");
            writer.WriteLine($"    <PropertyGroup>");
            switch (targetFrameworks.Length)
            {
                case 0:
                    break;
                case 1:
                    writer.WriteLine($"        <TargetFramework>{targetFrameworks[0]}</TargetFramework>");
                    break;
                default:
                    writer.WriteLine($"        <TargetFrameworks>{string.Join(";", targetFrameworks)}</TargetFrameworks>");
                    break;
            }

            writer.WriteLine($"        <_PackageCacheHash>{model.Hash}</_PackageCacheHash>");
            writer.WriteLine($"    </PropertyGroup>");
            foreach (var packageReferenceGroup in model.PackageReferenceGroups)
            {
                var condition = $"'$(TargetFramework)' == '{packageReferenceGroup.TargetFramework}'";
                writer.WriteLine($"    <ItemGroup Condition=\"{condition}\">");
                foreach (var packageReference in packageReferenceGroup.PackageReferences)
                {
                    writer.WriteLine($"        <PackageReference Include=\"{packageReference.Id}\" Version=\"{packageReference.Version}\" />");
                }
                writer.WriteLine($"    </ItemGroup>");
            }
            writer.WriteLine($"</Project>");
        }
    }
}