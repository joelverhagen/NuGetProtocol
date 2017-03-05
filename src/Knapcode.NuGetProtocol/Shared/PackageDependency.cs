namespace Knapcode.NuGetProtocol.Shared
{
    public class PackageDependency
    {
        public string Id { get; set; }
        public string VersionRange { get; set; }
        public string Framework { get; set; }
    }
}
