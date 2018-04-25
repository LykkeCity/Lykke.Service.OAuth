using Newtonsoft.Json;

namespace Core.Recaptcha
{
    public class RecaptchaResponse
    {
        public bool Success { get; set; }
        [JsonProperty("error-codes")]
        public string[] ErrorCodes { get; set; }
    }
}
