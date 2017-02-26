using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Knapcode.NuGetProtocol.V2
{
    public class Client
    {
        private readonly Protocol _v2Protocol;
        private readonly PackageReader _packageReader;

        public Client(Protocol v2Protocol, PackageReader packageReader)
        {
            _v2Protocol = v2Protocol;
            _packageReader = packageReader;
        }

        public async Task<HttpStatusCode> PushPackageAsync(PackageSource source, Stream package)
        {
            return await _v2Protocol.PushPackageAsync(source, package);
        }

        public async Task<bool> PackageExistsAsync(PackageSource source, PackageIdentity package)
        {
            var packageResult = await GetPackageAsync(source, package);

            return packageResult.StatusCode == HttpStatusCode.OK;
        }

        public async Task<HttpResult<PackageEntry>> GetPackageAsync(PackageSource source, PackageIdentity package)
        {
            return await _v2Protocol.GetPackageAsync(source, package);
        }

        public async Task<ConditionalPushResult> PushPackageIfNotExistsAsync(PackageSource source, Stream package)
        {
            var identity = _packageReader.GetPackageIdentity(package);

            package.Position = 0;

            if (await PackageExistsAsync(source, identity))
            {
                return new ConditionalPushResult
                {
                    Pushed = false,
                };
            }

            var statusCode = await PushPackageAsync(source, package);

            return new ConditionalPushResult
            {
                Pushed = true,
                StatusCode = statusCode,
            };
        }
    }
}
