namespace WebAuth.Models.Profile
{
    public class ProofOfAddressViewModel : StepViewModel
    {
        public DocumentViewModel ProofOfAddress { get; set; }

        public ProofOfAddressViewModel()
        {
            StepNumber = 4;
            Title = "Proof of Address";
            Description = @"Confirm you place of residence, so that we can allow you to work with the service.";

            ProofOfAddress = new DocumentViewModel();
        }
    }
}