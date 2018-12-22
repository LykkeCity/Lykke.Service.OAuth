using System.Security.Claims;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Extensions;
using Core.Extensions;
using Lykke.Service.GoogleAnalyticsWrapper.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace WebAuth.ViewComponents
{
    public class GaUserIdViewComponent : ViewComponent
    {
        private readonly IGoogleAnalyticsWrapperClient _gaWaraWrapperClient;

        public GaUserIdViewComponent(IGoogleAnalyticsWrapperClient gaWaraWrapperClient)
        {
            _gaWaraWrapperClient = gaWaraWrapperClient;
        }
        
        public async Task<IViewComponentResult> InvokeAsync()
        {
            ViewBag.UserId = string.Empty;

            var authenticateResult = await HttpContext.AuthenticateAsync(OpenIdConnectConstantsExt.Auth.DefaultScheme);

            if (authenticateResult.Succeeded)
            {
                var userId = authenticateResult.Principal.GetClaim(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrWhiteSpace(userId))
                    ViewBag.UserId = await _gaWaraWrapperClient.GetGaUserIdAsync(userId);
            }

            return View();
        }
    }
}
