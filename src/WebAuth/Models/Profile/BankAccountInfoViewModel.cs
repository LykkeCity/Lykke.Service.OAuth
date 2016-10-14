namespace WebAuth.Models.Profile
{
    public class BankAccountInfoViewModel :StepViewModel
    {
        public DocumentViewModel Funds { get; set; }

        public DocumentViewModel BankAccount { get; set; }

        public BankAccountInfoViewModel()
        {
            StepNumber = 5;
            Title = "Bank Account Information";
            Description = @"Confirm your employment, the availability of sufficient funds in your bank account.";

            Funds = new DocumentViewModel();
            BankAccount = new DocumentViewModel();
        }
    }
}