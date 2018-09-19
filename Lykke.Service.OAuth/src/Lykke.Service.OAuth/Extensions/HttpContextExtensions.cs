using Lykke.Common.Extensions;
using Microsoft.AspNetCore.Http;

namespace WebAuth.Extensions
{
    public static class HttpContextExtensions
    {
        public static string GetApplicationId(this HttpContext ctx)
        {
            return ctx.GetHeaderValueAs<string>("application_id");
        }
    }
}
