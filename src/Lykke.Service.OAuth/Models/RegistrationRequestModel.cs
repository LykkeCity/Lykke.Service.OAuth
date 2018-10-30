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
        /// Id of the client app which is used by a user for registration
        /// </summary>
        [Required]
        public string ClientId { get; set; }

        /// <summary>
        /// The Id of registration. Obtained while email verification
        /// </summary>
        [Required]
        public string RegistrationId { get; set; }

        public RegistrationDto ToDto()
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
