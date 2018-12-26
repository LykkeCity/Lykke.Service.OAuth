namespace Core.ExternalProvider
{
    public class LykkeUser : BaseUser
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }
        
        public string Country { get; set; }
        
        public string PartnerId { get; set; }
        
        public LykkeUser()
        {
            
        }

        public LykkeUser(IroncladUser user)
        {
            Id = user.LykkeUserId;
            Email = user.Email;
            Phone= user.Phone;
            PhoneVerified = user.PhoneVerified;
            EmailVerified = user.EmailVerified;
        }

        public LykkeUser(BaseUser user) : base(user)
        {

        }
    }
}
