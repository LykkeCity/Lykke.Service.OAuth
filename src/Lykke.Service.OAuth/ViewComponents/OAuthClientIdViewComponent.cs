using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebAuth.Settings.ServiceSettings;

namespace WebAuth.ViewComponents
{
    public class OAuthClientIdViewComponent : ViewComponent
    {
        private readonly SecuritySettings _securitySettings;

        public OAuthClientIdViewComponent(SecuritySettings securitySettings)
        {
            _securitySettings = securitySettings;
        }
        
        public async Task<IViewComponentResult> InvokeAsync()
        {
            ViewBag.OAuthClientId = _securitySettings.OAuthClientId;

            return View();
        }
    }
}
