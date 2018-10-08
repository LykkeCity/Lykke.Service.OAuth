using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Primitives;
using AspNet.Security.OpenIdConnect.Server;
using Core;
using Common.Log;
using Core.Application;
using Core.Extensions;
using Lykke.Service.ClientAccount.Client;
using Core.Services;
using Lykke.Common.Log;
using Lykke.Service.Session.Client;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace WebAuth.Providers
{
    public sealed class AuthorizationProvider : OpenIdConnectServerProvider
    {
        private readonly IApplicationRepository _applicationRepository;
        private readonly IClientSessionsClient _clientSessionsClient;
        private readonly IClientAccountClient _accountClient;
        private readonly ITokenService _tokenService;
        private readonly IValidationService _validationService;
        private readonly ILog _log;


        public AuthorizationProvider(
            IApplicationRepository applicationRepository,
            IClientSessionsClient clientSessionsClient,
            IClientAccountClient accountClient,
            ITokenService tokenService,
            IValidationService validationService,
            ILogFactory logFactory)
        {
            _applicationRepository = applicationRepository;
            _clientSessionsClient = clientSessionsClient;
            _tokenService = tokenService;
            _validationService = validationService;
            _accountClient = accountClient;
            _log = logFactory.CreateLog(this);
        }

        public override Task MatchEndpoint(MatchEndpointContext context)
        {
            // Note: by default, OpenIdConnectServerHandler only handles authorization requests made to the authorization endpoint.
            // This context handler uses a more relaxed policy that allows extracting authorization requests received at
            // /connect/authorize/accept and /connect/authorize/deny (see AuthorizationController.cs for more information).
            if (context.Options.AuthorizationEndpointPath.HasValue &&
                context.Request.Path.StartsWithSegments(context.Options.AuthorizationEndpointPath))
            {
                context.MatchAuthorizationEndpoint();
            }

            return Task.CompletedTask;
        }

        public override Task ExtractAuthorizationRequest(ExtractAuthorizationRequestContext context)
        {
            // If a request_id parameter can be found in the authorization request,
            // restore the complete authorization request stored in the user session.
            if (!string.IsNullOrEmpty(context.Request.RequestId))
            {
                var payload = context.HttpContext.Session.Get("authorization-request:" + context.Request.RequestId);
                if (payload == null)
                {
                    context.Reject(
                        OpenIdConnectConstants.Errors.InvalidRequest,
                        "Invalid request: timeout expired.");

                    return Task.FromResult(0);
                }

                var request = JsonConvert.DeserializeObject<OpenIdConnectRequest>(Encoding.UTF8.GetString(payload));

                foreach (var prop in request.GetParameters())
                {
                    context.Request.SetParameter(prop.Key, prop.Value);
                }
            }

            return Task.CompletedTask;
        }

        public override async Task ValidateAuthorizationRequest(ValidateAuthorizationRequestContext context)
        {
            // Note: the OpenID Connect server middleware supports the authorization code, implicit and hybrid flows
            // but this authorization provider only accepts response_type=code authorization/authentication requests.
            // You may consider relaxing it to support the implicit or hybrid flows. In this case, consider adding
            // checks rejecting implicit/hybrid authorization requests when the client is a confidential application.
            if (!context.Request.IsAuthorizationCodeFlow() && !context.Request.IsImplicitFlow())
            {
                context.Reject(
                    OpenIdConnectConstants.Errors.UnsupportedResponseType,
                    "Only the authorization and implicit code flows are supported by this authorization server");

                return;
            }

            // Note: to support custom response modes, the OpenID Connect server middleware doesn't
            // reject unknown modes before the ApplyAuthorizationResponse event is invoked.
            // To ensure invalid modes are rejected early enough, a check is made here.
            if (!string.IsNullOrEmpty(context.Request.ResponseMode) && !context.Request.IsFormPostResponseMode() &&
                !context.Request.IsFragmentResponseMode() &&
                !context.Request.IsQueryResponseMode())
            {
                context.Reject(
                    OpenIdConnectConstants.Errors.InvalidRequest,
                    "The specified response_mode is unsupported.");

                return;
            }

            // Retrieve the application details corresponding to the requested client_id.
            var application = await _applicationRepository.GetByIdAsync(context.ClientId);

            if (application == null)
            {
                context.Reject(
                    OpenIdConnectConstants.Errors.InvalidClient,
                    "Application not found in the database: ensure that your client_id is correct");

                return;
            }


            if (application.OAuthClientProperties != null && application.OAuthClientProperties.AllowedAuthorizationFlows?.Any() == true)
            {
                if (context.Request.IsAuthorizationCodeFlow() && !application.OAuthClientProperties.AllowedAuthorizationFlows.Contains(AuthorizationFlow.AuthorizationCode))
                {
                    context.Reject(OpenIdConnectConstants.Errors.InvalidRequest, "AuthorizationCode flow not supported for the client");
                    return;
                }

                if (context.Request.IsImplicitFlow() && !application.OAuthClientProperties.AllowedAuthorizationFlows.Contains(AuthorizationFlow.Implicit))
                {
                    context.Reject(OpenIdConnectConstants.Errors.InvalidRequest, "Implicit flow not supported for the client");
                    return;
                }
            }

            var redirectUrl = application.Urls.FirstOrDefault(item => item.Equals(context.RedirectUri, StringComparison.Ordinal));

            if (!string.IsNullOrEmpty(context.RedirectUri) && redirectUrl == null)
            {
                context.Reject(
                    OpenIdConnectConstants.Errors.InvalidClient,
                    "Invalid redirect_uri");

                return;
            }

            context.Validate(redirectUrl);
        }

        public override Task ValidateLogoutRequest(ValidateLogoutRequestContext context)
        {
            // Don't validate logout url, as we don't have it
            context.Validate();
            return Task.CompletedTask;
        }

        public override async Task ValidateTokenRequest(ValidateTokenRequestContext context)
        {
            // Note: the OpenID Connect server middleware supports authorization code, refresh token, client credentials
            // and resource owner password credentials grant types but this authorization provider uses a safer policy
            // rejecting the last two ones. You may consider relaxing it to support the ROPC or client credentials grant types.
            if (!context.Request.IsAuthorizationCodeGrantType() && !context.Request.IsRefreshTokenGrantType())
            {
                context.Reject(
                    OpenIdConnectConstants.Errors.UnsupportedGrantType,
                    "Only authorization code and refresh token grant types " +
                    "are accepted by this authorization server");

                return;
            }


            if (!await ValidateClient(context))
            {
                return;
            }

            var application = await _applicationRepository.GetByIdAsync(context.ClientId);
            if (application.OAuthClientProperties != null && application.OAuthClientProperties.AllowedAuthorizationFlows?.Any() == true)
            {
                if (context.Request.IsAuthorizationCodeGrantType() && !application.OAuthClientProperties.AllowedAuthorizationFlows.Contains(AuthorizationFlow.AuthorizationCode))
                {
                    context.Reject(OpenIdConnectConstants.Errors.UnsupportedGrantType, "AuthorizationCode grant type not supported for the client");
                    return;
                }
                if (context.Request.IsRefreshTokenGrantType() && !application.OAuthClientProperties.AllowOfflineAccess)
                {
                    context.Reject(OpenIdConnectConstants.Errors.UnsupportedGrantType, "Refresh token grant type not supported for client the client");
                    return;
                }
            }

            context.Validate();
        }

        private async Task<bool> ValidateClient(BaseValidatingClientContext context)
        {
            if (string.IsNullOrEmpty(context.ClientId))
            {
                context.Reject(
                    OpenIdConnectConstants.Errors.InvalidRequest,
                    "Missing credentials: ensure that your credentials were correctly " +
                    "flowed in the request body or in the authorization header");

                return false;
            }

            // Retrieve the application details corresponding to the requested client_id.
            var application = await _applicationRepository.GetByIdAsync(context.ClientId);

            if (application == null)
            {
                context.Reject(
                    OpenIdConnectConstants.Errors.InvalidClient,
                    "Application not found in the database: ensure that your client_id is correct");

                return false;
            }

            // Note: client authentication is not mandatory for non-confidential client applications like mobile apps
            // (except when using the client credentials grant type) but this authorization server uses a safer policy
            // that makes client authentication mandatory and returns an error if client_id or client_secret is missing.
            // You may consider relaxing it to support the resource owner password credentials grant type
            // with JavaScript or desktop applications, where client credentials cannot be safely stored.
            // In this case, call context.Skip() to inform the server middleware the client is not trusted.
            if (string.IsNullOrEmpty(context.ClientSecret) &&
                (application.OAuthClientProperties == null || application.OAuthClientProperties.RequireClientSecret))
            {
                context.Reject(
                    OpenIdConnectConstants.Errors.InvalidRequest,
                    "Missing credentials: ensure that your credentials were correctly " +
                    "flowed in the request body or in the authorization header");

                return false;
            }

            // Call context.Skip() to inform the server middleware the client is not trusted.
            if (application.OAuthClientProperties != null &&
                application.OAuthClientProperties.RequireClientSecret == false)
            {
                context.Skip();
                return false;
            }

            // Note: to mitigate brute force attacks, you SHOULD strongly consider applying
            // a key derivation function like PBKDF2 to slow down the secret validation process.
            // Added fixed time comparison to prevent timing attacks.
            var clientSecretBytes = Encoding.UTF8.GetBytes(context.ClientSecret);
            var applicationSecretBytes = Encoding.UTF8.GetBytes(application.Secret);
            if (!CryptographicOperations.FixedTimeEquals(clientSecretBytes, applicationSecretBytes))
            {
                context.Reject(
                    OpenIdConnectConstants.Errors.InvalidClient,
                    "Invalid credentials: ensure that you specified a correct client_secret");

                return false;
            }

            return true;
        }

        public override async Task ValidateIntrospectionRequest(ValidateIntrospectionRequestContext context)
        {
            if (!await ValidateClient(context))
            {
                return;
            }
            context.Validate();
        }

        public override async Task HandleIntrospectionRequest(HandleIntrospectionRequestContext context)
        {
            if (!context.Claims.TryGetValue(OpenIdConnectConstantsExt.Claims.SessionId, out var sessionId))
            {
                context.Reject(OpenIdConnectConstantsExt.Errors.UnknownSession, "Session id is not provided in claims");
            }

            var session = await _clientSessionsClient.GetAsync(sessionId.Value.ToString());
            if (session == null)
            {
                context.Reject(OpenIdConnectConstantsExt.Errors.UnknownSession, "Unable to find a session. Probably it expired");
                return;
            }

            if (string.IsNullOrEmpty(context.Subject))
            {
                context.Reject(OpenIdConnectConstantsExt.Errors.ClaimNotFound, "Subject claim not found");
                return;
            }

            if (await _accountClient.IsClientBannedAsync(context.Subject))
            {
                context.Reject(OpenIdConnectConstantsExt.Errors.ClientBanned, $"Client {context.Subject} banned");
                return;
            }

            context.Validate();
        }

        public override async Task HandleTokenRequest(HandleTokenRequestContext context)
        {
            await ValidateRefreshTokenGrantTypeAsync(context);
        }

        public override async Task ApplyTokenResponse(ApplyTokenResponseContext context)
        {
            await UpdateRefreshToken(context);
        }

        private async Task ValidateRefreshTokenGrantTypeAsync(BaseValidatingTicketContext context)
        {
            // Only proccess refresh token grant type.
            if (!string.IsNullOrWhiteSpace(context.Error) ||
                !context.Request.IsRefreshTokenGrantType())
                return;

            var sessionIdClaim = context.Ticket.Principal.Claims.FirstOrDefault(claim =>
                string.Equals(claim.Type, OpenIdConnectConstantsExt.Claims.SessionId, StringComparison.Ordinal));

            if (sessionIdClaim == null)
            {
                context.Reject
                    (OpenIdConnectConstantsExt.Errors.ClaimNotFound,
                    "Session id is not provided in claims.");
                return;
            }

            var oldRefreshToken = context.Request.RefreshToken;

            if (string.IsNullOrWhiteSpace(oldRefreshToken))
            {
                context.Reject(
                    OpenIdConnectConstants.Errors.InvalidRequest,
                    "refresh_token not present in request.");
                return;
            }

            var sessionId = sessionIdClaim.Value;

            var isRefreshTokenValid = await _validationService.IsRefreshTokenValidAsync(oldRefreshToken, sessionId);

            if (isRefreshTokenValid)
                return;

            _log.Info("refresh_token was revoked.");

            context.Reject(
                OpenIdConnectConstants.Errors.InvalidGrant,
                "refresh_token was revoked.");
        }

        private async Task UpdateRefreshToken(ApplyTokenResponseContext context)
        {
            // Only proccess flows that support refresh tokens
            if (!string.IsNullOrWhiteSpace(context.Error) ||
                !(context.Request.IsRefreshTokenGrantType() ||
                  context.Request.IsAuthorizationCodeGrantType()))
                return;

            await _tokenService.UpdateRefreshTokenInWhitelistAsync(
                    context.Request.RefreshToken,
                    context.Response.RefreshToken);
        }
    }
}
