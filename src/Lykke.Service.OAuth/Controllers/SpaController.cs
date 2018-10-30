using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.OAuth.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class SpaController : Controller
    {
        [HttpGet("~/registration")]
        public ActionResult Registration()
        {
            return View("Registration");
        }
    }
}
