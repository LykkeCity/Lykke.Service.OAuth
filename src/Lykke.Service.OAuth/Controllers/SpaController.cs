using Lykke.Service.OAuth.Models;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.OAuth.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class SpaController : Controller
    {
        [HttpGet("~/registration")]
        public ActionResult Registration([FromQuery]string registrationId)
        {
            return View("Registration", new RegistrationResponseModel
            {
                RegistrationId = registrationId
            });
        }
    }
}
