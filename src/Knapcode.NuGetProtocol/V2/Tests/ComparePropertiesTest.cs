using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Knapcode.NuGetProtocol.V2.Tests
{
    public class ComparePropertiesTest
    {
        private readonly PackageSourceProvider _packageSourceProvider;
        private readonly PackageReader _packageReader;
        private readonly Client _client;

        public ComparePropertiesTest(PackageSourceProvider packageSourceProvider, PackageReader packageReader, Client client)
        {
            _packageSourceProvider = packageSourceProvider;
            _packageReader = packageReader;
            _client = client;
        }

        public async Task ExecuteAsync(IEnumerable<PackageSource> sources)
        {
            foreach (var source in sources)
            {
                using (var stream = new FileStream(
                    "Microsoft.AspNet.Mvc.5.0.0-beta1.nupkg",
                    FileMode.Open,
                    FileAccess.Read))
                {
                    var identity = _packageReader.GetPackageIdentity(stream);

                    stream.Position = 0;

                    await _client.PushPackageIfNotExistsAsync(source, stream);

                    var package = await _client.GetPackageAsync(source, identity);
                }
            }
        }
    }
}
