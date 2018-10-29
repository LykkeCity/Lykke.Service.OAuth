using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Core.Registration
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RegistrationStep
    {
        /// <summary>
        /// Waiting for account info
        /// </summary>
        AccountInformation = 0,
        /// <summary>
        /// Waiting for pin
        /// </summary>
        Pin = 1
    }
}
