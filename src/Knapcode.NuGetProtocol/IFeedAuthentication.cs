using System.Net.Http;

namespace Knapcode.NuGetProtocol
{
    public interface IFeedAuthorization
    {
        void Authenticate(HttpRequestMessage request);
    }
}
