using System.Collections.Generic;
using Lykke.Service.Registration.Contract.Client.Models;

namespace WebAuth.Models
{
    public class RegistrationResultModel
    {
        public AccountsRegistrationResponseModel RegistrationResponse { get; set; }
        public bool IsPasswordComplex { get; set; }
        public bool IsAffiliateCodeCorrect { get; set; }
        public bool IsValid => IsPasswordComplex && IsAffiliateCodeCorrect;
        public List<string> Errors { get; set; } = new List<string>();
    }
}
