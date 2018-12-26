namespace Core.ExternalProvider
{
    public class BaseUser
    {
        public string Id { get; set; }

        public string Email { get; set; }

        public bool EmailVerified { get; set; }

        public string Phone { get; set; }

        public bool PhoneVerified { get; set; }

        public BaseUser()
        {
            
        }

        public BaseUser(BaseUser user)
        {
            Id = user.Id;
            Email = user.Email;
            Phone= user.Phone;
            PhoneVerified = user.PhoneVerified;
            EmailVerified = user.EmailVerified;
        }
    }
}
