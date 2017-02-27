namespace Knapcode.NuGetProtocol
{
    public class PackageIdentity
    {
        public PackageIdentity(string id, string version)
        {
            Id = id;
            Version = version;
        }

        public string Id { get; }
        public string Version { get; }

        public override string ToString()
        {
            return $"{Id} {Version}";
        }
    }
}
