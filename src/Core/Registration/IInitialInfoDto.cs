using System;

namespace Core.Registration
{
    public interface IRegistrationModelDto
    {
        string RegistrationId { get; set; }
        string Email { get; set; }
        string PasswordHash { get; set; }
        string PasswordSalt { get; set; }
        string ClientId { get; set; }
        RegistrationStep CurrentStep { get; set; }
        DateTime Started { get; set; }
    }
}
