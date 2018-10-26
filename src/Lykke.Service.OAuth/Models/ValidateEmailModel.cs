using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.OAuth.Models
{
    /// <summary>
    /// Email validation request model
    /// </summary>
    public class ValidateEmailModel
    {
        /// <summary>
        /// Email address
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        /// <summary>
        /// Email address hash
        /// </summary>
        [Required]
        public string Hash { get; set; }
    }
}
