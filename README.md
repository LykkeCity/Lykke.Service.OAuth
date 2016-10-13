# WebAuth server #

WebAuth is an OpenID Connect server based on [ASOS](https://github.com/aspnet-contrib/AspNet.Security.OpenIdConnect.Server) and ASP.NET Core 1.0

### Before running ###

The application with the following data needs to be added to the Backoffice:

* Application Name (will be used on the confirmation page)
* Application Id (client id)
* Secret key (client secret)
* Redirect uri (redirection URI to which the response will be sent)

### How to connect to the authentication server ###

The WebAuth server works according to the [OpenID Connect](http://openid.net/specs/openid-connect-core-1_0.html) specification.

The following URIs are accepted:

* Authorization endpoint path is `/connect/authorize`
* Logout endpoint path is `/connect/logout`
* Token endpoint path is `/connect/token`
* Userinfo endpoint path is `/connect/userinfo`

OpenID Connect uses the following OAuth 2.0 request parameters with the Authorization Code Flow:

* __scope__
`REQUIRED` OpenID Connect requests MUST contain the openid scope value.
* __response_type__
`REQUIRED` OAuth 2.0 Response Type value that determines the authorization processing flow to be used, including what parameters are returned from the endpoints used. This value is code.
* __client_id__
`REQUIRED` OAuth 2.0 Client Identifier.
* __client_secret__
`REQUIRED` OAuth 2.0 Client Secret.
* __redirect_uri__
`REQUIRED` Redirection URI to which the response will be sent. This URI MUST exactly match the Redirection URI value for the Client pre-registered.
* __state__
`RECOMMENDED` Opaque value used to maintain state between the request and the callback. Typically, Cross-Site Request Forgery (CSRF, XSRF) mitigation is done by cryptographically binding the value of this parameter with a browser cookie.

Possible scope values:

* __profile__
	This scope value requests access to the 'given_name', 'family_name' and 'documents' Claims.
* __email__
	This scope value requests access to the 'email' Claims.
* __address__
	This scope value requests access to the 'country' Claims.


![.Last build status](https://ci.appveyor.com/api/projects/status/ntr6ycohqkxel7jn?svg=true)