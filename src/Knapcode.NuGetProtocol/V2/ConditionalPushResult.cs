using System;
using System.Net;

namespace Knapcode.NuGetProtocol.V2
{
    public class ConditionalPushResult
    {
        public bool PushAttempted { get; set; }
        public bool? PackageExists { get; set; }
        public HttpStatusCode? StatusCode { get; set; }
        public TimeSpan? TimeToPush { get; set; }
        public TimeSpan? TimeToBeAvailable { get; set; }
    }
}
