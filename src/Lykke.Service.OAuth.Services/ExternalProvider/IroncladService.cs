using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Core.ExternalProvider;
using IdentityModel.Client;
using Ironclad.Client;

namespace Lykke.Service.OAuth.Services.ExternalProvider
{
    public class IroncladService : IIroncladService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IDiscoveryCache _discoveryCache;

        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _authority ;
        
        public IroncladService(
            IdentityProviderSettings ironcladSettings,
            IHttpClientFactory httpClientFactory,
            IDiscoveryCache discoveryCache)
        {
            _httpClientFactory = httpClientFactory;
            _discoveryCache = discoveryCache;

            if (ironcladSettings != null)
            {
                _clientId = ironcladSettings.ClientId;
                _clientSecret = ironcladSettings.ClientSecret;
                _authority = ironcladSettings.Authority;
            }
        }

        public async Task AddClaim(string ironcladUserId, string type, string value)
        {
            var httpClient = _httpClientFactory.CreateClient();

            var discoveryResponse = await _discoveryCache.GetAsync();

            if (discoveryResponse.IsError)
            {
                _discoveryCache.Refresh();
                discoveryResponse = await _discoveryCache.GetAsync();
            }

            ///GET FROM CACHE AND CHECK TTL
            var tokenResponse = await httpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = discoveryResponse.TokenEndpoint,
                ClientId = _clientId,
                ClientSecret = _clientSecret,
                Scope = "sample_api auth_api auth_api:write"
            });

            HttpMessageHandler handler = new BearerTokenHandler(tokenResponse.AccessToken);

            using (var usersClient = new UsersHttpClient(_authority, handler))
            {
                var claims = new Dictionary<string, object>
                {
                    {type, value}
                };

                await usersClient.ModifyUserAsync(new User
                {
                    Id = ironcladUserId,
                    //TODO:@gafanasiev Change it to userId, when it would be ready on Ironclad.
                    Username = "gmaf_dev_4@example.com",
                    Roles = null,
                    Claims = claims
                });
            }

            //TODO:@gafanasiev Add error handling.
            //if (tokenResponse.IsError)
            //{
            //    throw 
            //}
        }
    }

    internal class BearerTokenHandler : DelegatingHandler
    {
        private readonly string _token;

        public BearerTokenHandler(string token)
        {
            _token = token;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}
