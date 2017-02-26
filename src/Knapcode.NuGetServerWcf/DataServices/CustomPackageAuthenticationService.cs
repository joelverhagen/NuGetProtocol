using System;
using System.Security.Principal;
using NuGet.Server.Infrastructure;

namespace Knapcode.NuGetServerWcf
{
    public class CustomPackageAuthenticationService : IPackageAuthenticationService
    {
        public bool IsAuthenticated(IPrincipal user, string apiKey, string packageId)
        {
            var expectedApiKey = Environment.GetEnvironmentVariable("NUGET_API_KEY");

            if (string.IsNullOrWhiteSpace(expectedApiKey) || string.IsNullOrWhiteSpace(apiKey))
            {
                return false;
            }

            return apiKey == expectedApiKey;
        }
    }
}
