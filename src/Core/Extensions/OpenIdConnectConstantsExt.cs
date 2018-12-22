namespace Core.Extensions
{
    //todo: rename
    public class OpenIdConnectConstantsExt
    {
        public static class Claims
        {
            public const string Country = "country";
            public const string SignType = "SignType";
            public const string PartnerId = "http://lykke.com/oauth/partner_id";
            public const string SessionId = "http://lykke.com/oauth/sessionid";
            public const string Lsub = "lsub";
        }

        public static class Parameters
        {
            public const string PartnerId = "partnerId";
            public const string Tenant = "tenant";
            public const string Idp = "idp";
        }

        public static class AuthenticationProperties
        {
            public const string ExternalLoginRedirectUrl = "externalLoginRedirectUrl";
            public const string AcrValues = "acrValues";
        }

        public static class Auth
        {
            public const string DefaultScheme = "ServerCookie";
            public const string ExternalAuthenticationScheme = "ExternalAuthenticationScheme";
            public const string IroncladAuthenticationScheme = "IroncladAuthenticationScheme";
            public const string LykkeScheme = "LykkeScheme";
        }

        public static class Errors
        {
            public const string UnknownSession = "lykke_unknown_session";
            public const string ClaimNotFound = "lykke_claim_not_found";
            public const string ClientBanned = "lykke_client_banned";
        }

        public static class Providers
        {
            public const string Ironclad = "ironclad";
            public const string Lykke = "lykke";
        }

        public static class Protectors
        {
            public const string ExternalProviderCookieProtector = "ExternalProviderCookieProtector";
        }

        public static class Cookies
        {
            public const string TemporaryUserIdCookie = "TemporaryUserIdCookie";
        }
    }
}
