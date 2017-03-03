using System;
using System.Collections.Generic;
using Knapcode.NuGetProtocol.Shared;

namespace Knapcode.NuGetProtocol.Reporting
{
    public class Abbreviations
    {
        private static readonly Dictionary<PackageSourceType, string> PackageSourceTypes = new Dictionary<PackageSourceType, string>
        {
            { PackageSourceType.NuGetGallery, "NG" },
            { PackageSourceType.MyGet, "MY" },
            { PackageSourceType.Vsts, "VS" },
            { PackageSourceType.NuGetServerWcf, "NS2" },
            { PackageSourceType.NuGetServerWebApi, "NS3" }
        };

        public string AbbreviatePackageSourceType(PackageSourceType type)
        {
            string abbreviation;
            if (!PackageSourceTypes.TryGetValue(type, out abbreviation))
            {
                throw new ArgumentException($"The package source type '{type}' does not have an abbreviation.");
            }

            return abbreviation;
        }
    }
}
