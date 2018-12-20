using System.Security.Claims;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Extensions;
using Lykke.Service.GoogleAnalyticsWrapper.Client;
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

            if (User.Identity.IsAuthenticated)
            {
                var userId = UserClaimsPrincipal.GetClaim(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrWhiteSpace(userId))
                    ViewBag.UserId = await _gaWaraWrapperClient.GetGaUserIdAsync(userId);
            }

            return View();
        }
    }
}
