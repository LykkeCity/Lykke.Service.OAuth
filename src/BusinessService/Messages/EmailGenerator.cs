using System.Threading.Tasks;
using BusinessService.Messages.EmailTemplates.ViewModels;
using BusinessService.Messages.Resources;
using Core.Clients;
using Core.Messages;
using Core.Messages.Email;
using Core.Messages.Email.MessagesData;

namespace BusinessService.Messages
{
    public class EmailGenerator : IEmailGenerator
    {
        private readonly TemplateGenerator _localTemplateGenerator;
        private readonly ITemplateGenerator _templateGenerator;

        public EmailGenerator(ITemplateGenerator templateGenerator, TemplateGenerator localTemplateGenerator)
        {
            _templateGenerator = templateGenerator;
            _localTemplateGenerator = localTemplateGenerator;
        }

        public async Task<EmailMessage> GenerateWelcomeMsg(RegistrationData kycOkData)
        {
            var templateVm = new BaseTemplate
            {
                Year = kycOkData.Year
            };

            return new EmailMessage
            {
                Body = await _templateGenerator.GenerateAsync("WelcomeTemplate", templateVm, TemplateType.Email),
                Subject = EmailResources.Welcome_Subject,
                IsHtml = true
            };
        }

        public async Task<EmailMessage> GenerateConfirmEmailMsg(EmailComfirmationData registrationData)
        {
            var templateVm = new EmailVerificationTemplate
            {
                ConfirmationCode = registrationData.ConfirmationCode,
                Year = registrationData.Year
            };

            return new EmailMessage
            {
                Body = await _templateGenerator.GenerateAsync("EmailConfirmation", templateVm, TemplateType.Email),
                Subject = EmailResources.EmailConfirmation_Subject,
                IsHtml = true
            };
        }

        public async Task<EmailMessage> GenerateUserRegisteredMsg(IPersonalData personalData)
        {
            var templateVm = new UserRegisteredTemplate
            {
                ContactPhone = personalData.ContactPhone,
                Country = personalData.Country,
                DateTime = personalData.Regitered,
                Email = personalData.Email,
                FullName = personalData.FullName,
                UserId = personalData.Id
            };

            return new EmailMessage
            {
                Body =
                    await
                        _localTemplateGenerator.GenerateAsync("UserRegisteredTemplate", templateVm, TemplateType.Email),
                Subject = EmailResources.UserRegistered_Subject,
                IsHtml = true
            };
        }
    }
}