using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebAuth.ActionHandlers;

namespace WebAuth.ViewComponents
{
    public class StatusBarViewComponent : ViewComponent
    {
        private readonly ProfileActionHandler _profileActionHandler;

        public StatusBarViewComponent(ProfileActionHandler profileActionHandler)
        {
            _profileActionHandler = profileActionHandler;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = await _profileActionHandler.GetStatusBarModelAsync();
            return View(model);
        }
    }
}