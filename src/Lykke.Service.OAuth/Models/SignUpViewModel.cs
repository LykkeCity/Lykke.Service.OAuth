using System.ComponentModel.DataAnnotations;

namespace WebAuth.Models
{
    public class SignUpViewModel : ViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }
        public string Referer { get; set; }
        public string Key { get; set; }
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required(ErrorMessage = "First name is required")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Last name is required")]
        public string LastName { get; set; }
        public string Cid { get; set; }
        public string Traffic { get; set; }
        public string Hint { get; set; }
        public string Phone { get; set; }
        public string CountryOfResidence { get; set; }
    }
}
