﻿namespace Core.Extensions
{
    public class OpenIdConnectConstantsExt
    {
        public static class Claims
        {
            public const string Country = "country";
            public const string SignType = "SignType";
            public const string PartnerId = "http://lykke.com/oauth/partner_id";
            public const string SessionId = "http://lykke.com/oauth/sessionid";
            public const string PhoneNumberVerified = "http://lykke.com/oauth/phone_number_verified";
        }

        public static class Auth
        {
            public const string DefaultScheme = "ServerCookie";
            public const string VmoolaAuthenticationScheme = "vMoolaAuthScheme";
            public const string ExternalAuthenticationScheme = "ExternalAuthenticationScheme";
        }

        public static class Errors
        {
            public const string UnknownSession = "lykke_unknown_session";
            public const string ClaimNotFound = "lykke_claim_not_found";
            public const string ClientBanned = "lykke_client_banned";
        }

        public static class Providers
        {
            public const string Lykke = "Lykke";
            // ReSharper disable once InconsistentNaming
            public const string vMoola = "vMoola";
        }
    }
}
