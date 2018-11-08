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

        public RegistrationModel(string email)
        {
            Email = email;
            CurrentStep = RegistrationStep.InitialInfo;
            RegistrationId = GenerateId();
        }

        private static string GenerateId()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                .Replace("/", "_")
                .Replace("+", "-")
                .Substring(0, 22);
        }

        public void CompleteInitialInfoStep(InitialInfoDto context)
        {
            if (CurrentStep != RegistrationStep.InitialInfo)
                throw new InvalidRegistrationStateTransitionException(CurrentStep, RegistrationStep.InitialInfo);

            if (context.Email != Email)
                throw new RegistrationEmailMatchingException(context.Email);
            if (!IsPasswordComplex(context.Password))
                throw new PasswordIsNotComplexException();

            ClientId = context.ClientId;
            this.SetPassword(context.Password);

            CurrentStep = RegistrationStep.AccountInformation;
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

        public static bool IsPhoneNumberFormatCorrect(string phoneNumber)
        {
            return phoneNumber?.PreparePhoneNum()?.ToE164Number() != null;
        }

        public static bool IsPasswordComplex(string password)
        {
            return password.IsPasswordComplex(8, 128, true, false);
        }

        public void SetRegistrationId(string registrationId)
        {
            RegistrationId = registrationId;
        }

        public bool CanEmailBeUsed()
        {
            return CurrentStep == RegistrationStep.InitialInfo;
        }
    }
}
