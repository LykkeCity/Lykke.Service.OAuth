namespace Core.ExternalProvider
{
    public class IroncladUser : BaseUser
    {
        public string LykkeUserId { get; set; }

        public string Idp { get; set; }

        public IroncladUser()
        {
            
        }

        public IroncladUser(BaseUser user) : base(user)
        {

        }
    }
}
