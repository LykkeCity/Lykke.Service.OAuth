using System.Collections.Generic;
using Lykke.Service.Registration.Models;

namespace WebAuth.Models
{
    public class RegistrationResultModel
    {
        public RegistrationResponse RegistrationResponse { get; set; }
        public bool IsPasswordComplex { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}
