using System;
using NuGet.Server;
using NuGet.Server.Core.Infrastructure;

namespace Knapcode.NuGetServerWebApi.App_Start
{
    public class CustomServiceResolver : IServiceResolver
    {
        private readonly IServiceResolver _innerServiceResolver = new DefaultServiceResolver();

        public object Resolve(Type type)
        {
            if (type == typeof(IPackageAuthenticationService))
            {
                return new CustomPackageAuthenticationService();
            }

            return _innerServiceResolver.Resolve(type);
        }
    }
}
