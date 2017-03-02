using System.Net.Http;

namespace Knapcode.NuGetProtocol.Shared
{
    public class NoAuthorization : IFeedAuthorization
    {
        public void Authenticate(HttpRequestMessage request)
        {
        }
    }
}
