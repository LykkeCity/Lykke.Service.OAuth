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
        public RegistrationStep ActiveStep { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string CountryCodeIso2 { get; private set; }
        public string PhoneNumber { get; private set; }

        public RegistrationModel(string email)
        {
            Email = email;
            ActiveStep = RegistrationStep.InitialInfo;
            RegistrationId = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                .Replace("/", "_")
                .Replace("+", "-")
                .Substring(0, 22);
        }

        public void CompleteStep(object context)
        {
            switch (ActiveStep)
            {
                case RegistrationStep.InitialInfo:
                {
                    if (context is InitialInfoDto ctx)
                    {
                        if (ctx.Email != Email)
                            throw new ArgumentException("Email doesn't match to verified one.");
                        if (!IsPasswordComplex(ctx.Password))
                            throw new PasswordIsNotComplexException();

                        ClientId = ctx.ClientId;
                        this.SetPassword(ctx.Password);

                        break;
                    }

                    throw new InvalidRegistrationStepContext(ActiveStep);
                }
                case RegistrationStep.AccountInformation:
                {
                    if (context is AccountInfoDto ctx)
                    {
                        if (!IsPhoneNumberFormatCorrect(ctx.PhoneNumber))
                            throw new InvalidPhoneNumberFormatException(ctx.PhoneNumber);

                        FirstName = ctx.FirstName;
                        LastName = ctx.LastName;
                        CountryCodeIso2 = ctx.CountryCodeIso2;
                        PhoneNumber = ctx.PhoneNumber;

                        break;
                    }

                    throw new InvalidRegistrationStepContext(ActiveStep);
                }
                default:
                    throw new NotImplementedException();
            }

            ActiveStep += 1;
        }

        public static bool IsPhoneNumberFormatCorrect(string phoneNumber)
        {
            return phoneNumber.PreparePhoneNum().ToE164Number() != null;
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
            return ActiveStep == RegistrationStep.InitialInfo;
        }
    }
}
