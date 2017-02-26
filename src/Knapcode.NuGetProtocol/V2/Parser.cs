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
        private const string Atom = "http://www.w3.org/2005/Atom";
        private const string Metadata = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";
        private const string DataServices = "http://schemas.microsoft.com/ado/2007/08/dataservices";

        private static readonly XName _xnameFeed = XName.Get("feed", Atom);
        private static readonly XName _xnameEntry = XName.Get("entry", Atom);

        private static readonly XName _xnameAuthor = XName.Get("author", Atom);
        private static readonly XName _xnameContent = XName.Get("content", Atom);
        private static readonly XName _xnameLink = XName.Get("link", Atom);
        private static readonly XName _xnameName = XName.Get("name", Atom);
        private static readonly XName _xnameSummary = XName.Get("summary", Atom);
        private static readonly XName _xnameTitle = XName.Get("title", Atom);
        private static readonly XName _xnameProperties = XName.Get("properties", Metadata);
        
        public async Task<PackageEntry> ParsePackageEntryAsync(Stream stream)
        {
            var doc = await ParseXmlAsync(stream);

            if (doc.Root.Name != _xnameEntry)
            {
                throw new InvalidDataException($"Expected root element '{_xnameEntry}', found '{doc.Root.Name}'.");
            }

            return ParsePackage(doc.Root);
        }

        public async Task<PackageFeed> ParsePackageFeedAsync(Stream stream)
        {
            var doc = await ParseXmlAsync(stream);

            if (doc.Root.Name != _xnameFeed)
            {
                throw new InvalidDataException($"Expected root element '{_xnameFeed}', found '{doc.Root.Name}'.");
            }

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

        private PackageEntry ParsePackage(XElement element)
        {
            var downloadUrl = element
                .Element(_xnameContent)
                .Attribute("src")
                .Value;

            var title = element
                .Element(_xnameTitle)?
                .Value;

            var summary = element
                .Element(_xnameSummary)?
                .Value;

            var authors = element
                .Element(_xnameAuthor)?
                .Elements(_xnameName)
                .Select(e => e.Value)
                .ToList();

            var properties = element.Element(_xnameProperties);
            var propertyNames = new HashSet<string>(properties
                .Descendants()
                .Where(x => x.Name.NamespaceName == DataServices)
                .Select(x => x.Name.LocalName));

            return new PackageEntry
            {
                PropertyNames = propertyNames,
            };
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
    }
}