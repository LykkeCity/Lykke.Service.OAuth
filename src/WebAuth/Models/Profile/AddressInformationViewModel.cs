namespace WebAuth.Models.Profile
{
    public class AddressInformationViewModel : StepViewModel
    {
        public string City { get; set; }

        public string Address { get; set; }

        public string Zip { get; set; }

        public DocumentViewModel IdCard { get; set; }

        public AddressInformationViewModel()
        {
            StepNumber = 3;
            Title = "Address Information";
            Description = @"Enter the details of your residence.";

            IdCard = new DocumentViewModel();
        }
    }
}