using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Knapcode.NuGetProtocol.Reporting;
using Knapcode.NuGetProtocol.Shared;
using Knapcode.NuGetProtocol.V2;
using Knapcode.NuGetProtocol.V2.Tests;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Knapcode.NuGetProtocol.Sandbox
{
    public class Program
    {
        public static void Main(string[] args)
        {
            MainAsync(args).Wait();
        }

        private static async Task MainAsync(string[] args)
        {
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.dev.json", optional: false);
            var configuration = configurationBuilder.Build();
            var packageSourceSettings = new PackageSourceSettings();
            configuration.Bind(packageSourceSettings);
            var options = Options.Create(packageSourceSettings);
            var packageSourceProvider = new PackageSourceProvider(options);
            var packageReader = new PackageReader();
            var testData = TestData.InitializeFromRepository();
            var httpClientHandler = new HttpClientHandler();
            var loggerFactory = new LoggerFactory();
            var parser = new Parser();
            var mapper = new Mapper();
            loggerFactory.AddConsole();
            var loggingHttpHandler = new LoggingHttpHandler(httpClientHandler, loggerFactory.CreateLogger<LoggingHttpHandler>());
            using (var httpClient = new HttpClient(loggingHttpHandler))
            {
                var protocol = new Protocol(httpClient, parser);
                var client = new Client(protocol, packageReader);

                var propertyComparisonTest = new PropertyComparisonTest(packageSourceProvider, packageReader, testData, client, mapper);
                var propertyComparison = await propertyComparisonTest.ExecuteAsync();
                
                var schemaComparisonTest = new SchemaComparisonTest(packageSourceProvider, client);
                var schemaComparison = await schemaComparisonTest.ExecuteAsync();
                Console.WriteLine(JsonConvert.SerializeObject(schemaComparison, new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    Converters =
                    {
                        new StringEnumConverter()
                    }
                }));

                var abbreviation = new Abbreviations();
                var markdownTableWriter = new MarkdownTableWriter();
                var report = new SchemaComparisonWriter(abbreviation, markdownTableWriter);
                var sb = new StringBuilder();
                report.Write(sb, schemaComparison);

                Console.WriteLine(sb.ToString());
            }
        }
    }
}