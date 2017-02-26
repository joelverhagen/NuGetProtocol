using System.Net;

namespace Knapcode.NuGetProtocol.V2
{
    public class ConditionalPushResult
    {
        public bool Pushed { get; set; }
        public HttpStatusCode? StatusCode { get; set; } 
    }
}
