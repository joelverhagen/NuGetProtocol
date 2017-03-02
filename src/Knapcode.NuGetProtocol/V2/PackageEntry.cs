using System.Collections.Generic;

namespace Knapcode.NuGetProtocol.V2
{
    public class PackageEntry
    {
        public List<KeyValuePair<string, string>> AtomPairs { get; set; }
        public List<KeyValuePair<string, string>> PropertyPairs { get; set; }
    }
}