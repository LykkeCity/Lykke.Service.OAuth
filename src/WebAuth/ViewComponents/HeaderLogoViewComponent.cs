using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Common.Extensions;
using Microsoft.AspNetCore.Mvc;
using WebAuth.Models;

namespace WebAuth.ViewComponents
{
    public class HeaderLogoViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var referer = HttpContext.GetReferer();
            var streamsUrls = new List<string> {"streams", "localhost:53395" };

            var model = new HeaderLogoViewModel
            {
                Url = referer,
                IsStreams = streamsUrls.Any(item => referer?.ToLower().Contains(item) ?? false)
            };

            return View(model);
        }
    }
}
