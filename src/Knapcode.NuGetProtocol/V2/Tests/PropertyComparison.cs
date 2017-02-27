using System.Collections.Generic;

namespace Knapcode.NuGetProtocol.V2.Tests
{
    public class PropertyComparison
    {
        public HashSet<PackageSourceType> AllTypes { get; set; }
        public List<string> PropertyNamesOnAllTypes { get; set; }
        public Dictionary<string, HashSet<PackageSourceType>> PropertyNameToTypes { get; set; }
    }
}
