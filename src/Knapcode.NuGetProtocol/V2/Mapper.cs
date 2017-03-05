using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Knapcode.NuGetProtocol.Shared;

namespace Knapcode.NuGetProtocol.V2
{
    public class Mapper
    {
        private static readonly List<PropertyMapping> Properties = new List<PropertyMapping>
        {
            new PropertyMapping("Authors", (d, v) => d.Authors = v, d => d.Authors),
            new PropertyMapping("Copyright", (d, v) => d.Copyright = v, d => d.Copyright),
            new PropertyMapping("Created", (d, v) => d.Created = v, d => d.Created),
            new PropertyMapping("Dependencies", (d, v) => d.Dependencies = ParseDependencies(v), d => FlattenDependencies(d.Dependencies)),
            new PropertyMapping("Description", (d, v) => d.Description = v, d => d.Description),
            new PropertyMapping("DevelopmentDependency", (d, v) => d.DevelopmentDependency = v, d => d.DevelopmentDependency),
            new PropertyMapping("DownloadCount", (d, v) => d.DownloadCount = v, d => d.DownloadCount),
            new PropertyMapping("GalleryDetailsUrl", (d, v) => d.GalleryDetailsUrl = v, d => d.GalleryDetailsUrl),
            new PropertyMapping("IconUrl", (d, v) => d.IconUrl = v, d => d.IconUrl),
            new PropertyMapping("Id", (d, v) => d.Id = v, d => d.Id),
            new PropertyMapping("IsAbsoluteLatestVersion", (d, v) => d.IsAbsoluteLatestVersion = v, d => d.IsAbsoluteLatestVersion),
            new PropertyMapping("IsLatestVersion", (d, v) => d.IsLatestVersion = v, d => d.IsLatestVersion),
            new PropertyMapping("IsPrerelease", (d, v) => d.IsPrerelease = v, d => d.IsPrerelease),
            new PropertyMapping("Language", (d, v) => d.Language = v, d => d.Language),
            new PropertyMapping("LastEdited", (d, v) => d.LastEdited = v, d => d.LastEdited),
            new PropertyMapping("LastUpdated", (d, v) => d.LastUpdated = v, d => d.LastUpdated),
            new PropertyMapping("LicenseNames", (d, v) => d.LicenseNames = v, d => d.LicenseNames),
            new PropertyMapping("LicenseReportUrl", (d, v) => d.LicenseReportUrl = v, d => d.LicenseReportUrl),
            new PropertyMapping("LicenseUrl", (d, v) => d.LicenseUrl = v, d => d.LicenseUrl),
            new PropertyMapping("Listed", (d, v) => d.Listed = v, d => d.Listed),
            new PropertyMapping("MinClientVersion", (d, v) => d.MinClientVersion = v, d => d.MinClientVersion),
            new PropertyMapping("NormalizedVersion", (d, v) => d.NormalizedVersion = v, d => d.NormalizedVersion),
            new PropertyMapping("Owners", (d, v) => d.Owners = v, d => d.Owners),
            new PropertyMapping("PackageHash", (d, v) => d.PackageHash = v, d => d.PackageHash),
            new PropertyMapping("PackageHashAlgorithm", (d, v) => d.PackageHashAlgorithm = v, d => d.PackageHashAlgorithm),
            new PropertyMapping("PackageSize", (d, v) => d.PackageSize = v, d => d.PackageSize),
            new PropertyMapping("ProjectUrl", (d, v) => d.ProjectUrl = v, d => d.ProjectUrl),
            new PropertyMapping("Published", (d, v) => d.Published = v, d => d.Published),
            new PropertyMapping("ReleaseNotes", (d, v) => d.ReleaseNotes = v, d => d.ReleaseNotes),
            new PropertyMapping("ReportAbuseUrl", (d, v) => d.ReportAbuseUrl = v, d => d.ReportAbuseUrl),
            new PropertyMapping("RequireLicenseAcceptance", (d, v) => d.RequireLicenseAcceptance = v, d => d.RequireLicenseAcceptance),
            new PropertyMapping("Summary", (d, v) => d.Summary = v, d => d.Summary),
            new PropertyMapping("Tags", (d, v) => d.Tags = v, d => d.Tags),
            new PropertyMapping("Title", (d, v) => d.Title = v, d => d.Title),
            new PropertyMapping("Version", (d, v) => d.Version = v, d => d.Version),
            new PropertyMapping("VersionDownloadCount", (d, v) => d.VersionDownloadCount = v, d => d.VersionDownloadCount),
        };

        private static readonly Dictionary<string, Action<Package, string>> SetProperty = Properties
            .ToDictionary(x => x.Name, x => x.Set);

        private static readonly Dictionary<string, Func<Package, string>> GetProperty = Properties
            .ToDictionary(x => x.Name, x => x.Get);

