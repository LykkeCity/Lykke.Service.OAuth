using System.Security.Claims;
using AspNet.Security.OAuth.Validation;
using AspNet.Security.OpenIdConnect.Server;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAuth.Controllers
{
    [Route("api")]
    public class ResourceController : Controller
    {
        [Authorize(AuthenticationSchemes = OAuthValidationConstants.Schemes.Bearer), HttpGet, Route("message")]
        public IActionResult GetMessage()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return BadRequest();
            }

            return Json($"{identity.Name} has been successfully authenticated.");
        }
    }
}
