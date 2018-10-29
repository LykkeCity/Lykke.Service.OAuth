using System.ComponentModel.DataAnnotations;
using Core.Registration;

namespace WebAuth.Models
{
    /// <summary>
    /// Registration status response
    /// </summary>
    public class RegistrationStatusResponse
    {
        /// <summary>
        /// The current step of registration
        /// </summary>
        [EnumDataType(typeof(RegistrationStep))]
        public RegistrationStep RegistrationStep { get; set; }
    }
}
