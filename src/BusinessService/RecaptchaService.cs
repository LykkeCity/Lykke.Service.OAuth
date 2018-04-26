using System;
using System.Threading.Tasks;
using Common.Log;
using Core.Recaptcha;
using Flurl.Http;
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
            ILog log)
        {
            _httpContextAccessor = httpContextAccessor;
            _secret = secret;
            _log = log.CreateComponentScope(nameof(RecaptchaService));
        }
        
        public async Task<bool> Validate(string response = null)
        {
            var resp = response ?? (string)_httpContextAccessor.HttpContext.Request.Form["g-recaptcha-response"];

            if (resp == null)
                return false;

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
                _log.WriteWarning(nameof(Validate), string.Empty, "Error validating captcha", ex);
            }

            return false;
        }
    }
}
