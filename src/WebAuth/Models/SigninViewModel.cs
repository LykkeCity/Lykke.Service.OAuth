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

        [Required]
        [DataType(DataType.EmailAddress)]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}