using System.Collections.Generic;

namespace Knapcode.NuGetProtocol.V2
{
    public class PackageFeed
    {
        public List<PackageEntry> Entries { get; set; }
        public string NextUrl { get; set; }
    }
}