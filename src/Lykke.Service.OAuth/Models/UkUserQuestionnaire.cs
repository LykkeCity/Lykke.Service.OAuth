using System.Collections.Generic;

namespace WebAuth.Models
{
    public class UkUserQuestionnaire
    {
        public IReadOnlyDictionary<string, string> InvestorTypeAnswer { get; set; }
        public IReadOnlyDictionary<string, string> InvestorStatement { get; set; }
        public IReadOnlyDictionary<string, string> GeneralAnswers { get; set; }
    }
}
