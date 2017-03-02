using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Knapcode.NuGetProtocol.Shared
{
    public class LoggingHttpHandler : DelegatingHandler
    {
        private readonly ILogger<LoggingHttpHandler> _logger;

        public LoggingHttpHandler(HttpMessageHandler innerHandler, ILogger<LoggingHttpHandler> logger) : base(innerHandler)
        {
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Request starting HTTP/{version} {method} {requestUri} {contentType} {contentLength}",
                request.Version,
                request.Method,
                request.RequestUri,
                request.Content?.Headers.ContentType?.ToString() ?? string.Empty,
                request.Content?.Headers.ContentLength?.ToString() ?? string.Empty);

            var stopwatch = Stopwatch.StartNew();

            var response = await base.SendAsync(request, cancellationToken);

            var totalMilliseconds = stopwatch.Elapsed.TotalMilliseconds;

            _logger.LogInformation(
                "Request finished in {totalMilliseconds}ms {statusCode} {contentType}",
                totalMilliseconds,
                (int)response.StatusCode,
                response.Content?.Headers.ContentType?.ToString() ?? string.Empty);

            return response;
        }
    }
}
