using System.ComponentModel.DataAnnotations;

namespace WebAuth.Models
{
    public class SigninViewModel : ViewModel
    {
        public SigninViewModel()
        {
        }

        public SigninViewModel(string returnUrl) : base(returnUrl)
        {
        }

        [Required(ErrorMessage = "E-mail is required")]
        [DataType(DataType.EmailAddress)]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}