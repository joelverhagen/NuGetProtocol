namespace Knapcode.NuGetProtocol
{
    public class PackageSource
    {
        public PackageSource(string sourceUri, string pushUri, IFeedAuthorization sourceAuth, IFeedAuthorization pushAuth)
        {
            SourceUri = sourceUri;
            PushUri = pushUri;
            SourceAuthorization = sourceAuth;
            PushAuthorization = pushAuth;

        }
        
        public string SourceUri { get; }
        public string PushUri { get; }
        public IFeedAuthorization SourceAuthorization { get; }
        public IFeedAuthorization PushAuthorization { get; }

        public override string ToString()
        {
            return SourceUri;
        }
    }
}
