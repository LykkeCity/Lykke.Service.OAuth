namespace WebAuth.Models
{
    public class VerificationCodeRequest
    {
        public string Key { get; set; }
        public string Code { get; set; }
        public string Phone { get; set; }
    }
}
