using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Knapcode.NuGetProtocol.Shared;

namespace Knapcode.NuGetProtocol.V2
{
    public class Protocol
    {
        private readonly HttpClient _httpClient;
        private readonly Parser _parser;

        public Protocol(HttpClient httpClient, Parser parser)
        {
            _httpClient = httpClient;
            _parser = parser;
        }

        public async Task<Metadata> GetMetadataAsync(PackageSource source)
        {
            var uri = $"{source.SourceUri}/$metadata";

            using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
            {
                source.SourceAuthorization.Authenticate(request);

                using (var response = await _httpClient.SendAsync(request))
                {
                    VerifyStatusCode(response, HttpStatusCode.OK);

                    using (var stream = await response.Content.ReadAsStreamAsync())
                    {
                        return await _parser.ParseMetadataAsync(stream);
                    }
                }
            }
        }

        public async Task<HttpStatusCode> PushPackageAsync(PackageSource source, Stream package)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Put, source.PushUri))
            {
                source.PushAuthorization.Authenticate(request);

                var content = new MultipartFormDataContent();
                request.Content = content;
                request.Headers.TransferEncodingChunked = true;
                var packageContent = new StreamContent(package);
                packageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
                content.Add(packageContent, "package", "package.nupkg");

                using (var response = await _httpClient.SendAsync(request))
                {
                    VerifyStatusCode(response, HttpStatusCode.Created, HttpStatusCode.Accepted);

                    return response.StatusCode;
                }
            }
        }

        public async Task<HttpResult<PackageEntry>> GetPackageAsync(PackageSource source, PackageIdentity package)
        {
            var uri = $"{source.SourceUri}/Packages(Id='{Uri.EscapeDataString(package.Id)}',Version='{Uri.EscapeDataString(package.Version)}')";

            using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
            {
                source.SourceAuthorization.Authenticate(request);

                using (var response = await _httpClient.SendAsync(request))
                {
                    VerifyStatusCode(response, HttpStatusCode.OK, HttpStatusCode.NotFound);

                    var output = new HttpResult<PackageEntry>
                    {
                        StatusCode = response.StatusCode,
                    };

                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        return output;
                    }

                    using (var stream = await response.Content.ReadAsStreamAsync())
                    {
                        output.Data = await _parser.ParsePackageEntryAsync(stream);
                    }

                    return output;
                }
            }
        }

        private static void VerifyStatusCode(HttpResponseMessage response, params HttpStatusCode[] statusCodes)
        {
            if (!statusCodes.Contains(response.StatusCode))
            {
                throw new HttpRequestException($"Unexpected HTTP status code {(int)response.StatusCode} encountered.");
            }
        }
    }
}
