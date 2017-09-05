using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace RedBear.Auth.Client
{
    public class OAuth2Client : IOAuth2Client
    {
        private readonly HttpClient _httpClient;
        private readonly IFileReader _fileReader;

        public OAuth2Client(HttpClient httpClient, IFileReader fileReader)
        {
            _httpClient = httpClient;
            _fileReader = fileReader;
        }

        public async Task<AccessToken> GetAccessTokenAsync(OAuth2Params oauthParams)
        {
            var jwt = GetJwt(oauthParams);

            var form = new FormUrlEncodedContent(new []
            {
                new KeyValuePair<string, string>("grant_type", "urn:ietf:params:oauth:grant-type:jwt-bearer"),
                new KeyValuePair<string, string>("assertion", jwt),
                new KeyValuePair<string, string>("client_id", oauthParams.ClientId),
                new KeyValuePair<string, string>("client_secret", oauthParams.ClientSecret) 
            });

            var request = new HttpRequestMessage(HttpMethod.Post, oauthParams.AuthServerUri) {Content = form};
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new WebException($"Unable to obtain an access token. HTTP Status Code: {response.StatusCode}");
            }

            var content = JObject.Parse(await response.Content.ReadAsStringAsync());

            return new AccessToken
            {
                Token = content["access_token"].Value<string>(),
                Expires = DateTime.UtcNow.AddSeconds(content["expires_in"].Value<int>())
            };
        }

        private string GetJwt(OAuth2Params oauthParams)
        {
            _fileReader.Open(oauthParams.CertificateFilePath);
            var pemReader = new PemReader(_fileReader.Reader);
            var keyPair = (AsymmetricCipherKeyPair)pemReader.ReadObject();
            var rsaParams = DotNetUtilities.ToRSAParameters((RsaPrivateCrtKeyParameters)keyPair.Private);

            var rsa = new RSACryptoServiceProvider(2048);
            rsa.ImportParameters(rsaParams);

            var tokenHandler = new JwtSecurityTokenHandler();
            var now = DateTime.UtcNow;

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("scope", oauthParams.ScopesAsString())
                }),
                Issuer = oauthParams.ClientId,
                Audience = oauthParams.Audience,
                Expires = now.AddHours(1),
                SigningCredentials = new SigningCredentials(new RsaSecurityKey(rsa), "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256")
            };

            return tokenHandler.CreateEncodedJwt(tokenDescriptor);
        }
    }
}
