using System.ComponentModel.DataAnnotations;
using Core.Registration;

namespace Lykke.Service.OAuth.Models
{
    /// <summary>
    /// The model for registration start
    /// </summary>
    public class RegistrationRequestModel
    {
        /// <summary>
        /// User email
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        /// <summary>
        /// User password
        /// </summary>
        [Required]
        public string Password { get; set; }

        /// <summary>
        /// Client Id of the client which is used for registration
        /// </summary>
        [Required]
        public string ClientId { get; set; }

        public RegistrationDto ToDomain()
        {
            return new RegistrationDto
            {
                Email = Email,
                Password = Password,
                ClientId = ClientId
            };
        }
    }
}
