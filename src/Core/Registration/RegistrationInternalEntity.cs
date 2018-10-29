using Common.PasswordTools;
using MessagePack;

namespace Core.Registration
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class RegistrationInternalEntity : IPasswordKeeping
    {
        public string Hash { get; set; }
        public string Salt { get; set; }
        public string Email { get; }
        public string ClientId { get; }
        public RegistrationStep RegistrationStep { get; }
        public RegistrationInternalEntity(RegistrationModel registrationModel)
        {
            Email = registrationModel.Email;
            ClientId = registrationModel.ClientId;
            this.SetPassword(registrationModel.Password);
            RegistrationStep = RegistrationStep.InitialInfo;
        }

        [SerializationConstructor]
        public RegistrationInternalEntity(string email, string clientId, string hash, string salt, RegistrationStep registrationStep)
        {
            Email = email;
            ClientId = clientId;
            Hash = hash;
            Salt = salt;
            RegistrationStep = registrationStep;
        }
    }
}
