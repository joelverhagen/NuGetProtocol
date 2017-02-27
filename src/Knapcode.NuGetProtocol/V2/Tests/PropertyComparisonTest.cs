using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Knapcode.NuGetProtocol.V2.Tests
{
    public class PropertyComparisonTest
    {
        private readonly PackageSourceProvider _packageSourceProvider;
        private readonly PackageReader _packageReader;
        private readonly TestData _testData;
        private readonly Client _client;

        public PropertyComparisonTest(PackageSourceProvider packageSourceProvider, PackageReader packageReader, TestData testData, Client client)
        {
            _packageSourceProvider = packageSourceProvider;
            _packageReader = packageReader;
            _testData = testData;
            _client = client;
        }

        public async Task<PropertyComparison> ExecuteAsync()
        {
            var allTypes = new HashSet<PackageSourceType>();
            var propertyNameToTypes = new Dictionary<string, HashSet<PackageSourceType>>();

            var sources = _packageSourceProvider.GetPackageSouces();
            foreach (var source in sources)
            {
                allTypes.Add(source.Type);

                using (var stream = _testData.PackageKNpA)
                {
                    var identity = _packageReader.GetPackageIdentity(stream);
                    stream.Position = 0;

                    var pushResult = await _client.PushPackageIfNotExistsAsync(source, stream);
                    if (pushResult.PackageResult.Data == null)
                    {
                        throw new InvalidOperationException("The package was not pushed successfully.");
                    }

                    foreach (var propertyName in pushResult.PackageResult.Data.PropertyNames)
                    {
                        HashSet<PackageSourceType> types;
                        if (!propertyNameToTypes.TryGetValue(propertyName, out types))
                        {
                            types = new HashSet<PackageSourceType>();
                            propertyNameToTypes[propertyName] = types;
                        }

                        types.Add(source.Type);
                    }
                }
            }

            var propertyNamesOnAllTypes = new List<string>();
            foreach (var propertyName in propertyNameToTypes.Keys.ToList())
            {
                var types = propertyNameToTypes[propertyName];
                if (types.SetEquals(allTypes))
                {
                    propertyNamesOnAllTypes.Add(propertyName);
                    propertyNameToTypes.Remove(propertyName);
                }
            }

            return new PropertyComparison
            {
                AllTypes = allTypes,
                PropertyNamesOnAllTypes = propertyNamesOnAllTypes,
                PropertyNameToTypes = propertyNameToTypes,
            };
        }
    }
}
