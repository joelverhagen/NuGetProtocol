using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Knapcode.NuGetProtocol.Reporting
{
    public class MarkdownTableWriterTest
    {
        [Fact]
        public void Write_GeneratesExpectedTable()
        {
            // Arrange
            var target = new MarkdownTableWriter();
            var header = new[] { "Short", "Header longer than the data", "A" };
            var data = new[]
            {
                new[] { "B", "C", "Third cell" },
                new[] { "Longer than the header", string.Empty, "LOL" }
            };
            var sb = new StringBuilder();

            // Act
            target.Write(sb, header, data);

            // Act
            var actual = sb.ToString();
            var expected =
                "Short                   | Header longer than the data  | A" + Environment.NewLine +
                "----------------------- | ---------------------------- | -----------" + Environment.NewLine +
                "B                       | C                            | Third cell" + Environment.NewLine +
                "Longer than the header  |                              | LOL" + Environment.NewLine;
            Assert.Equal(expected, actual);
        }
    }
}
