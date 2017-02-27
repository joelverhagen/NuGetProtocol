using System;
using System.Net;

namespace Knapcode.NuGetProtocol.V2
{
    public class ConditionalPushResult
    {
        public bool PackageAlreadyExists { get; set; }
        public HttpResult<PackageEntry> PackageResult { get; set; }
        public bool? PackagePushSuccessfully { get; set; }
        public HttpStatusCode? PushStatusCode { get; set; }
        public TimeSpan? TimeToPush { get; set; }
        public TimeSpan? TimeToBeAvailable { get; set; }
    }
}
