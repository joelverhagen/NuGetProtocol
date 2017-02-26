using System;
using NuGet.Server;
using NuGet.Server.Infrastructure;
using NuGet.Server.Publishing;

namespace Knapcode.NuGetServerWcf
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

            if (type == typeof(IPackageService))
            {
                return new PackageService(
                    (IServerPackageRepository) Resolve(typeof(IServerPackageRepository)),
                    (IPackageAuthenticationService) Resolve(typeof(IPackageAuthenticationService)));
            }

            return _innerServiceResolver.Resolve(type);
        }
    }
}
