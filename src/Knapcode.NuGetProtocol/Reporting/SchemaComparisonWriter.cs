using System.Collections.Generic;
using System.Linq;
using System.Text;
using Knapcode.NuGetProtocol.V2;
using Knapcode.NuGetProtocol.V2.Tests;

namespace Knapcode.NuGetProtocol.Reporting
{
    public class SchemaComparisonWriter
    {
        private static readonly Dictionary<string, string> TargetPathToDisplay = new Dictionary<string, string>
        {
            { Constants.SyndicationAuthorName, "`<author>` > `<name>`" },
            { Constants.SyndicationPublished, "`<published>`" },
            { Constants.SyndicationSummary, "`<summary>`" },
            { Constants.SyndicationTitle, "`<title>`" },
            { Constants.SyndicationUpdated, "`<updated>`" },
        };

        private readonly Abbreviations _abbreviations;
        private readonly MarkdownTableWriter _markdownTableWriter;

        public SchemaComparisonWriter(Abbreviations abbreviations, MarkdownTableWriter markdownTableWriter)
        {
            _abbreviations = abbreviations;
            _markdownTableWriter = markdownTableWriter;
        }

        public void Write(StringBuilder sb, SchemaComparison data)
        {
            sb.AppendLine("### Properties from `<m:properties>`");
            sb.AppendLine();
            sb.AppendLine("The following elements are available on every known server implementation.");
            sb.AppendLine();
            _markdownTableWriter.Write(
                sb,
                new[] { "Property Element", "Type" },
                data
                    .DirectPropertiesOnAllTypes
                    .OrderBy(x => x)
                    .Select(x => new[] { $"`<d:{x}>`", data.PropertyTypes[x] }));
            sb.AppendLine();
            sb.AppendLine("The following properties vary from one server implementation to the next.");
            sb.AppendLine();
            _markdownTableWriter.Write(
                sb,
                new[] { "Property Element", "Type", "[Availability](#quirk-abbreviations)" },
                data
                    .DirectPropertiesOnSomeTypes
                    .OrderBy(x => x.Key)
                    .Select(x => new[]
                    {
                        $"`<d:{x.Key}>`",
                        data.PropertyTypes[x.Key],
                        string.Join(" ", x
                            .Value
                            .Select(y => _abbreviations.AbbreviatePackageSourceType(y))
                            .OrderBy(y => y))
                    }));
            sb.AppendLine();
            sb.AppendLine("The following properties must be fetched from Atom elements, instead of from the `<m:properties>` element. In other");
            sb.AppendLine("words, clients must check child elements of `<entry>` to get all package metadata on different server implementations.");
            sb.AppendLine();
            _markdownTableWriter.Write(
                sb,
                new[] { "Equivalent Property Element", "Atom Element", "[Availability](#quirk-abbreviations)" },
                data
                    .OnlyUsesTargetPaths
                    .SelectMany(x => x
                        .Value
                        .Select(y => new[]
                        {
                            $"`<d:{x.Key}>`",
                            TargetPathToDisplay[y.Key],
                            string.Join(" ", y
                                .Value
                                .Select(z => _abbreviations.AbbreviatePackageSourceType(z))
                                .OrderBy(z => z))
                        }))
                    .OrderBy(x => x[0]));
        }
    }
}
