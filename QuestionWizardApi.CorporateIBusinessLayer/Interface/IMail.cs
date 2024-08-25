using QuestionWizardApi.CorporateModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace QuestionWizardApi.CorporateIBusinessLayer.Interface
{
    public interface IMail
    {
        CandidateBM GetEmailAddress(int TestId);
       // MailBM GetMailConfig(int TestId, int ProfileId, bool IsInitialMail, bool IsFinalMail, bool MailToHr, bool MailToCandidate);
        void CreateDirectory();
        void DownloadFileFromS3Bucket(string FileName, string TestId);
      //  Task SentMail(MailBM ObjMailBody, string URL);
        Task FinalSentMail(MailBM ObjMailBody, string TestId, string FileName);
        Task SentFinalEmailToSupport(MailBM ObjMailBody, string TestId, string FileName);
        byte[] PdfGenerate(string RecevierName, string FileName, int MainType);
        void DeleteDirectoryAndFile(string TestId);
        //void SentFinalEmailToSupport(MailBM ObjMailBody, string TestId, string FileName, AlternateView htmlView);
        MailBM GetMailConfig(int TestId, int ProfileId, bool IsInitialMail, bool IsFinalMail);
        Task SentMail(MailBM ObjMailBody, string TestId, string URL);
        void Dispose();
    }
}
