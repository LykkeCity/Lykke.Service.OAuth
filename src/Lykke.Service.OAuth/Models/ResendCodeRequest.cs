namespace WebAuth.Models
{
    public class ResendCodeRequest
    {
        public string Key { get; set; }
        public string Captcha { get; set; }
    }
}
