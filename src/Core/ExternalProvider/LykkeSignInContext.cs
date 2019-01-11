using MessagePack;

namespace Core.ExternalProvider
{
    /// <summary>
    ///     Sign in page context.
    /// </summary>
    [MessagePackObject(true)]
    public class LykkeSignInContext
    {       
        public string Platform { get; set; }

        public string Partnerid { get; set; }

        public string RelativeUrl { get; set; }
    }
}
