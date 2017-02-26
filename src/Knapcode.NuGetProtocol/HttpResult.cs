using System.Net;

namespace Knapcode.NuGetProtocol
{
    public class HttpResult<T>
    {
        public HttpStatusCode StatusCode { get; set; }
        public T Data { get; set; }
    }
}
