namespace WebAuth.Models
{
    public class ViewModel
    {
        public string ReturnUrl { get; set; }

        public ViewModel(string returnUrl) : this()
        {
            ReturnUrl = returnUrl;
        }

        public ViewModel()
        {
        }
    }
}