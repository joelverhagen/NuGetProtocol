using NuGet.Packaging;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;
using System;
using System.Threading.Tasks;

namespace Knapcode.NuGetProtocol.Shared
{
    public class PackageReader
    {
        public PackageIdentity GetPackageIdentity(Stream package)
        {
            string id;
            string version;
            using (var reader = new PackageArchiveReader(package, leaveStreamOpen: true))
            {
                id = reader.NuspecReader.GetId();
                version = reader.NuspecReader.GetVersion().ToNormalizedString();
            }

            return new PackageIdentity(id, version);
        }

        public async Task<Package> GetPackageAsync(Stream package, bool isAbsoluteLatestVersion, bool isLatestVersion, bool listed)
        {
            var positionBefore = package.Position;

            string packageHash;
            long packageSize = 0;
            using (var sha512 = IncrementalHash.CreateHash(HashAlgorithmName.SHA512))
            {
                var buffer = new byte[16 * 512];
                int read;
                while ((read = await package.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    packageSize += read;
                    sha512.AppendData(buffer, 0, read);
                }

                var hashBytes = sha512.GetHashAndReset();
                packageHash = Convert.ToBase64String(hashBytes);
            }

            package.Position = positionBefore;

            using (var reader = new PackageArchiveReader(package, leaveStreamOpen: true))
            {
                var nuspec = reader.NuspecReader;

                return new Package
                {
                    Authors = nuspec.GetAuthors(),
                    Copyright = nuspec.GetCopyright(),
                    Dependencies = GetPackageDependencies(nuspec.GetDependencyGroups()),
                    Description = nuspec.GetDescription(),
                    DevelopmentDependency = ToString(nuspec.GetDevelopmentDependency()),
                    IconUrl = nuspec.GetIconUrl(),
                    Id = nuspec.GetId(),
                    IsAbsoluteLatestVersion = ToString(isAbsoluteLatestVersion),
                    IsLatestVersion = ToString(isLatestVersion),
                    IsPrerelease = ToString(nuspec.GetVersion().IsPrerelease),
                    Language = nuspec.GetLanguage(),
                    LicenseUrl = nuspec.GetLicenseUrl(),
                    Listed = ToString(listed),
                    MinClientVersion = nuspec.GetMinClientVersion()?.ToFullString(),
                    NormalizedVersion = nuspec.GetVersion().ToNormalizedString(),
                    Owners = nuspec.GetOwners(),
                    PackageHash = packageHash,
                    PackageHashAlgorithm = "SHA512",
                    PackageSize = packageSize.ToString(),
                    ProjectUrl = nuspec.GetProjectUrl(),
                    ReleaseNotes = nuspec.GetReleaseNotes(),
                    RequireLicenseAcceptance = ToString(nuspec.GetRequireLicenseAcceptance()),
                    Summary = nuspec.GetSummary(),
                    Tags = nuspec.GetTags(),
                    Title = nuspec.GetTitle(),
                    Version = nuspec.GetVersion().ToFullString(),
                };
            }
        }

        private static string ToString(bool input)
        {
            return input ? "true" : "false";
        }

        private static List<PackageDependency> GetPackageDependencies(IEnumerable<PackageDependencyGroup> groups)
        {
            var output = new List<PackageDependency>();

            foreach (var group in groups)
            {
                foreach (var dependency in group.Packages)
                {
                    output.Add(new PackageDependency
                    {
                        Id = dependency.Id,
                        VersionRange = dependency.VersionRange.ToLegacyShortString(),
                        Framework = group.TargetFramework.GetShortFolderName(),
                    });
                }
            }

            return output;
        }
    }
}
