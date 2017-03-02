using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Knapcode.NuGetProtocol.Shared;

namespace Knapcode.NuGetProtocol.V2
{
    public class Client
    {
        private static readonly ConcurrentDictionary<string, Task<Metadata>> _metadata
            = new ConcurrentDictionary<string, Task<Metadata>>();

        private readonly Protocol _protocol;
        private readonly PackageReader _packageReader;

        public Client(Protocol protocol, PackageReader packageReader)
        {
            _protocol = protocol;
            _packageReader = packageReader;
        }

        public async Task<Metadata> GetMetadataAsync(PackageSource source)
        {
            return await _metadata.GetOrAdd(
                source.SourceUri,
                _ => _protocol.GetMetadataAsync(source));
        }

        public async Task<HttpStatusCode> PushPackageAsync(PackageSource source, Stream package)
        {
            return await _protocol.PushPackageAsync(source, package);
        }

        public async Task<HttpResult<PackageEntry>> GetPackageAsync(PackageSource source, PackageIdentity package)
        {
            return await _protocol.GetPackageAsync(source, package);
        }

        public async Task<ConditionalPushResult> PushPackageIfNotExistsAsync(PackageSource source, Stream package)
        {
            var identity = _packageReader.GetPackageIdentity(package);

            package.Position = 0;

            var packageResultBeforePush = await GetPackageAsync(source, identity);
            if (packageResultBeforePush.StatusCode == HttpStatusCode.OK)
            {
                return new ConditionalPushResult
                {
                    PackageAlreadyExists = true,
                    PackageResult = packageResultBeforePush,
                };
            }

            var stopwatch = Stopwatch.StartNew();
            var pushStatusCode = await PushPackageAsync(source, package);
            var timeToPush = stopwatch.Elapsed;
            var packageStatusCode = HttpStatusCode.NotFound;
            HttpResult<PackageEntry> packageResultAfterPush = null;
            while (packageStatusCode == HttpStatusCode.NotFound &&
                   stopwatch.Elapsed < TimeSpan.FromMinutes(20))
            {
                packageResultAfterPush = await GetPackageAsync(source, identity);
                packageStatusCode = packageResultAfterPush.StatusCode;
                if (packageStatusCode == HttpStatusCode.NotFound)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            }

            return new ConditionalPushResult
            {
                PackageAlreadyExists = false,
                PackageResult = packageResultAfterPush,
                PackagePushSuccessfully = packageStatusCode == HttpStatusCode.OK,
                PushStatusCode = pushStatusCode,
                TimeToPush = timeToPush,
                TimeToBeAvailable = packageStatusCode == HttpStatusCode.OK ? stopwatch.Elapsed : (TimeSpan?)null,
            };
        }
    }
}
