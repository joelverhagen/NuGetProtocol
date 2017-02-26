using System;
using NuGet.Server.Core.Infrastructure;
using System.Security.Principal;

namespace Knapcode.NuGetServerWebApi.App_Start
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
