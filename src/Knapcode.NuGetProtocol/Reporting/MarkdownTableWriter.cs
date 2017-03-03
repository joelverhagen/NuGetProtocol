using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Knapcode.NuGetProtocol.Reporting
{
    public class MarkdownTableWriter
    {
        public void Write(StringBuilder sb, IEnumerable<object> headerRow, IEnumerable<IEnumerable<object>> dataRows)
        {
            var headerList = headerRow
                .Select(x => x.ToString())
                .ToList();

            if (headerList.Count == 0)
            {
                throw new ArgumentException("The must be at least one cell in the header row.", nameof(headerRow));
            }

            var dataList = dataRows
                .Select(x => x
                    .Select(y => y.ToString())
                    .ToList())
                .ToList();

            if (headerList.Count != dataList.Min(x => x.Count) || headerList.Count != dataList.Max(x => x.Count))
            {
                throw new ArgumentException("The data rows must have the same number of cells as the header row.", nameof(dataRows));
            }
            
            var columnWidths = new[] { headerList }
                .Concat(dataList)
                .Select(x => x.Select(y => y.Length))
                .Aggregate((a, b) => a.Zip(b, (ae, be) => Math.Max(ae, be)))
                .Select(x => x + 1)
                .ToList();

            WriteRow(sb, columnWidths, headerList);
            WriteDivider(sb, columnWidths);
            foreach (var row in dataList)
            {
                WriteRow(sb, columnWidths, row);
            }
        }

        private void WriteDivider(StringBuilder sb, List<int> columnWidths)
        {
            WriteRow(
                sb,
                columnWidths,
                columnWidths
                    .Select(x => new string('-', x))
                    .ToList());
        }

        private void WriteRow(StringBuilder sb, List<int> columnWidths, List<string> row)
        {
            for (var i = 0; i < row.Count; i++)
            {
                if (i > 0)
                {
                    sb.Append(' ');
                    sb.Append('|');
                    sb.Append(' ');
                }

                sb.Append(row[i]);

                if (i < row.Count - 1)
                {
                    sb.Append(' ', columnWidths[i] - row[i].Length);
                }
            }

            sb.AppendLine();
        }
    }
}
