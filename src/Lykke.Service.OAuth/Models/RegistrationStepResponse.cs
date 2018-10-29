using System.ComponentModel.DataAnnotations;
using Core.Registration;

namespace WebAuth.Models
{
    public class RegistrationStepResponse
    {
        [EnumDataType(typeof(RegistrationStep))]
        public RegistrationStep RegistrationStep { get; set; }
    }
}
