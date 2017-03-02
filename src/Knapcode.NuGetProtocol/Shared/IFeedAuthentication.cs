using System.Net.Http;

namespace Knapcode.NuGetProtocol.Shared
{
    public interface IFeedAuthorization
    {
        void Authenticate(HttpRequestMessage request);
    }
}
