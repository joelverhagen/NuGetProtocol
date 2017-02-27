using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace Knapcode.NuGetProtocol
{
    public class PackageSourceProvider
    {
        private IOptions<PackageSourceSettings> _options;

        public PackageSourceProvider(IOptions<PackageSourceSettings> options)
        {
            _options = options;
        }

        public IEnumerable<PackageSource> GetPackageSouces()
        {
            if (_options.Value.NuGetGalleryV2SourceUri != null)
            {
                yield return new PackageSource(
                    _options.Value.NuGetGalleryV2SourceUri,
                    _options.Value.NuGetGalleryV2PushUri,
                    new NoAuthorization(),
                    new ApiKeyAuthorization(_options.Value.NuGetGalleryApiKey));
            }

            if (_options.Value.MyGetV2SourceUri != null)
            {
                yield return new PackageSource(
                    _options.Value.MyGetV2SourceUri,
                    _options.Value.MyGetV2PushUri,
                    new NoAuthorization(),
                    new ApiKeyAuthorization(_options.Value.MyGetApiKey));
            }

            if (_options.Value.VstsV2SourceUri != null)
            {
                yield return new PackageSource(
                    _options.Value.VstsV2SourceUri,
                    _options.Value.VstsV2PushUri,
                    new BasicAuthorization("PAT", _options.Value.VstsReadPersonalAccessToken),
                    new AggregateAuthorization(
                        new ApiKeyAuthorization("key"),
                        new BasicAuthorization("PAT", _options.Value.VstsReadWritePersonalAccessToken)));
            }

            if (_options.Value.NuGetServerWcfSourceUri != null)
            {
                yield return new PackageSource(
                    _options.Value.NuGetServerWcfSourceUri,
                    _options.Value.NuGetServerWcfPushUri,
                    new NoAuthorization(),
                    new ApiKeyAuthorization(_options.Value.NuGetServerWcfApiKey));
            }

            if (_options.Value.NuGetServerWebApiSourceUri != null)
            {
                yield return new PackageSource(
                    _options.Value.NuGetServerWebApiSourceUri,
                    _options.Value.NuGetServerWebApiPushUri,
                    new NoAuthorization(),
                    new ApiKeyAuthorization(_options.Value.NuGetServerWebApiApiKey));
            }
        }
    }
}
