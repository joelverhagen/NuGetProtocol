using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Knapcode.NuGetProtocol.Shared;

namespace Knapcode.NuGetProtocol.V2.Tests
{
    public class PropertyConsistencyTest
    {
        private readonly PackageSourceProvider _packageSourceProvider;
        private readonly Client _client;
        private readonly TestData _testData;
        private readonly PackageReader _packageReader;

        public PropertyConsistencyTest(PackageSourceProvider packageSourceProvider, Client client, TestData testData, PackageReader packageReader)
        {
            _packageSourceProvider = packageSourceProvider;
            _client = client;
            _testData = testData;
            _packageReader = packageReader;
        }

        public async Task<PropertyConsistency> ExecuteAsync()
        {
            // Collect the packages entries on all sources.
            var typeToPackageEntry = new Dictionary<PackageSourceType, Dictionary<PackageEntrySource, PackageEntry>>();
            foreach (var source in _packageSourceProvider.GetPackageSources())
            {
                typeToPackageEntry[source.Type] = await GetPackageEntries(source);
            }

            // Analyze all property pairs on all package entries.
            var typeToValues = new Dictionary<PackageSourceType, Dictionary<string, Dictionary<string, HashSet<PackageEntrySource>>>>();
            foreach (var type in typeToPackageEntry.Keys)
            {
                var packageEntries = typeToPackageEntry[type];
                typeToValues[type] = AnalyzePackageEntries(packageEntries);
            }

            // Remove properties that are the same in every response.
            var typeToConsistent = new Dictionary<PackageSourceType, Dictionary<string, string>>();
            foreach (var type in typeToValues.Keys.ToList())
            {
                var values = typeToValues[type];
                foreach (var propertyName in values.Keys.ToList())
                {
                    if (values[propertyName].Count < 2)
                    {
                        if (values[propertyName].Count == 1)
                        {
                            Dictionary<string, string> consistentValues;
                            if (!typeToConsistent.TryGetValue(type, out consistentValues))
                            {
                                consistentValues = new Dictionary<string, string>();
                                typeToConsistent[type] = consistentValues;
                            }

                            consistentValues[propertyName] = values[propertyName].Keys.First();
                        }

                        values.Remove(propertyName);
                    }
                }
            }

            return new PropertyConsistency
            {
                TypeToInconsistent = typeToValues,
                TypeToConsistent = typeToConsistent,
            };
        }

        private static Dictionary<string, Dictionary<string, HashSet<PackageEntrySource>>> AnalyzePackageEntries(Dictionary<PackageEntrySource, PackageEntry> packageEntries)
        {
            var propertyNameToValues = new Dictionary<string, Dictionary<string, HashSet<PackageEntrySource>>>();
            foreach (var source in packageEntries.Keys)
            {
                var packageEntry = packageEntries[source];
                var pairs = packageEntry
                    .PropertyPairs
                    .Concat(packageEntry.AtomPairs);

                foreach (var pair in pairs)
                {
                    Dictionary<string, HashSet<PackageEntrySource>> valueToSources;
                    if (!propertyNameToValues.TryGetValue(pair.Key, out valueToSources))
                    {
                        valueToSources = new Dictionary<string, HashSet<PackageEntrySource>>();
                        propertyNameToValues[pair.Key] = valueToSources;
                    }

                    HashSet<PackageEntrySource> sources;
                    if (!valueToSources.TryGetValue(pair.Value, out sources))
                    {
                        sources = new HashSet<PackageEntrySource>();
                        valueToSources[pair.Value] = sources;
                    }

                    sources.Add(source);
                }
            }

            return propertyNameToValues;
        }

        private async Task<Dictionary<PackageEntrySource, PackageEntry>> GetPackageEntries(PackageSource source)
        {
            var sourceToPackageEntry = new Dictionary<PackageEntrySource, PackageEntry>();

            // Push then fetch.
            var pushResult = await _client.PushPackageIfNotExistsAsync(source, _testData.PackageKNpB);
            if (pushResult.PackageResult.Data == null)
            {
                throw new InvalidOperationException("The package was not pushed successfully.");
            }
            sourceToPackageEntry[PackageEntrySource.GetPackageFirst] = pushResult.PackageResult.Data;

            // Fetch again. This should return same result as the previous request since it's using the exact same API.
            var packageIdentity = _packageReader.GetPackageIdentity(_testData.PackageKNpB);
            var getPackageResult = await _client.GetPackageEntryAsync(source, packageIdentity);
            if (getPackageResult.Data == null)
            {
                throw new InvalidOperationException("The package could not be fetched from using the ID and version keys.");
            }
            sourceToPackageEntry[PackageEntrySource.GetPackageSecond] = getPackageResult.Data;

            // Fetch from the packages collection using a simple query that could be easily send to another data store.
            var simpleFilterResult = await _client.GetPackageEntryFromCollectionWithSimpleFilterAsync(
                source,
                packageIdentity);
            if (simpleFilterResult.Data == null)
            {
                throw new InvalidOperationException("The package could not be fetched from the packages collection using a simple filter.");
            }
            sourceToPackageEntry[PackageEntrySource.PackageCollectionWithSimpleFilter] = simpleFilterResult.Data;

            // Fetch from the packages collection using a tricky query that should go to the database.
            var customFilterResult = await _client.GetPackageEntryFromCollectionWithCustomFilterAsync(
                source,
                packageIdentity);
            if (customFilterResult.Data == null)
            {
                throw new InvalidOperationException("The package could not be fetched from the packages collection using a custom filter.");
            }
            sourceToPackageEntry[PackageEntrySource.PackageCollectionWithCustomFilter] = customFilterResult.Data;

            return sourceToPackageEntry;
        }
    }
}
