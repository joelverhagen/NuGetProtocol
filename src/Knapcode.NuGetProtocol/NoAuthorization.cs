using System.Net.Http;

namespace Knapcode.NuGetProtocol
{
    public class NoAuthorization : IFeedAuthorization
    {
        public void Authenticate(HttpRequestMessage request)
        {
        }
    }
}
