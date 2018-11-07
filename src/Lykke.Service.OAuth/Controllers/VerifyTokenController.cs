using System.Threading.Tasks;
using Core.Recaptcha;
using Flurl.Http;
using Microsoft.AspNetCore.Mvc;
using WebAuth.Controllers;
using WebAuth.Settings.ServiceSettings;

namespace Lykke.Service.OAuth.Controllers
{
    public class VerifyTokenController : BaseController
    {
        private readonly SecuritySettings _securitySettings;
        
        public VerifyTokenController(
            SecuritySettings securitySettings)
        {
            _securitySettings = securitySettings;
        }
        
        [HttpGet("~/verifyToken")]
        public async Task<IActionResult> VerifyToken([FromQuery] string token)
        {
            var result = await "https://www.google.com/recaptcha/api/siteverify"
                .PostUrlEncodedAsync(new
                {
                    secret = _securitySettings.RecaptchaSecrect,
                    response = token
                }).ReceiveJson<RecaptchaResponse>();

            return Ok(result);
        }
    }
}
