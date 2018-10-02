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

        public static class Auth
        {
            public const string DefaultScheme = "ServerCookie";
        }

        public static class Errors
        {
            public const string UnknownSession = "Unknown session";
            public const string NoSubjectClaim = "No subject";
            public const string ClientBanned = "Client banned";
        }
    }
}
