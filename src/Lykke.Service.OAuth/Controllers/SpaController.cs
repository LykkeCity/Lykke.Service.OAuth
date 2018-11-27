using Lykke.Service.OAuth.Attributes;
using Lykke.Service.OAuth.Settings;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.OAuth.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class SpaController : Controller
    {
        [TypeFilter(typeof(FeatureToggleFilter), Arguments = new object[] { Features.Registration })]
        [HttpGet("~/registration")]
        public ActionResult Registration()
        {
            return View("Registration");
        }
    }
}
