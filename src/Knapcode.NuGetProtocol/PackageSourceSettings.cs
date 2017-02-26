namespace Knapcode.NuGetProtocol
{
    public class PackageSourceSettings
    {
        public string MyGetV2SourceUri { get; set; }
        public string MyGetApiKey { get; set; }
        public string NuGetGalleryV2SourceUri { get; set; }
        public string NuGetGalleryApiKey { get; set; }
        public string VstsV2SourceUri { get; set; }
        public string VstsPersonalReadAccessToken { get; }
        public string VstsPersonalReadWriteAccessToken { get; }
        public string NuGetServerWcfSourceUri { get; set; }
        public string NuGetServerWcfApiKey { get; set; }
        public string NuGetServerWebApiSourceUri { get; set; }
        public string NuGetServerWebApiApiKey { get; set; }
    }
}
