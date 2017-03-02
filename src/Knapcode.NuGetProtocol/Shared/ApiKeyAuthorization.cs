using System.Net.Http;

namespace Knapcode.NuGetProtocol.Shared
{
    public class ApiKeyAuthorization : IFeedAuthorization
    {
        private readonly string _apiKey;

        public ApiKeyAuthorization(string apiKey)
        {
            _apiKey = apiKey;
        }

        public void Authenticate(HttpRequestMessage request)
        {
            request.Headers.Add("X-NuGet-ApiKey", _apiKey);
        }
    }
}
