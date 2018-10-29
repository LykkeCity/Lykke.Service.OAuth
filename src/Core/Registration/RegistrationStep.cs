using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Core.Registration
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RegistrationStep
    {
        InitialInfo = 0,
        AccountInformation,
        Pin
    }
}
