using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PresenceLight.Worker
{
    public class TokenProvider
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
