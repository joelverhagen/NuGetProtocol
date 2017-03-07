using System.Collections.Generic;
using Knapcode.NuGetProtocol.Shared;

namespace Knapcode.NuGetProtocol.V2.Tests
{
    public class PropertyConsistency
    {
        /// <summary>
        /// Properties that are inconsistent between different package entry sources. The first key is the package
        /// source type. The second key is the property name. The third key is the property value.
        /// </summary>
        public Dictionary<PackageSourceType, Dictionary<string, Dictionary<string, HashSet<PackageEntrySource>>>> TypeToInconsistent { get; set; }

        /// <summary>
        /// Properties that are consistent for each package entry source. The first key is the package source type. The
        /// second key is the property name. The last value is the property value.
        /// </summary>
        public Dictionary<PackageSourceType, Dictionary<string, string>> TypeToConsistent { get; set; }
    }
}