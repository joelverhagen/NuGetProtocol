using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Knapcode.NuGetProtocol.V2
{
    public class Parser
    {
        private const string Edmx = "http://schemas.microsoft.com/ado/2007/06/edmx";

        private const string Atom = "http://www.w3.org/2005/Atom";
        private const string Metadata = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";
        private const string DataServices = "http://schemas.microsoft.com/ado/2007/08/dataservices";

        private static readonly XName _xnameEdmx = XName.Get("Edmx", Edmx);
        private static readonly XName _xnameDataServices = XName.Get("DataServices", Edmx);

        private static readonly XName _xnameTargetPath = XName.Get("FC_TargetPath", Metadata);
        private static readonly XName _xnameContentKind = XName.Get("FC_ContentKind", Metadata);
        private static readonly XName _xnameKeepInContent = XName.Get("FC_KeepInContent", Metadata);
        private static readonly XName _xnameProperties = XName.Get("properties", Metadata);

        private static readonly XName _xnameFeed = XName.Get("feed", Atom);
        private static readonly XName _xnameEntry = XName.Get("entry", Atom);
        private static readonly XName _xnameLink = XName.Get("link", Atom);

        private class EdmXNames
        {
            private static readonly EdmXNames _200604 = new EdmXNames("http://schemas.microsoft.com/ado/2006/04/edm");
            private static readonly EdmXNames _200911 = new EdmXNames("http://schemas.microsoft.com/ado/2009/11/edm");
            private static readonly Dictionary<string, EdmXNames> _instances = new[]
            {
                _200604,
                _200911,
            }.ToDictionary(x => x.Namespace);

            public static EdmXNames Edm200604 => _200604;
            public static EdmXNames Edm200911 => _200911;

            public static EdmXNames GetInstance(string ns)
            {
                return _instances[ns];
            }

            private EdmXNames(string ns)
            {
                Namespace = ns;
                Schema = XName.Get("Schema", ns);
                EntityType = XName.Get("EntityType", ns);
                Property = XName.Get("Property", ns);
                EntityContainer = XName.Get("EntityContainer", ns);
                EntitySet = XName.Get("EntitySet", ns);
            }

            public string Namespace { get; }
            public XName Schema { get; }
            public XName EntityType { get; }
            public XName Property { get; }
            public XName EntityContainer { get; }
            public XName EntitySet { get; }
        }

        public async Task<Metadata> ParseMetadataAsync(Stream stream)
        {
            var doc = await ParseXmlAsync(stream);

            ValidateRootElement(doc, _xnameEdmx);

            var dataServices = doc.Root.Element(_xnameDataServices);

            // Discover EDM version.
            var edmNs = dataServices
                .Elements()
                .Where(x => x.Name.LocalName == EdmXNames.Edm200604.Schema.LocalName)
                .Select(x => x.Name.NamespaceName)
                .First();
            var edm = EdmXNames.GetInstance(edmNs);

            var packagesEntitySet = dataServices
                .Elements(edm.Schema)
                .SelectMany(x => x.Elements(edm.EntityContainer))
                .SelectMany(x => x.Elements(edm.EntitySet))
                .First(x => x.Attribute("Name").Value == "Packages");
            var packagesEntityType = packagesEntitySet.Attribute("EntityType").Value;

            var metadata = new Metadata();
            foreach (var schemaEl in dataServices.Elements(edm.Schema))
            {
                var schemaNamespace = schemaEl.Attribute("Namespace").Value;
                foreach (var entityTypeEl in schemaEl.Elements(edm.EntityType))
                {
                    var entityName = entityTypeEl.Attribute("Name").Value;
                    var entityType = $"{schemaNamespace}.{entityName}";
                    if (entityType == packagesEntityType)
                    {
                        metadata.PackageEntityType = ParseEntityType(edm, entityTypeEl);
                    }
                }
            }

            return metadata;
        }

        private EntityType ParseEntityType(EdmXNames edm, XElement entityTypeEl)
        {
            var propertyEls = entityTypeEl.Elements(edm.Property);
            var entityProperties = new List<EntityProperty>();
            foreach (var el in propertyEls)
            {
                var name = el.Attribute("Name").Value;
                var type = el.Attribute("Type").Value;
                var nullable = bool.Parse(el.Attribute("Nullable")?.Value ?? "true");
                var targetPath = el.Attribute(_xnameTargetPath)?.Value;
                var contentKind = el.Attribute(_xnameContentKind)?.Value;
                var keepInContent = bool.Parse(el.Attribute(_xnameKeepInContent)?.Value ?? "true");

                entityProperties.Add(new EntityProperty
                {
                    Name = name,
                    Type = type,
                    Nullable = nullable,
                    TargetPath = targetPath,
                    ContentKind = contentKind,
                    KeepInContent = keepInContent,
                });
            }

            return new EntityType
            {
                Properties = entityProperties,
            };
        }

        public async Task<PackageEntry> ParsePackageEntryAsync(Stream stream)
        {
            var doc = await ParseXmlAsync(stream);

            ValidateRootElement(doc, _xnameEntry);

            return ParsePackage(doc.Root);
        }

        public async Task<PackageFeed> ParsePackageFeedAsync(Stream stream)
        {
            var doc = await ParseXmlAsync(stream);

            ValidateRootElement(doc, _xnameFeed);

            var entries = doc
                .Root
                .Elements(_xnameEntry)
                .Select(ParsePackage)
                .ToList();

            return new PackageFeed
            {
                Entries = entries,
                NextUrl = GetNextUrl(doc),
            };
        }

        private PackageEntry ParsePackage(XElement entry)
        {
            var propertyPairs = entry
                .Element(_xnameProperties)
                .Elements()
                .Where(x => x.Name.NamespaceName == DataServices)
                .Select(x => new KeyValuePair<string, string>(x.Name.LocalName, x.Value))
                .ToList();
            
            var atomPairs = entry
                .Elements()
                .Where(x => x.Name.NamespaceName == Atom)
                .SelectMany(GetAtomPairs)
                .ToList();

            return new PackageEntry
            {
                AtomPairs = atomPairs,
                PropertyPairs = propertyPairs,
            };
        }
        
        private static IEnumerable<KeyValuePair<string, string>> GetAtomPairs(XElement el)
        {
            if (el.Name.NamespaceName != Atom)
            {
                yield break;
            }

            switch (el.Name.LocalName)
            {
                case Constants.AuthorPath:
                    yield return new KeyValuePair<string, string>(Constants.AuthorNamePath, el.Element("name")?.Value);
                    break;
                case Constants.CategoryPath:
                    yield return new KeyValuePair<string, string>(Constants.CategoryTermPath, el.Attribute("term")?.Value);
                    break;
                case Constants.ContentPath:
                    yield return new KeyValuePair<string, string>(Constants.ContentSrcPath, el.Attribute("src")?.Value);
                    yield return new KeyValuePair<string, string>(Constants.ContentTypePath, el.Attribute("type")?.Value);
                    break;
                case Constants.LinkPath:
                    break;
                case Constants.IdPath:
                case Constants.PublishedPath:
                case Constants.SummaryPath:
                case Constants.TitlePath:
                case Constants.UpdatedPath:
                    yield return new KeyValuePair<string, string>(el.Name.LocalName, el.Value);
                    break;
                default:
                    throw new NotSupportedException($"The Atom element '{el.Name.LocalName}' is not supported.");
            }
        }

        private string GetNextUrl(XDocument doc)
        {
            return (from e in doc.Root.Elements(_xnameLink)
                    let attr = e.Attribute("rel")
                    where attr != null && string.Equals(attr.Value, "next", StringComparison.OrdinalIgnoreCase)
                    select e.Attribute("href") into nextLink
                    where nextLink != null
                    select nextLink.Value).FirstOrDefault();
        }

        private async Task<XDocument> ParseXmlAsync(Stream stream)
        {
            using (var buffer = new MemoryStream())
            {
                await stream.CopyToAsync(buffer);

                buffer.Position = 0;

                using (var xmlReader = XmlReader.Create(buffer, new XmlReaderSettings
                {
                    CloseInput = true,
                    IgnoreWhitespace = true,
                    IgnoreComments = true,
                    IgnoreProcessingInstructions = true,
                    DtdProcessing = DtdProcessing.Ignore,
                }))
                {
                    return XDocument.Load(xmlReader, LoadOptions.None);
                }
            }
        }

        private static void ValidateRootElement(XDocument doc, XName expectedRoot)
        {
            if (doc.Root.Name != expectedRoot)
            {
                throw new InvalidDataException($"Expected root element '{expectedRoot}', found '{doc.Root.Name}'.");
            }
        }
    }
}
