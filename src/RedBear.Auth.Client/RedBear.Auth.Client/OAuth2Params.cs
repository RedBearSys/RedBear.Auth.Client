using System;
using System.Collections.Generic;

namespace RedBear.Auth.Client
{
    public class OAuth2Params
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Audience { get; set; }
        public string CertificateFilePath { get; set; }
        public IEnumerable<string> Scopes { get; set; } = new List<string>();
        public Uri AuthServerUri { get; set; }

        public string ScopesAsString()
        {
            return string.Join(" ", Scopes).Trim();
        }
    }
}
