using System.Linq;
using System.Text;
using Knapcode.NuGetProtocol.V2.Tests;

namespace Knapcode.NuGetProtocol.Reporting
{
    public class PropertyComparisonWriter
    {
        private readonly Abbreviations _abbreviations;
        private readonly MarkdownTableWriter _markdownTableWriter;

        public PropertyComparisonWriter(Abbreviations abbreviations, MarkdownTableWriter markdownTableWriter)
        {
            _abbreviations = abbreviations;
            _markdownTableWriter = markdownTableWriter;
        }

        public void Write(StringBuilder sb, PropertyComparison data)
        {
            sb.AppendLine("### Properties from `<m:properties>`");
            sb.AppendLine();
            _markdownTableWriter.Write(
                sb,
                new[] { "Element", "Type" },
                data
                    .DirectPropertiesOnAllTypes
                    .OrderBy(x => x)
                    .Select(x => new[] { $"`<d:{x}>`", data.PropertyTypes[x] }));
            sb.AppendLine();
            _markdownTableWriter.Write(
                sb,
                new[] { "Element", "Type", "[Availability](#quirk-abbreviations)" },
                data
                    .DirectPropertiesOnSomeTypes
                    .OrderBy(x => x.Key)
                    .Select(x => new[]
                    {
                        $"`<d:{x.Key}>`",
                        data.PropertyTypes[x.Key],
                        string.Join(" ", x
                            .Value
                            .OrderBy(y => y)
                            .Select(y => _abbreviations.AbbreviatePackageSourceType(y)))
                    }));
        }
    }
}
