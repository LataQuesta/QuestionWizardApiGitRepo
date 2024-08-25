using QuestionWizardApi.CorporateModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestionWizardApi.CorporateIBusinessLayer.Interface
{
    public interface IMailH1
    {
        Task FinalSentMail(ClsMailBodyModel ObjMailBody, string TestId, bool IsReportRequireToSent);
        MailBM GetSenderBody(int ProfileId, int CompanyId);
        Task InitialMailSent(MailBM ObjMailBody, string URL);
        MailBM GetFinalSenderBody(int ProfileId, int CompanyId);
        MailBM GetFinalSenderBodyToCandidate(int ProfileId, int CompanyId);
        Tuple<byte[], string> GetFileName(int TestId);

     //   Task GeneratePDF(int TestId);
        void SendIndividualQSSSRReport(int TestId, string hostname);

        void SendIndividualQMapReport(int TestId, string hostname);

        void SendIndividualStandardReport(int TestId, string hostname);

        void SendIndividualQLeapReport(int TestId, string hostname);

        void SendUrlPdfToMail(string url);
        void Dispose();
    }
}
