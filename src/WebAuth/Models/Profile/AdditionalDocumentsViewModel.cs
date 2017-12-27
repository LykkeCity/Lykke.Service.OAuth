namespace WebAuth.Models.Profile
{
    public class AdditionalDocumentsViewModel : StepViewModel
    {
        public DocumentViewModel FirstDocument { get; set; }

        public DocumentViewModel SecondDocument { get; set; }

        public AdditionalDocumentsViewModel()
        {
            StepNumber = 6;
            Title = "Additional documents";
            Description = @"Add more documents to complete your profile.";

            FirstDocument = new DocumentViewModel();
            SecondDocument = new DocumentViewModel();
        }

    }
}