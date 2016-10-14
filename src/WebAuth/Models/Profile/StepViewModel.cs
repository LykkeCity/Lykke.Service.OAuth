namespace WebAuth.Models.Profile
{
    public class StepViewModel : ViewModel
    {
        public int StepNumber { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string ErrorDetails { get; set; }

        public string NextStepUrl { get; set; }

        public string PrevStepUrl { get; set; }
    }
}