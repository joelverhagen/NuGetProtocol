using System.Collections.Generic;
using Knapcode.NuGetProtocol.Shared;

namespace Knapcode.NuGetProtocol.V2.Tests
{
    public class PropertyComparison
    {
        /// <summary>
        /// All of the package source types evaluated.
        /// </summary>
        public HashSet<PackageSourceType> AllTypes { get; set; }

        /// <summary>
        /// Properties found on all package source types. These are properties mentioned in the metadata document.
        /// </summary>
        public List<string> PropertiesOnAllTypes { get; set; }

        /// <summary>
        /// Properties found on some package source types. These are properties mentioned in the metadata document. The
        /// key is the property name and the value is the set of package source types that have this property.
        /// </summary>
        public Dictionary<string, HashSet<PackageSourceType>> PropertiesOnSomeTypes { get; set; }

        /// <summary>
        /// Properties found on all package source types. These are properties available in the property bag, and not
        /// using target paths (i.e. values not in children of the Atom <code>&lt;entry&gt;</code> element).
        /// </summary>
        public List<string> DirectPropertiesOnAllTypes { get; set; }

        /// <summary>
        /// Properties found on some package source types. These are properties available in the property bag, and not
        /// using target paths (i.e. values not in children of the Atom <code>&lt;entry&gt;</code> element). The key is
        /// the property name and the value is the set of package source types that have this property.
        /// </summary>
        public Dictionary<string, HashSet<PackageSourceType>> DirectPropertiesOnSomeTypes { get; set; }

        /// <summary>
        /// Properties that use target paths (i.e. children of the Atom <code>&lt;entry&gt;</code> element). The first
        /// key is the property name. The second key is the target path.
        /// </summary>
        public Dictionary<string, Dictionary<string, HashSet<PackageSourceType>>> TargetPaths { get; set; }

        /// <summary>
        /// The types for properties that vary from one source to the next. The first key is the property name. The
        /// second key is the property type.
        /// </summary>
        public Dictionary<string, Dictionary<string, HashSet<PackageSourceType>>> DifferingPropertyTypes { get; set; }

        /// <summary>
        /// The types for properties that are the same on all package source types. The key is the property name. The
        /// value is the property type.
        /// </summary>
        public Dictionary<string, string> PropertyTypes { get; set; }

        /// <summary>
        /// The nullability for properties that vary from one source to the next. The first key is the property name.
        /// The second key is the nullability.
        /// </summary>
        public Dictionary<string, Dictionary<bool, HashSet<PackageSourceType>>> DifferingNullability { get; set; }

        /// <summary>
        /// The nullability for properties that are the same on all package source types. The key is the property name.
        /// The value is the nullability.
        /// </summary>
        public Dictionary<string, bool> Nullability { get; set; }

        /// <summary>
        /// Whether or not value should be kept in content when the setting varies from one source to the next. The
        /// first key is the property name. The second key is whether or not to keep the property in content.
        /// </summary>
        public Dictionary<string, Dictionary<bool, HashSet<PackageSourceType>>> DifferingKeepInContent { get; set; }

        /// <summary>
        /// Whether or not value should be kept in content when the setting is the same on all package source types.
        /// The key is the property name. The value is whether or not to keep the property in content.
        /// </summary>
        public Dictionary<string, bool> KeepInContent { get; set; }
    }
}
