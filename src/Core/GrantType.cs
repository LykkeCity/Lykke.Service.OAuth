using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Core
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum GrantType
    {
        AuthorizationCode = 0,
        Implicit = 1,
        RefreshToken = 2
    }
}
