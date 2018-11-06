using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.OAuth.Models.Registration
{
    /// <summary>
    /// Account information registration step details
    /// </summary>
    public class AccountInfoRequestModel
    {
        /// <summary>
        /// Gets or sets first name
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets last name
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets country code in iso2 format
        /// </summary>
        [Required]
        [MaxLength(2)]
        public string CountryCodeIso2{ get; set; }

        /// <summary>
        /// Gets or sets phone number
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// The Id of registration. Obtained while email verification
        /// </summary>
        [Required]
        public string RegistrationId { get; set; }
    }
}
