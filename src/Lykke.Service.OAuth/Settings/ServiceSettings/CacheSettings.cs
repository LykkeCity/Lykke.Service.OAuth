using System;

namespace WebAuth.Settings.ServiceSettings
{
    public class CacheSettings
    {
        public TimeSpan VerificationCodeExpiration { get; set; }
        public TimeSpan RegistrationExpiration { get; set; }
    }
}
