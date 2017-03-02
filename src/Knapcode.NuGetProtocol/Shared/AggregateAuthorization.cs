using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Knapcode.NuGetProtocol.Shared
{
    public class AggregateAuthorization : IFeedAuthorization
    {
        private readonly List<IFeedAuthorization> _auth;

        public AggregateAuthorization(params IFeedAuthorization[] auth) : this(auth.AsEnumerable())
        {
        }

        public AggregateAuthorization(IEnumerable<IFeedAuthorization> auth)
        {
            _auth = auth.ToList();
        }

        public void Authenticate(HttpRequestMessage request)
        {
            foreach (var auth in _auth)
            {
                auth.Authenticate(request);
            }
        }
    }
}
