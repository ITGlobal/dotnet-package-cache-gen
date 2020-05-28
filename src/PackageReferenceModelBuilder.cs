using System;
using System.Collections.Generic;
using System.Linq;

namespace ITGlobal.DotNetPackageCacheGenerator
{
    public sealed class PackageReferenceModelBuilder
    {
        private sealed class TargetFrameworkGroup : Dictionary<string, PackageGroup>
        {
            public TargetFrameworkGroup()
                : base(StringComparer.OrdinalIgnoreCase)
            {
            }
        }

        private sealed class PackageGroup : HashSet<string>
        {
            public PackageGroup()
                : base(StringComparer.OrdinalIgnoreCase)
            {
            }
        }

        private readonly Dictionary<string, TargetFrameworkGroup> _packageReferencesByTargetFramework
            = new Dictionary<string, TargetFrameworkGroup>(StringComparer.OrdinalIgnoreCase);

        public void AddPackage(string targetFramework, string packageId, string packageVersion)
        {
            if (!_packageReferencesByTargetFramework.TryGetValue(targetFramework, out var targetFrameworkGroup))
            {
                targetFrameworkGroup = new TargetFrameworkGroup();
                _packageReferencesByTargetFramework.Add(targetFramework, targetFrameworkGroup);
            }

            if (!targetFrameworkGroup.TryGetValue(packageId, out var packageGroup))
            {
                packageGroup = new PackageGroup();
                targetFrameworkGroup.Add(packageId, packageGroup);
            }

            packageGroup.Add(packageVersion);
        }

        public PackageReferenceModel Build()
        {
            var packageReferenceGroups = BuildPackageReferenceGroups()
                .OrderBy(_ => _.TargetFramework)
                .ToArray();
            return new PackageReferenceModel(packageReferenceGroups);
        }

        private IEnumerable<PackageReferenceGroup> BuildPackageReferenceGroups()
        {
            foreach (var (targetFramework, targetFrameworkGroup) in _packageReferencesByTargetFramework)
            {
                yield return new PackageReferenceGroup(
                    targetFramework,
                    BuildPackageReferences(targetFrameworkGroup).OrderBy(_ => _.Id).ThenBy(_ => _.Version).ToArray()
                );
            }
        }

        private IEnumerable<PackageReference> BuildPackageReferences(TargetFrameworkGroup targetFrameworkGroup)
        {
            foreach (var (packageId, packageVersions) in targetFrameworkGroup)
            {
                if (packageVersions.Count > 1)
                {
                    Console.Error.WriteLine($"Package version conflict: see package {packageId}");
                }

                foreach (var packageVersion in packageVersions)
                {
                    yield return new PackageReference(packageId, packageVersion);
                }
            }
        }
    }
}