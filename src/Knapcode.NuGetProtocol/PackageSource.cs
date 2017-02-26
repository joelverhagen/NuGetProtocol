namespace Knapcode.NuGetProtocol
{
    public class PackageSource
    {
        public PackageSource(string uri, IFeedAuthorization auth)
        {
            Uri = uri;
            FeedAuthorization = auth;
        }
        
        public string Uri { get; set; }
        public IFeedAuthorization FeedAuthorization { get; set; }
    }
}
