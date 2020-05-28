namespace ITGlobal.DotNetPackageCacheGenerator
{
    public sealed class PackageReferenceGroup
    {
        public PackageReferenceGroup(string targetFramework, PackageReference[] packageReferences)
        {
            TargetFramework = targetFramework;
            PackageReferences = packageReferences;
        }

        public string TargetFramework { get; }
        public PackageReference[] PackageReferences { get; }
    }
}