using System;
using Common;
using Common.PasswordTools;
using Core.Exceptions;
using MessagePack;

namespace Core.Registration
{
    [MessagePackObject(true)]
    public class RegistrationModel : IPasswordKeeping
    {
        public string RegistrationId { get; private set; }
        public string Hash { get; set; }
        public string Salt { get; set; }
        public string Email { get; }
        public string ClientId { get; private set; }
        public RegistrationStep CurrentStep { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string CountryOfResidenceIso2 { get; private set; }
        public string PhoneNumber { get; private set; }
        public DateTime Started { get; private set; }
        public string Cid { get; private set; }

        public RegistrationModel(string email, DateTime started)
        {
            Email = email?.ToLower();
            CurrentStep = RegistrationStep.InitialInfo;
            RegistrationId = GenerateId();
            Started = started;
        }

        public RegistrationModel(IRegistrationModelDto dto)
        {
            RegistrationId = dto.RegistrationId;
            Email = dto.Email;
            Hash = dto.PasswordHash;
            Salt = dto.PasswordSalt;
            ClientId = dto.ClientId;
            CurrentStep = dto.CurrentStep;
            Started = dto.Started;
        }

        public void CompleteInitialInfoStep(InitialInfoDto context)
        {
            if (CurrentStep != RegistrationStep.InitialInfo)
                throw new InvalidRegistrationStateTransitionException(CurrentStep, RegistrationStep.InitialInfo);

            if (context.Email.ToLower() != Email)
                throw new RegistrationEmailMatchingException(context.Email);
            if (!IsPasswordComplex(context.Password))
                throw new PasswordIsNotComplexException();

            ClientId = context.ClientId;
            this.SetPassword(context.Password);

            CurrentStep = RegistrationStep.AccountInformation;

            Cid = context.Cid;
        }

        public void CompleteAccountInfoStep(AccountInfoDto context)
        {
            if (CurrentStep != RegistrationStep.AccountInformation)
                throw new InvalidRegistrationStateTransitionException(CurrentStep, RegistrationStep.AccountInformation);

            if (!IsPhoneNumberFormatCorrect(context.PhoneNumber))
                throw new InvalidPhoneNumberFormatException(context.PhoneNumber);

            FirstName = context.FirstName;
            LastName = context.LastName;
            CountryOfResidenceIso2 = context.CountryCodeIso2;
            PhoneNumber = context.PhoneNumber;

            CurrentStep = RegistrationStep.Pin;
        }

        public void SetRegistrationId(string registrationId, DateTime started)
        {
            RegistrationId = registrationId;
            Started = started;
        }

        public static bool IsPhoneNumberFormatCorrect(string phoneNumber)
        {
            return phoneNumber?.PreparePhoneNum()?.ToE164Number() != null;
        }

        public static bool IsPasswordComplex(string password)
        {
            return password.IsPasswordComplex(8, 128, true, false);
        }

        public bool CanEmailBeUsed()
        {
            return CurrentStep == RegistrationStep.InitialInfo;
        }

        private static string GenerateId()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                .Replace("/", "_")
                .Replace("+", "-")
                .Substring(0, 22);
        }
    }
}
