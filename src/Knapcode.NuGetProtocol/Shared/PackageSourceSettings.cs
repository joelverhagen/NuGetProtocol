namespace Knapcode.NuGetProtocol.Shared
{
    public class PackageSourceSettings
    {
        public string MyGetV2SourceUri { get; set; }
        public string MyGetV2PushUri { get; set; }
        public string MyGetApiKey { get; set; }

        public string NuGetGalleryV2SourceUri { get; set; }
        public string NuGetGalleryV2PushUri { get; set; }
        public string NuGetGalleryApiKey { get; set; }

        public string VstsV2SourceUri { get; set; }
        public string VstsV2PushUri { get; set; }
        public string VstsReadPersonalAccessToken { get; set; }
        public string VstsReadWritePersonalAccessToken { get; set; }

        public string NuGetServerWcfSourceUri { get; set; }
        public string NuGetServerWcfPushUri { get; set; }
        public string NuGetServerWcfApiKey { get; set; }

        public string NuGetServerWebApiSourceUri { get; set; }
        public string NuGetServerWebApiPushUri { get; set; }
        public string NuGetServerWebApiApiKey { get; set; }
    }
}
