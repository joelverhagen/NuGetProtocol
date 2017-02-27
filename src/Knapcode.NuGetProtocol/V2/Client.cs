using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Knapcode.NuGetProtocol.V2
{
    public class Client
    {
        private readonly Protocol _protocol;
        private readonly PackageReader _packageReader;

        public Client(Protocol protocol, PackageReader packageReader)
        {
            _protocol = protocol;
            _packageReader = packageReader;
        }

        public async Task<HttpStatusCode> PushPackageAsync(PackageSource source, Stream package)
        {
            return await _protocol.PushPackageAsync(source, package);
        }

        public async Task<bool> PackageExistsAsync(PackageSource source, PackageIdentity package)
        {
            var packageResult = await GetPackageAsync(source, package);

            return packageResult.StatusCode == HttpStatusCode.OK;
        }

        public async Task<HttpResult<PackageEntry>> GetPackageAsync(PackageSource source, PackageIdentity package)
        {
            return await _protocol.GetPackageAsync(source, package);
        }

        public async Task<ConditionalPushResult> PushPackageIfNotExistsAsync(PackageSource source, Stream package)
        {
            var identity = _packageReader.GetPackageIdentity(package);

            package.Position = 0;

            if (await PackageExistsAsync(source, identity))
            {
                return new ConditionalPushResult
                {
                    PushAttempted = false,
                };
            }

            var stopwatch = Stopwatch.StartNew();
            var pushStatusCode = await PushPackageAsync(source, package);
            var timeToPush = stopwatch.Elapsed;
            var packageStatusCode = HttpStatusCode.NotFound;
            while (packageStatusCode == HttpStatusCode.NotFound &&
                   stopwatch.Elapsed < TimeSpan.FromMinutes(20))
            {
                var fetchedPackage = await GetPackageAsync(source, identity);
                packageStatusCode = fetchedPackage.StatusCode;
                if (packageStatusCode == HttpStatusCode.NotFound)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            }

            return new ConditionalPushResult
            {
                PushAttempted = true,
                PackageExists = packageStatusCode == HttpStatusCode.OK,
                StatusCode = pushStatusCode,
                TimeToPush = timeToPush,
                TimeToBeAvailable = packageStatusCode == HttpStatusCode.OK ? stopwatch.Elapsed : (TimeSpan?)null,
            };
        }
    }
}
