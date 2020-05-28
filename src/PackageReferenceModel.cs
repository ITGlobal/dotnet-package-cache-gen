using System;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace ITGlobal.DotNetPackageCacheGenerator
{
    public sealed class PackageReferenceModel
    {
        public PackageReferenceModel(PackageReferenceGroup[] packageReferenceGroups)
        {
            PackageReferenceGroups = packageReferenceGroups;
            var json = JsonConvert.SerializeObject(new {Groups=packageReferenceGroups}, Formatting.None);
            using var alg = SHA1.Create();
            var hash = alg.ComputeHash(Encoding.UTF8.GetBytes(json));
            Hash = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        public PackageReferenceGroup[] PackageReferenceGroups { get; }
        public string Hash { get; }
    }
}