using System.Linq;
using System.Text.RegularExpressions;
using AspNet.Security.OpenIdConnect.Primitives;
using Common;

namespace Lykke.Service.OAuth.Extensions
{
    public static class OpenIdConnectRequestExtensions
    {
        public static string GetAcrValue(this OpenIdConnectRequest request, string key)
        {
            var acrValues = request.GetAcrValues();
            var value = acrValues.FirstOrDefault(s => s.StartsWith($"{key}:"));
            return value?.Substring(key.Length + 1);
        }

        public static string Serialize(this OpenIdConnectRequest request)
        {
            var serialized = request.ToJson();

            var woState = SanitizeValueFromJson(serialized, "state");
            var woNonce = SanitizeValueFromJson(woState, "nonce");

            return woNonce;
        }

        private static string SanitizeValueFromJson(string source, string key)
        {
            return Regex.Replace(source, $@"""{key}"":""[_.a-zA-Z0-9-]+""", $"{key}:*sanitized*");
        }
    }
}
