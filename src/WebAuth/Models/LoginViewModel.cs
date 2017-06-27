namespace WebAuth.Models
{
    public class LoginViewModel
    {
        public SigninViewModel Signin { get; set; }
        public RegistrationViewModel Registration { get; set; }

        public LoginViewModel(SigninViewModel login = null, RegistrationViewModel registration = null)
        {
            Signin = login;
            Registration = registration;
        }

        public LoginViewModel(string returnUrl, string referer)
        {
            if (Signin == null)
            {
                Signin = new SigninViewModel(returnUrl);
            }
            else
            {
                Signin.ReturnUrl = returnUrl;
            }

            if (Registration == null)
            {
                Registration = new RegistrationViewModel(returnUrl, referer);
            }
            else
            {
                Registration.ReturnUrl = returnUrl;
            }
        }
    }
}