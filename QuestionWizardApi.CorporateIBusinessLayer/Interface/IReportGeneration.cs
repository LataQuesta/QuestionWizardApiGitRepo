using System.Threading.Tasks;

namespace QuestionWizardApi.CorporateIBusinessLayer.Interface
{
    public interface IReportGeneration
    {
        Task GenerateCandidateReport(int TestId, int AssessmentId, string CandidateName, string hostname);
        void Dispose();
    }
}
