using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace ITGlobal.DotNetPackageCacheGenerator
{
    public sealed class PackageReferenceModel
    {
        public PackageReferenceModel(
            string[] runtimeIdentifiers,
            PackageReferenceGroup[] packageReferenceGroups)
        {
            PackageReferenceGroups = packageReferenceGroups;
            RuntimeIdentifiers = runtimeIdentifiers;
            TargetFrameworks = packageReferenceGroups.Select(_ => _.TargetFramework).ToArray();
            var json = JsonConvert.SerializeObject(new {Groups = packageReferenceGroups}, Formatting.None);
            using var alg = SHA1.Create();
            var hash = alg.ComputeHash(Encoding.UTF8.GetBytes(json));
            Hash = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        public string[] RuntimeIdentifiers { get; }
        public string[] TargetFrameworks { get; }
        public PackageReferenceGroup[] PackageReferenceGroups { get; }
        public string Hash { get; }
    }
}