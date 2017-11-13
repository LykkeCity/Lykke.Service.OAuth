using System.ComponentModel.DataAnnotations;

namespace WebAuth.Models
{
    public class SignUpViewModel : ViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }
        public string Referer { get; set; }
        public string Code { get; set; }
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
        [Required(ErrorMessage = "First name is required")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Last name is required")]
        public string LastName { get; set; }
    }
}
