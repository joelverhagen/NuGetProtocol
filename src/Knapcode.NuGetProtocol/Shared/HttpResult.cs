using System.Net;

namespace Knapcode.NuGetProtocol.Shared
{
    public class HttpResult<T>
    {
        public HttpStatusCode StatusCode { get; set; }
        public T Data { get; set; }
    }
}
