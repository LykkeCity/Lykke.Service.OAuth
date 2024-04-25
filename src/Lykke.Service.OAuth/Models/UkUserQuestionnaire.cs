using System.Collections.Generic;

namespace WebAuth.Models
{
    public class UkUserQuestionnaire
    {
        public UkUserQuestionnaireInvestorTypeAnswer InvestorTypeAnswer { get; set; }
        public IReadOnlyDictionary<string, string> GeneralAnswers { get; set; }
    }
}
