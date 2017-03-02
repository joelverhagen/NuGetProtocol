using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Knapcode.NuGetProtocol.Shared
{
    public class BasicAuthorization : IFeedAuthorization
    {
        private readonly string _username;
        private readonly string _password;

        public BasicAuthorization(string username, string password)
        {
            _username = username;
            _password = password;
        }

        public void Authenticate(HttpRequestMessage request)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Basic",
                Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_username}:{_password}")));
        }
    }
}
