using System.Linq;
using AspNet.Security.OpenIdConnect.Primitives;

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
    }
}
