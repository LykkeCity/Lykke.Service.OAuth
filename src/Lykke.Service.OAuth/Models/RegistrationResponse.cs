namespace WebAuth.Controllers
{
    public class RegistrationResponse
    {
        public RegistrationResponse(string registrationState)
        {
            RegistrationState = registrationState;
        }

        public string RegistrationState { get; }
    }
}
