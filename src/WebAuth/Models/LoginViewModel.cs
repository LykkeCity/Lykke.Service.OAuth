using System.ComponentModel.DataAnnotations;

namespace WebAuth.Models
{
    public class LoginViewModel : ViewModel
    {
        public LoginViewModel() {}

        public LoginViewModel(string returnUrl, string referer)
        {
            ReturnUrl = returnUrl;
            Referer = referer;
        }

        [Required(ErrorMessage = "E-mail is required and can't be empty")]
        [DataType(DataType.EmailAddress)]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required and can't be empty")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        public string Referer { get; set; }
        public bool IsLogin { get; set; }
    }
}
