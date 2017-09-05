using System;

namespace RedBear.Auth.Client
{
    public class AccessToken
    {
        public string Token { get; set; }
        public DateTime Expires { get; set; }
    }
}
