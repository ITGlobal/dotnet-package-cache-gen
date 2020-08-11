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
            string rootPath,
            string[] sdks,
            string[] runtimeIdentifiers,
            PackageReferenceGroup[] packageReferenceGroups)
        {
            RootPath = rootPath;
            Sdks = sdks;
            PackageReferenceGroups = packageReferenceGroups;
            RuntimeIdentifiers = runtimeIdentifiers;
            TargetFrameworks = packageReferenceGroups.Select(_ => _.TargetFramework).ToArray();
            var json = JsonConvert.SerializeObject(new {Groups = packageReferenceGroups}, Formatting.None);
            using var alg = SHA1.Create();
            var hash = alg.ComputeHash(Encoding.UTF8.GetBytes(json));
            Hash = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        public string RootPath { get; }
        public string[] Sdks { get; }
        public string[] RuntimeIdentifiers { get; }
        public string[] TargetFrameworks { get; }
        public PackageReferenceGroup[] PackageReferenceGroups { get; }
        public string Hash { get; }
    }
}