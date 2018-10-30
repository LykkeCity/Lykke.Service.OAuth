using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.OAuth.Controllers
{
    public class SpaController : Controller
    {
        [HttpGet("~/registration")]
        public ActionResult Registration()
        {
            return View("Registration");
        }
    }
}
