﻿using System.Collections.Generic;
using Knapcode.NuGetProtocol.Shared;

namespace Knapcode.NuGetProtocol.V2.Tests
{
    public class PropertyComparison
    {
        /// <summary>
        /// The expected values for package properties. These can be derived by the package publisher by inspecting the
        /// the set of packages pushed to the package source. The key is the property name and the value is expected
        /// property value.
        /// </summary>
        public Dictionary<string, string> ExpectedValues { get; set; }

        /// <summary>
        /// Package properties that have been verified to have their expected values. The first key is the name of the
        /// property. The second key is the value of the property.
        /// </summary>
        public Dictionary<string, KeyValuePair<string, HashSet<PackageSourceType>>> ValuesSameAsSource { get; set; }

        /// <summary>
        /// Package properties that are intended to be different from one source to the next. The first key is the name
        /// of the property. The second key is the value found on the different package source types.
        /// </summary>
        public Dictionary<string, Dictionary<string, HashSet<PackageSourceType>>> ValuesIntendedToBeDifferent { get; set; }

        /// <summary>
        /// Packages properties that are not intended to be different from one source to the next. The first key is the
        /// name of the property. The second key is the value found on the different package source types.
        /// </summary>
        public Dictionary<string, Dictionary<string, HashSet<PackageSourceType>>> ValuesNotIntendedToBeDifferent { get; set; }
    }
}
