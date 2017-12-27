using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace WebAuth.Models.Profile
{
    public class PersonalInformationViewModel : StepViewModel
    {
        [DataType(DataType.EmailAddress)]
        [Remote("VerifyEmail", "UserValidation")]
        public string Email { get; set; }

        [Required(ErrorMessage = "First name is required")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        public string LastName { get; set; }

        public string ContactPhone { get; set; }

        public DateTime DateOfBirth { get; set; }

        public PersonalInformationViewModel()
        {
            StepNumber = 1;
            //TODO: get text from resource file
            Title = "Personal Information";
            Description = @"Fill out personal information to gain access to all services. Also enter contact information so that
                    we can contact you if necessary";
        }
    }
}