using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebAuth.Models;

namespace WebAuth.ViewComponents
{
    public class LykkeLogoViewComponent : ViewComponent
    {
        public string DefaultUrl { get; set; }

        public LykkeLogoViewComponent()
        {
            DefaultUrl = "https://www.lykke.com/";
        }

        public Task<IViewComponentResult> InvokeAsync(string url)
        {
            var href = String.IsNullOrEmpty(url) ? DefaultUrl : url;
            var model = new LykkeLogoViewModel
            {
                Url = href,
            };

            return Task.FromResult<IViewComponentResult>(View(model));
        }
    }
}
