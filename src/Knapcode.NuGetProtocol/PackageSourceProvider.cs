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
                    new ApiKeyAuthorization(_options.Value.NuGetGalleryApiKey));
            }

            if (_options.Value.MyGetV2SourceUri != null)
            {
                yield return new PackageSource(
                    _options.Value.MyGetV2SourceUri,
                    new ApiKeyAuthorization(_options.Value.MyGetApiKey));
            }

            if (_options.Value.VstsV2SourceUri != null)
            {
                yield return new PackageSource(
                    _options.Value.VstsV2SourceUri,
                    new BasicAuthorization("PAT", _options.Value.VstsPersonalReadWriteAccessToken));
            }

            if (_options.Value.NuGetServerWcfSourceUri != null)
            {
                yield return new PackageSource(
                    _options.Value.NuGetServerWcfSourceUri,
                    new ApiKeyAuthorization(_options.Value.NuGetServerWcfApiKey));
            }

            if (_options.Value.NuGetServerWebApiSourceUri != null)
            {
                yield return new PackageSource(
                    _options.Value.NuGetServerWebApiSourceUri,
                    new ApiKeyAuthorization(_options.Value.NuGetServerWebApiApiKey));
            }
        }
    }
}
