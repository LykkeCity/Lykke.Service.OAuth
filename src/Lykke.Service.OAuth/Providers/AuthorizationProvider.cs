using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Primitives;
using AspNet.Security.OpenIdConnect.Server;
using Core.Application;
using Core.Extensions;
using Lykke.Service.Session.Client;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace WebAuth.Providers
{
    public sealed class AuthorizationProvider : OpenIdConnectServerProvider
    {
        private readonly IApplicationRepository _applicationRepository;
        private readonly IClientSessionsClient _clientSessionsClient;


        public AuthorizationProvider(IApplicationRepository applicationRepository, IClientSessionsClient clientSessionsClient)
        {
            _applicationRepository = applicationRepository;
            _clientSessionsClient = clientSessionsClient;
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

            // Note: client authentication is not mandatory for non-confidential client applications like mobile apps
            // (except when using the client credentials grant type) but this authorization server uses a safer policy
            // that makes client authentication mandatory and returns an error if client_id or client_secret is missing.
            // You may consider relaxing it to support the resource owner password credentials grant type
            // with JavaScript or desktop applications, where client credentials cannot be safely stored.
            // In this case, call context.Skip() to inform the server middleware the client is not trusted.
            if (string.IsNullOrEmpty(context.ClientId) || string.IsNullOrEmpty(context.ClientSecret))
            {
                context.Reject(
                    OpenIdConnectConstants.Errors.InvalidRequest,
                    "Missing credentials: ensure that your credentials were correctly " +
                    "flowed in the request body or in the authorization header");

                return;
            }

            if (!await ValidateClient(context))
            {
                return;
            }

            context.Validate();
        }

        private async Task<bool> ValidateClient(BaseValidatingClientContext context)
        {
            // Retrieve the application details corresponding to the requested client_id.
            var application = await _applicationRepository.GetByIdAsync(context.ClientId);

            if (application == null)
            {
                context.Reject(
                    OpenIdConnectConstants.Errors.InvalidClient,
                    "Application not found in the database: ensure that your client_id is correct");

                return false;
            }

            // Note: to mitigate brute force attacks, you SHOULD strongly consider applying
            // a key derivation function like PBKDF2 to slow down the secret validation process.
            // You SHOULD also consider using a time-constant comparer to prevent timing attacks.
            // For that, you can use the CryptoHelper library developed by @henkmollema:
            // https://github.com/henkmollema/CryptoHelper. If you don't need .NET Core support,
            // SecurityDriven.NET/inferno is a rock-solid alternative: http://securitydriven.net/inferno/
            if (!string.Equals(context.ClientSecret, application.Secret, StringComparison.Ordinal))
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
                context.Reject("No session", "Session id is not provided in claims");
            }

            var session = await _clientSessionsClient.GetAsync(sessionId.Value.ToString());
            if (session == null)
            {
                context.Reject("Unknown session", "Unable to find a session. Probably it expired");

            }
            context.Validate();
        }
    }
}
