using MessagePack;

namespace Core.ExternalProvider
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class LykkeSignInContext
    {
        public string Platform { get; set; }

        public string Partnerid { get; set; }

        public string ReturnUrl { get; set; }

        public string AfterLykkeLoginReturnUrl { get; set; }
    }
}
