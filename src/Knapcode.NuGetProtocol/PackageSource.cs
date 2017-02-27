namespace Knapcode.NuGetProtocol
{
    public class PackageSource
    {
        public PackageSource(
            PackageSourceType type,
            string sourceUri,
            string pushUri,
            IFeedAuthorization sourceAuth,
            IFeedAuthorization pushAuth)
        {
            Type = type;
            SourceUri = sourceUri;
            PushUri = pushUri;
            SourceAuthorization = sourceAuth;
            PushAuthorization = pushAuth;
        }

        public PackageSourceType Type { get; }
        public string SourceUri { get; }
        public string PushUri { get; }
        public IFeedAuthorization SourceAuthorization { get; }
        public IFeedAuthorization PushAuthorization { get; }

        public override string ToString()
        {
            return $"{Type}: {SourceUri}";
        }
    }
}
