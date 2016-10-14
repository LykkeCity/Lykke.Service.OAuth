using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace WebAuth.Controllers
{
    public class HomeController : Controller
    {
        [Route("/")]
        public IActionResult Index()
        {
            ViewBag.Version = typeof(HomeController).GetTypeInfo().Assembly.GetName().Version.ToString();
            return View();
        }

        [Route("/home/error/{errorCode}")]
        public IActionResult Error(string errorCode)
        {
            if (string.Equals(errorCode, "404"))
            {
                return View("NotFound");
            }
            return View("Error", new OpenIdConnectMessage());
        }

        [Route("/home/error")]
        public IActionResult Error()
        {
            
            return View("Error", new OpenIdConnectMessage());
        }

        [Route("/version"), HttpGet]
        public IActionResult Version()
        {
            return Json(typeof(HomeController).GetTypeInfo().Assembly.GetName().Version.ToString());
        }


    }
}