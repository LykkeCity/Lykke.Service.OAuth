using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Lykke.Service.OAuth.Middleware
{
    public class RedirectResponseOverride
    {
        private readonly RequestDelegate _next;

        public RedirectResponseOverride(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            await _next(context);

            if (context.Request.Path == "/api/registration/accountInfo" &&
                context.Response.StatusCode == 302)
            {
                var location = context.Response.Headers["location"];
                context.Response.StatusCode = 200;
                var json = $@"{{""location"" : ""{location}""}}";
                await context.Response.WriteAsync(json);
            }
        }
    }
}
