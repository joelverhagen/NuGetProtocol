using System.Threading.Tasks;

namespace Knapcode.NuGetProtocol.V2.Tests
{
    public class ComparePropertiesTest
    {
        private readonly PackageSourceProvider _packageSourceProvider;
        private readonly PackageReader _packageReader;
        private readonly TestData _testData;
        private readonly Client _client;

        public ComparePropertiesTest(PackageSourceProvider packageSourceProvider, PackageReader packageReader, TestData testData, Client client)
        {
            _packageSourceProvider = packageSourceProvider;
            _packageReader = packageReader;
            _testData = testData;
            _client = client;
        }

        public async Task ExecuteAsync()
        {
            var sources = _packageSourceProvider.GetPackageSouces();
            foreach (var source in sources)
            {
                using (var stream = _testData.PackageKNpA)
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
