using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Knapcode.NuGetProtocol.Shared;

namespace Knapcode.NuGetProtocol.V2.Tests
{
    public class PropertyComparisonTest
    {
        private static readonly HashSet<string> PropertiesIntendedToBeDifference = new HashSet<string>
        {
            "Created",
            "DownloadCount",
            "GalleryDetailsUrl",
            "LastUpdated",
            "Published",
            "ReportAbuseUrl",
            "VersionDownloadCount",
            "LastEdited",
            "LicenseNames",
            "LicenseReportUrl",
        };

        private readonly PackageSourceProvider _packageSourceProvider;
        private readonly PackageReader _packageReader;
        private readonly TestData _testData;
        private readonly Client _client;
        private readonly Mapper _mapper;

        public PropertyComparisonTest(PackageSourceProvider packageSourceProvider, PackageReader packageReader, TestData testData, Client client, Mapper mapper)
        {
            _packageSourceProvider = packageSourceProvider;
            _packageReader = packageReader;
            _testData = testData;
            _client = client;
            _mapper = mapper;
        }

        public async Task<PropertyComparison> ExecuteAsync()
        {
            var typeToPackageProperties = new Dictionary<PackageSourceType, Dictionary<string, string>>();
            foreach (var source in _packageSourceProvider.GetPackageSources())
            {
                var result = await _client.PushPackageIfNotExistsAsync(source, _testData.PackageKNpB);
                if (result.PackageResult.Data == null)
                {
                    throw new InvalidOperationException("The package was not pushed successfully.");
                }

                var metadata = await _client.GetMetadataAsync(source);

                typeToPackageProperties[source.Type] = GetPropertyDictionary(metadata, result.PackageResult.Data);
            }
            
            // Collect the data per property name across all sources.
            var actualValues = new Dictionary<string, Dictionary<string, HashSet<PackageSourceType>>>();
            foreach (var type in typeToPackageProperties.Keys)
            {
                var packageProperties = typeToPackageProperties[type];
                foreach (var pair in packageProperties)
                {
                    Dictionary<string, HashSet<PackageSourceType>> valueToTypes;
                    if (!actualValues.TryGetValue(pair.Key, out valueToTypes))
                    {
                        valueToTypes = new Dictionary<string, HashSet<PackageSourceType>>();
                        actualValues[pair.Key] = valueToTypes;
                    }

                    HashSet<PackageSourceType> types;
                    if (!valueToTypes.TryGetValue(pair.Value, out types))
                    {
                        types = new HashSet<PackageSourceType>();
                        valueToTypes[pair.Value] = types;
                    }

                    types.Add(type);
                }
            }

            // Get the expected properties from the package itself.
            var originalPackage = await _packageReader.GetPackageAsync(
                _testData.PackageKNpB,
                listed: true,
                isAbsoluteLatestVersion: true,
                isLatestVersion: false);
            var originalPackageEntry = _mapper.GetPackageEntry(originalPackage);
            var expectedValues = GetPropertyDictionary(Metadata.Empty, originalPackageEntry);

            // Categorize the differences.
            var valuesSameAsSource = new Dictionary<string, KeyValuePair<string, HashSet<PackageSourceType>>>();
            var valuesIntendedToBeDifferent = new Dictionary<string, Dictionary<string, HashSet<PackageSourceType>>>();
            var valuesNotIntendedToBeDifferent = new Dictionary<string, Dictionary<string, HashSet<PackageSourceType>>>();
            foreach (var propertyName in actualValues.Keys)
            {
                var propertyNameToValues = PropertiesIntendedToBeDifference.Contains(propertyName) ?
                    valuesIntendedToBeDifferent :
                    valuesNotIntendedToBeDifferent;

                string expectedValue;
                if (!expectedValues.TryGetValue(propertyName, out expectedValue))
                {
                    expectedValue = null;
                }

                var unsortedValueToTypes = actualValues[propertyName];
                foreach (var actualValue in unsortedValueToTypes.Keys)
                {
                    var types = unsortedValueToTypes[actualValue];
                    if (actualValue == expectedValue)
                    {
                        var pair = new KeyValuePair<string, HashSet<PackageSourceType>>(expectedValue, types);
                        valuesSameAsSource[propertyName] = pair;
                    }
                    else
                    {
                        Dictionary<string, HashSet<PackageSourceType>> valueToTypes;
                        if (!propertyNameToValues.TryGetValue(propertyName, out valueToTypes))
                        {
                            valueToTypes = new Dictionary<string, HashSet<PackageSourceType>>();
                            propertyNameToValues[propertyName] = valueToTypes;
                        }

                        valueToTypes[actualValue] = types;
                    }
                }
            }

            return new PropertyComparison
            {
                ExpectedValues = expectedValues,
                ValuesSameAsSource = valuesSameAsSource,
                ValuesIntendedToBeDifferent = valuesIntendedToBeDifferent,
                ValuesNotIntendedToBeDifferent = valuesNotIntendedToBeDifferent,
            };
        }

        private Dictionary<string, string> GetPropertyDictionary(Metadata metadata, PackageEntry entry)
        {
            return _mapper
                .GetPropertyPairs(metadata, entry)
                .GroupBy(x => x.Key)
                .ToDictionary(x => x.Key, x => x.First().Value);
        }
    }
}
