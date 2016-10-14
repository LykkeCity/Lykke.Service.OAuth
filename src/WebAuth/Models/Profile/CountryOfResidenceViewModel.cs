using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebAuth.Models.Profile
{
    public class CountryOfResidenceViewModel : StepViewModel
    {
        public string Country { get; set; }

        public List<SelectListItem> Countries { get; set; }

        public CountryOfResidenceViewModel()
        {
            StepNumber = 2;
            Title = "Country of Residence";
            Description = @"Specify the place of residence or nationality, so that we can confirm your identity.";
        }
    }
}