using System;
using Common.PasswordTools;
using MessagePack;

namespace Core.Registration
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class RegistrationModel : IPasswordKeeping
    {
        public string RegistrationId { get; }
        public string Hash { get; set; }
        public string Salt { get; set; }
        public string Email { get; }
        public string ClientId { get; }
        public RegistrationStep RegistrationStep { get; private set; }
        public RegistrationModel(RegistrationDto registrationDto)
        {
            Email = registrationDto.Email;
            ClientId = registrationDto.ClientId;
            this.SetPassword(registrationDto.Password);
            RegistrationId = GenerateRegistrationId();
        }

        [SerializationConstructor]
        public RegistrationModel(string registrationId, string email, string clientId, string hash, string salt, RegistrationStep registrationStep)
        {
            RegistrationId = registrationId;
            Email = email;
            ClientId = clientId;
            Hash = hash;
            Salt = salt;
            RegistrationStep = registrationStep;
        }

        private string GenerateRegistrationId()
        {
            var guid = Guid.NewGuid();
            string enc = Convert.ToBase64String(guid.ToByteArray());
            enc = enc.Replace("/", "_");
            enc = enc.Replace("+", "-");
            return enc.Substring(0, 22);
        }

        public void SetInitialInfoAsValid()
        {
            RegistrationStep = RegistrationStep.AccountInformation;
        }
    }
}
