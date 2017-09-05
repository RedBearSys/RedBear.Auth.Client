using System.Threading.Tasks;

namespace RedBear.Auth.Client
{
    public interface IOAuth2Client
    {
        Task<AccessToken> GetAccessTokenAsync(OAuth2Params oauthParams);
    }
}
