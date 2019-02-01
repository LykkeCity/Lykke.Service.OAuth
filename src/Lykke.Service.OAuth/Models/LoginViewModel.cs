using System.ComponentModel.DataAnnotations;

namespace WebAuth.Models
{
    public class LoginViewModel : ViewModel
    {
        [Required(ErrorMessage = "E-mail is required and can't be empty")]
        [DataType(DataType.EmailAddress)]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required and can't be empty")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        public string Referer { get; set; }
        public string LoginRecaptchaKey { get; set; }
        public string RegisterRecaptchaKey { get; set; }
        public bool? IsLogin { get; set; }
        public string Cid { get; set; }
        public string PartnerId { get; set; }
        public string Phone { get; set; }
        public string IroncladLogoutUrl { get; set; }
    }
}
