using System;
using System.Threading.Tasks;
using Common.Log;
using Core.Recaptcha;
using Flurl.Http;
using Lykke.Common.Log;
using Microsoft.AspNetCore.Http;

namespace BusinessService
{
    public class RecaptchaService : IRecaptchaService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _secret;
        private readonly ILog _log;

        public RecaptchaService(
            IHttpContextAccessor httpContextAccessor,
            string secret,
            ILogFactory logFactory)
        {
            _httpContextAccessor = httpContextAccessor;
            _secret = secret;
            _log = logFactory.CreateLog(this);
        }

        public async Task<bool> Validate(string response = null)
        {
            var resp = response ?? _httpContextAccessor.HttpContext.Request.Form["g-recaptcha-response"];

            try
            {
                var result = await "https://www.google.com/recaptcha/api/siteverify"
                    .PostUrlEncodedAsync(new
                    {
                        secret = _secret,
                        response = resp
                    }).ReceiveJson<RecaptchaResponse>();

                return result.Success;
            }
            catch (Exception ex)
            {
                _log.Warning(nameof(Validate), ex, "Error validating captcha");
            }

            return false;
        }
    }
}
