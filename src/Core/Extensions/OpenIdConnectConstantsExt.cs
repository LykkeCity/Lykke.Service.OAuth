namespace Core.Extensions
{
    public class OpenIdConnectConstantsExt
    {
        public static class Claims
        {
            public const string Country = "country";
            public const string SignType = "SignType";
            public const string PartnerId = "http://lykke.com/oauth/partner_id";
            public const string SessionId = "http://lykke.com/oauth/sessionid";
        }

        public static class Parameters
        {
            public const string PartnerIdParameter = "partnerId";
            public const string AfterExternalLoginCallback = "after_external_login_callback";
            public const string Tenant = "tenant";
        }

        public static class Auth
        {
            public const string DefaultScheme = "ServerCookie";
            public const string ExternalAuthenticationScheme = "ExternalAuthenticationScheme";
            public const string IroncladAuthenticationScheme = "IroncladAuthenticationScheme";

        }

        public static class Errors
        {
            public const string UnknownSession = "lykke_unknown_session";
            public const string ClaimNotFound = "lykke_claim_not_found";
            public const string ClientBanned = "lykke_client_banned";
        }

        public static class Tenants
        {
            public const string Ironclad = "ironclad";
        }

        public static class Providers
        {
            public const string Ironclad = "ironclad";
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
