using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Knapcode.NuGetProtocol.Shared;

namespace Knapcode.NuGetProtocol.V2.Tests
{
    public class SchemaComparisonTest
    {
        private readonly PackageSourceProvider _packageSourceProvider;
        private readonly Client _client;

        public SchemaComparisonTest(PackageSourceProvider packageSourceProvider, Client client)
        {
            _packageSourceProvider = packageSourceProvider;
            _client = client;
        }

        public async Task<SchemaComparison> ExecuteAsync()
        {
            var allTypes = new HashSet<PackageSourceType>();
            var propertyNameToData = new Dictionary<string, PropertyData>();
            var sources = _packageSourceProvider.GetPackageSouces();
            foreach (var source in sources)
            {
                allTypes.Add(source.Type);

                var metadata = await _client.GetMetadataAsync(source);
                foreach (var entityProperty in metadata.PackageEntityType.Properties)
                {
                    PropertyData data;
                    if (!propertyNameToData.TryGetValue(entityProperty.Name, out data))
                    {
                        data = new PropertyData
                        {
                            PackageSourceTypeToNullable = new Dictionary<PackageSourceType, bool>(),
                            PackageSourceTypeToPropertyType = new Dictionary<PackageSourceType, string>(),
                            PackageSourceTypeToOnlyUsesTargetPath = new Dictionary<PackageSourceType, string>(),
                            PackageSourceTypeToKeepInContent = new Dictionary<PackageSourceType, bool>(),
                        };
                        propertyNameToData[entityProperty.Name] = data;
                    }

                    data.PackageSourceTypeToNullable[source.Type] = entityProperty.Nullable;
                    data.PackageSourceTypeToPropertyType[source.Type] = entityProperty.Type;
                    if (entityProperty.TargetPath != null && !entityProperty.KeepInContent)
                    {
                        data.PackageSourceTypeToOnlyUsesTargetPath[source.Type] = entityProperty.TargetPath;
                    }
                    data.PackageSourceTypeToKeepInContent[source.Type] = entityProperty.KeepInContent;
                }
            }

            var propertiesOnAllTypes = new List<string>();
            var propertiesOnSomeTypes = new Dictionary<string, HashSet<PackageSourceType>>();
            var directPropertiesOnAllTypes = new List<string>();
            var directPropertiesOnSomeTypes = new Dictionary<string, HashSet<PackageSourceType>>();
            var onlyUsesTargetPaths = new Dictionary<string, Dictionary<string, HashSet<PackageSourceType>>>();
            var propertyTypes = new Dictionary<string, Dictionary<string, HashSet<PackageSourceType>>>();
            var nullability = new Dictionary<string, Dictionary<bool, HashSet<PackageSourceType>>>();
            var keepInContent = new Dictionary<string, Dictionary<bool, HashSet<PackageSourceType>>>();

            foreach (var propertyName in propertyNameToData.Keys.ToList())
            {
                AddIfAllTypes(
                    allTypes,
                    propertiesOnAllTypes,
                    propertiesOnSomeTypes,
                    propertyName,
                    propertyNameToData[propertyName]
                        .PackageSourceTypeToPropertyType
                        .Select(x => x.Key));

                AddIfAllTypes(
                    allTypes,
                    directPropertiesOnAllTypes,
                    directPropertiesOnSomeTypes,
                    propertyName,
                    propertyNameToData[propertyName]
                        .PackageSourceTypeToKeepInContent
                        .Where(x => x.Value)
                        .Select(x => x.Key));

                GroupByValue(
                    onlyUsesTargetPaths,
                    propertyName,
                    propertyNameToData[propertyName].PackageSourceTypeToOnlyUsesTargetPath);

                GroupByValue(
                    propertyTypes,
                    propertyName,
                    propertyNameToData[propertyName].PackageSourceTypeToPropertyType);

                GroupByValue(
                    nullability,
                    propertyName,
                    propertyNameToData[propertyName].PackageSourceTypeToNullable);

                GroupByValue(
                    keepInContent,
                    propertyName,
                    propertyNameToData[propertyName].PackageSourceTypeToKeepInContent);
            }

            var output = new SchemaComparison
            {
                AllTypes = allTypes,
                PropertiesOnAllTypes = propertiesOnAllTypes,
                PropertiesOnSomeTypes = propertiesOnSomeTypes,
                DirectPropertiesOnAllTypes = directPropertiesOnAllTypes,
                DirectPropertiesOnSomeTypes = directPropertiesOnSomeTypes,
                OnlyUsesTargetPaths = onlyUsesTargetPaths,
                DifferingPropertyTypes = GetDiffering(propertyTypes),
                PropertyTypes = GetConsistent(propertyTypes),
                DifferingNullability = GetDiffering(nullability),
                Nullability = GetConsistent(nullability),
                DifferingKeepInContent = GetDiffering(keepInContent),
                KeepInContent = GetConsistent(keepInContent),
            };

            return output;
        }
        public static Dictionary<string, T> GetConsistent<T>(
            Dictionary<string, Dictionary<T, HashSet<PackageSourceType>>> propertyTypes)
        {
            return propertyTypes
                .Where(x => x.Value.Count() == 1)
                .ToDictionary(x => x.Key, x => x.Value.First().Key);
        }

        public static Dictionary<string, Dictionary<T, HashSet<PackageSourceType>>> GetDiffering<T>(
            Dictionary<string, Dictionary<T, HashSet<PackageSourceType>>> propertyTypes)
        {
            return propertyTypes
                .Where(x => x.Value.Count() != 1)
                .ToDictionary(x => x.Key, x => x.Value);
        }

        public static void AddIfAllTypes(
            HashSet<PackageSourceType> allTypes,
            List<string> outputIfAllTypes,
            Dictionary<string, HashSet<PackageSourceType>> outputIfSomeTypes,
            string propertyName,
            IEnumerable<PackageSourceType> types)
        {
            var set = new HashSet<PackageSourceType>(types);
            if (set.SetEquals(allTypes))
            {
                outputIfAllTypes.Add(propertyName);
            }
            else
            {
                outputIfSomeTypes[propertyName] = set;
            }
        }

        public static void GroupByValue<T>(
            Dictionary<string, Dictionary<T, HashSet<PackageSourceType>>> allPropertyNames,
            string propertyName,
            Dictionary<PackageSourceType, T> typeToValue)
        {
            var current = new Dictionary<T, HashSet<PackageSourceType>>();
            foreach (var pair in typeToValue)
            {
                HashSet<PackageSourceType> set;
                if (!current.TryGetValue(pair.Value, out set))
                {
                    set = new HashSet<PackageSourceType>();
                    current[pair.Value] = set;
                }

                set.Add(pair.Key);
            }

            if (current.Any())
            {
                allPropertyNames[propertyName] = current;
            }
        }

        private class PropertyData
        {
            public Dictionary<PackageSourceType, string> PackageSourceTypeToPropertyType { get; set; }
            public Dictionary<PackageSourceType, bool> PackageSourceTypeToNullable { get; set; }
            public Dictionary<PackageSourceType, string> PackageSourceTypeToOnlyUsesTargetPath { get; set; }
            public Dictionary<PackageSourceType, bool> PackageSourceTypeToKeepInContent { get; set; }
        }
    }
}
