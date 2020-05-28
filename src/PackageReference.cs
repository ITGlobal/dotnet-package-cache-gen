namespace ITGlobal.DotNetPackageCacheGenerator
{
    public sealed class PackageReference
    {
        public PackageReference(string id, string version)
        {
            Id = id;
            Version = version;
        }

        public string Id { get; }
        public string Version { get; }
    }
}