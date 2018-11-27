using System.Collections.Generic;
using System.Linq;
using Lykke.Service.OAuth.Settings.ServiceSettings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Lykke.Service.OAuth.Attributes
{
    public class FeatureToggleFilter : ActionFilterAttribute
    {
        private readonly List<string> _features;
        private readonly FeatureToggleSettings _featureToggleSettings;

        public FeatureToggleFilter(FeatureToggleSettings settings, string features)
        {
            _featureToggleSettings = settings;
            _features = features.Split(',').ToList();
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            foreach (var feature in _features)
            {
                var featureToogle = _featureToggleSettings.DisabledFeatures.FirstOrDefault(x => x == feature);

                if (featureToogle != null)
                {
                    context.Result = new BadRequestObjectResult("Feature is disabled");
                }
            }
        }
    }
}
