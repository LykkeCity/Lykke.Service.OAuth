using Newtonsoft.Json;

namespace Core.Recaptcha
{
    public class RecaptchaResponse
    {
        public double Score { get; set; }
        public bool Success { get; set; }
        [JsonProperty("error-codes")]
        public string[] ErrorCodes { get; set; }
    }
}
