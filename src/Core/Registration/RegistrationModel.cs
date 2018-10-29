using System.ComponentModel.DataAnnotations;

namespace Core.Registration
{
    public class RegistrationModel
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string ClientId { get; set; }
    }
}