        /*
         * SyndicationAuthorEmail: The atom:email child element of the atom:author element.
         * SyndicationAuthorName: The atom:name child element of the atom:author element.
         * SyndicationAuthorUri: The atom:uri child element of the atom:author element.
         * SyndicationContributorEmail: The atom:email child element of the atom:contributor element.
         * SyndicationContributorName: The atom:name child element of the atom:contributor element.
         * SyndicationContributorUri: The atom:uri child element of the atom:contributor element.
         * SyndicationPublished: The atom:published element.
         * SyndicationRights: The atom:rights element.
         * SyndicationSummary: The atom:summary element.
         * SyndicationTitle: The atom:title element.
         * SyndicationUpdated: The atom:updated element
         */
        private static readonly Dictionary<string, string> TargetPathToChildPath = new Dictionary<string, string>
        {
            { Constants.SyndicationAuthorName, Constants.AuthorNamePath },
            { Constants.SyndicationPublished, Constants.PublishedPath },
            { Constants.SyndicationSummary, Constants.SummaryPath },
            { Constants.SyndicationTitle, Constants.TitlePath },
            { Constants.SyndicationUpdated, Constants.UpdatedPath },
        };

        public PackageEntry GetPackageEntry(Package package)
        {
            var propertyPairs = new List<KeyValuePair<string, string>>();
            foreach (var property in Properties)
            {
                var value = property.Get(package);
                if (value == null)
                {
                    continue;
                }

                propertyPairs.Add(new KeyValuePair<string, string>(property.Name, value));
            }

            return new PackageEntry
            {
                AtomPairs = new List<KeyValuePair<string, string>>(),
                PropertyPairs = propertyPairs,
            };
        }

        public List<KeyValuePair<string, string>> GetPropertyPairs(Metadata metadata, PackageEntry entry)
        {
            var output = new List<KeyValuePair<string, string>>();

            output.AddRange(entry.PropertyPairs);

            var atomProperties = entry
                .AtomPairs
                .ToLookup(x => x.Key, x => x.Value);

            foreach (var entityProperty in metadata.PackageEntityType.Properties)
            {
                if (entityProperty.TargetPath == null)
                {
                    continue;
                }

                string childPath;
                if (!TargetPathToChildPath.TryGetValue(entityProperty.TargetPath, out childPath))
                {
                    throw new NotSupportedException($"The target path '{entityProperty.TargetPath}' is not supported.");
                }

                foreach (var value in atomProperties[childPath])
                {
                    output.Add(new KeyValuePair<string, string>(entityProperty.Name, value));
                }
            }

            return output;
        }

        public Package GetPackage(Metadata metadata, PackageEntry entry)
        {
            var package = new Package();
            var propertyPairs = GetPropertyPairs(metadata, entry);
            foreach (var pair in propertyPairs)
            {
                Func<Package, string> getProperty;
                if (!GetProperty.TryGetValue(pair.Key, out getProperty))
                {
                    throw new NotSupportedException($"The property element '{pair.Key}' is not supported.");
                }

                var existingValue = getProperty(package);
                if (existingValue != null)
                {
                    continue;
                }

                var setProperty = SetProperty[pair.Key];
                setProperty(package, pair.Value);
            }

            return package;
        }

        private static string FlattenDependencies(IEnumerable<PackageDependency> dependencies)
        {
            var output = new StringBuilder();
            var first = true;
            foreach (var dependency in dependencies)
            {
                if (!first)
                {
                    output.Append('|');
                }

                first = false;

                output.Append(dependency.Id);

                if (dependency.VersionRange != null)
                {
                    output.Append(':');
                    output.Append(dependency.VersionRange);

                    if (dependency.Framework != null)
                    {
                        output.Append(':');
                        output.Append(dependency.Framework);
                    }
                }
            }

            return output.ToString();
        }

        private static List<PackageDependency> ParseDependencies(string flattened)
        {
            var output = new List<PackageDependency>();
            foreach (var unparsedDependency in flattened.Split('|'))
            {
                var pieces = unparsedDependency.Split(':');

                var packageDependency = new PackageDependency();
                packageDependency.Id = pieces[0];

                if (pieces.Length > 1)
                {
                    packageDependency.VersionRange = pieces[1];
                }

                if (pieces.Length > 2)
                {
                    packageDependency.Framework = pieces[2];
                }

                output.Add(packageDependency);
            }

            return output;
        }

        private class PropertyMapping
        {
            public PropertyMapping(string name, Action<Package, string> set, Func<Package, string> get)
            {
                Name = name;
                Set = set;
                Get = get;
            }

            public string Name { get; }
            public Action<Package, string> Set { get; }
            public Func<Package, string> Get { get; }
        }
    }
}
