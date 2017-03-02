using System;
using System.Collections.Generic;
using System.Linq;

namespace Knapcode.NuGetProtocol.V2
{
    public class Mapper
    {
        private static readonly Dictionary<string, Action<Package, string>> SetProperty = new Dictionary<string, Action<Package, string>>
        {
            { "Authors", (d, v) => d.Authors = v },
            { "Copyright", (d, v) => d.Copyright = v },
            { "Created", (d, v) => d.Created = v },
            { "Dependencies", (d, v) => d.Dependencies = v },
            { "Description", (d, v) => d.Description = v },
            { "DevelopmentDependency", (d, v) => d.DevelopmentDependency = v },
            { "DownloadCount", (d, v) => d.DownloadCount = v },
            { "GalleryDetailsUrl", (d, v) => d.GalleryDetailsUrl = v },
            { "IconUrl", (d, v) => d.IconUrl = v },
            { "Id", (d, v) => d.Id = v },
            { "IsAbsoluteLatestVersion", (d, v) => d.IsAbsoluteLatestVersion = v },
            { "IsLatestVersion", (d, v) => d.IsLatestVersion = v },
            { "IsPrerelease", (d, v) => d.IsPrerelease = v },
            { "Language", (d, v) => d.Language = v },
            { "LastEdited", (d, v) => d.LastEdited = v },
            { "LastUpdated", (d, v) => d.LastUpdated = v },
            { "LicenseNames", (d, v) => d.LicenseNames = v },
            { "LicenseReportUrl", (d, v) => d.LicenseReportUrl = v },
            { "LicenseUrl", (d, v) => d.LicenseUrl = v },
            { "Listed", (d, v) => d.Listed = v },
            { "MinClientVersion", (d, v) => d.MinClientVersion = v },
            { "NormalizedVersion", (d, v) => d.NormalizedVersion = v },
            { "Owners", (d, v) => d.Owners = v },
            { "PackageHash", (d, v) => d.PackageHash = v },
            { "PackageHashAlgorithm", (d, v) => d.PackageHashAlgorithm = v },
            { "PackageSize", (d, v) => d.PackageSize = v },
            { "ProjectUrl", (d, v) => d.ProjectUrl = v },
            { "Published", (d, v) => d.Published = v },
            { "ReleaseNotes", (d, v) => d.ReleaseNotes = v },
            { "ReportAbuseUrl", (d, v) => d.ReportAbuseUrl = v },
            { "RequireLicenseAcceptance", (d, v) => d.RequireLicenseAcceptance = v },
            { "Summary", (d, v) => d.Summary = v },
            { "Tags", (d, v) => d.Tags = v },
            { "Title", (d, v) => d.Title = v },
            { "Version", (d, v) => d.Version = v },
            { "VersionDownloadCount", (d, v) => d.VersionDownloadCount = v },
        };

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
            { "SyndicationAuthorName", Constants.AuthorNamePath },
            { "SyndicationPublished", Constants.PublishedPath },
            { "SyndicationSummary", Constants.SummaryPath },
            { "SyndicationTitle", Constants.TitlePath },
            { "SyndicationUpdated", Constants.UpdatedPath },
        };

        public Package GetPackage(Metadata metadata, PackageEntry entry)
        {
            var package = new Package();

            foreach (var pair in entry.PropertyPairs)
            {
                Action<Package, string> setProperty;
                if (!SetProperty.TryGetValue(pair.Key, out setProperty))
                {
                    throw new NotSupportedException($"The property element '{pair.Key}' is not supported.");
                }

                setProperty(package, pair.Value);
            }

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

                var value = atomProperties[childPath].LastOrDefault();
                if (value == null)
                {
                    continue;
                }

                Action<Package, string> setProperty;
                if (!SetProperty.TryGetValue(entityProperty.Name, out setProperty))
                {
                    throw new NotSupportedException($"The property element '{entityProperty.Name}' is not supported.");
                }

                setProperty(package, value);
            }

            return package;
        }
    }
}
