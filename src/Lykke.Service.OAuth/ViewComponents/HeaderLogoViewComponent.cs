using System.Threading.Tasks;
using Lykke.Common.Extensions;
using Microsoft.AspNetCore.Mvc;
using WebAuth.Models;

namespace WebAuth.ViewComponents
{
    public class HeaderLogoViewComponent : ViewComponent
    {
        public Task<IViewComponentResult> InvokeAsync()
        {
            var referer = HttpContext.GetReferer();

            var model = new HeaderLogoViewModel
            {
                Url = referer
            };

            return Task.FromResult<IViewComponentResult>(View(model));
        }
    }
}
