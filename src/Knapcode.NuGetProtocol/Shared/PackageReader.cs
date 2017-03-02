using NuGet.Packaging;
using System.IO;

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
    }
}
