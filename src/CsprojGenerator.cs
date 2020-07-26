using System.IO;

namespace ITGlobal.DotNetPackageCacheGenerator
{
    public static class CsprojGenerator
    {
        public static void Generate(PackageReferenceModel model, TextWriter writer)
        {
            writer.WriteLine($"<!-- NuGet package cache -->");
            switch (model.Sdks.Length)
            {
                case 1:
                    writer.WriteLine($"<Project Sdk=\"{model.Sdks[0]}\">");
                    break;
                default:
                    writer.WriteLine($"<Project>");
                    break;
            }

            writer.WriteLine($"    <PropertyGroup>");

            if (model.Sdks.Length != 1)
            {
                foreach (var sdk in model.Sdks)
                {
                    writer.WriteLine($"     <Sdk Name=\"{sdk}\" />");
                }
            }

            switch (model.TargetFrameworks.Length)
            {
                case 0:
                    break;
                case 1:
                    writer.WriteLine($"        <TargetFramework>{model.TargetFrameworks[0]}</TargetFramework>");
                    break;
                default:
                    writer.WriteLine($"        <TargetFrameworks>{string.Join(";", model.TargetFrameworks)}</TargetFrameworks>");
                    break;
            }

            switch (model.RuntimeIdentifiers.Length)
            {
                case 0:
                    break;
                case 1:
                    writer.WriteLine($"        <RuntimeIdentifier>{model.RuntimeIdentifiers[0]}</RuntimeIdentifier>");
                    break;
                default:
                    writer.WriteLine($"        <RuntimeIdentifiers>{string.Join(";", model.RuntimeIdentifiers)}</RuntimeIdentifiers>");
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