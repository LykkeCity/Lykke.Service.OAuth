using System;
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
    public class IroncladFacade : IIroncladFacade
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IDiscoveryCache _discoveryCache;

        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _authority;
        private readonly string _scope;
        private TokenResponse _accessTokenResponse;
        private DateTime _accessTokenExpiryTime;

        public IroncladFacade(
            IdentityProviderSettings ironcladSettings,
            IHttpClientFactory httpClientFactory,
            IDiscoveryCache discoveryCache)
        {
            _httpClientFactory = httpClientFactory;
            _discoveryCache = discoveryCache;

            _clientId = ironcladSettings.ClientId;
            _clientSecret = ironcladSettings.ClientSecret;
            _authority = ironcladSettings.Authority;
            _scope = string.Join(' ', ironcladSettings.Scopes);
        }

        public async Task AddUserClaim(string ironcladUserId, string type, string value)
        {
            var accessToken = await GetAccessToken();

            var handler = new BearerTokenHandler(accessToken.AccessToken);

            using (var usersClient = new UsersHttpClient(_authority, handler))
            {
                var claims = new Dictionary<string, object>
                {
                    {type, value}
                };

                await usersClient.ModifyUserAsync(new User
                    {
                        Id = ironcladUserId,
                        //FIXME: @gafanasiev Could be removed after fixing ironclad usersClient.
                        Username = ironcladUserId,
                        Roles = null,
                        Claims = claims
                    },
                    ironcladUserId);
            }
        }

        private async Task<TokenResponse> GetAccessToken()
        {
            var httpClient = _httpClientFactory.CreateClient();

            var discoveryResponse = await _discoveryCache.GetAsync();

            if (discoveryResponse.IsError)
            {
                _discoveryCache.Refresh();
                throw discoveryResponse.Exception;
            }

            //use token if it exists and is still fresh
            if (_accessTokenResponse != null && 
                _accessTokenExpiryTime > DateTime.UtcNow)
                return _accessTokenResponse;

            //else get a new token
            var tokenResponse = await httpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = discoveryResponse.TokenEndpoint,
                ClientId = _clientId,
                ClientSecret = _clientSecret,
                Scope = _scope
            });

            if (tokenResponse.IsError) 
                throw tokenResponse.Exception;

            //set Token to the new token and set the expiry time to the new expiry time
            _accessTokenResponse = tokenResponse;

            _accessTokenExpiryTime = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);

            //return fresh token
            return _accessTokenResponse;
        }
    }

    internal class BearerTokenHandler : DelegatingHandler
    {
        private readonly string _token;

        public BearerTokenHandler(string token)
        {
            _token = token;
            InnerHandler = new HttpClientHandler();
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}
