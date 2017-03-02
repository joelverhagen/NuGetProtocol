using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
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
            loggerFactory.AddConsole();
            var loggingHttpHandler = new LoggingHttpHandler(httpClientHandler, loggerFactory.CreateLogger<LoggingHttpHandler>());
            using (var httpClient = new HttpClient(loggingHttpHandler))
            {
                var parser = new Parser();
                var protocol = new Protocol(httpClient, parser);
                var client = new Client(protocol, packageReader);
                var test = new PropertyComparisonTest(packageSourceProvider, packageReader, testData, client);

                var sources = packageSourceProvider.GetPackageSouces().ToList();

                // await protocol.GetMetadataAsync(sources.Last());
                var result = await test.ExecuteAsync();
                Console.WriteLine(JsonConvert.SerializeObject(result, new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    Converters =
                    {
                        new StringEnumConverter()
                    }
                }));
            }
        }
    }
}