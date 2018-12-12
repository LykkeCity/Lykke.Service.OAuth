using System.Threading.Tasks;
using IdentityModel.Client;
using System;
using System.Net.Http;
using IdentityModel;

namespace Lykke.Service.OAuth.Providers
{
    public interface IKycTokenProvider
    {
        Task<string> GetKycTokenAsync();
    }

    class KycTokenProvider : IKycTokenProvider
    {
        static readonly IDiscoveryCache DiscoveryCache = new DiscoveryCache("https://auth-test.lykkecloud.com/");

        public async Task<string> GetKycTokenAsync()
        {
            var response = await RequestTokenAsync();

            Console.WriteLine($"access : {response.AccessToken}");

            return response.AccessToken;
        }

        static async Task<TokenResponse> RequestTokenAsync()
        {
            var client = new HttpClient();

            var disco = await DiscoveryCache.GetAsync();
            if (disco.IsError) throw new Exception(disco.Error);

            var response = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,
                GrantType = OidcConstants.GrantTypes.ClientCredentials,
                ClientId = "sample_client",
                ClientSecret = "secret",
                //todo: set up the scopes
            });

            if (response.IsError) throw new Exception(response.Error);
            return response;
        }
    }
}
