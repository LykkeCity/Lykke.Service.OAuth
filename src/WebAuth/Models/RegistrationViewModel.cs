using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace WebAuth.Models
{
    public class RegistrationViewModel : ViewModel
    {
        public RegistrationViewModel()
        {
        }

        public RegistrationViewModel(string returnUrl) : base(returnUrl)
        {
        }

        [Required(ErrorMessage = "E-mail is required")]
        [DataType(DataType.EmailAddress)]
        [Remote("VerifyEmail", "UserValidation")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [RegularExpression(@".{6,}", ErrorMessage = "The password length should not be less than 6 characters")]
        public string RegistrationPassword { get; set; }

        [Required(ErrorMessage = "Confirm password is required")]
        [DataType(DataType.Password)]
        [Compare("RegistrationPassword", ErrorMessage = "Password and confirm password should be the same")]
        public string ConfirmPassword { get; set; }
    }
}