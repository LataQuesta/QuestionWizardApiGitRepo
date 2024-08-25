using QuestionWizardApi.CorporateModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestionWizardApi.CorporateIBusinessLayer.Interface
{
    public interface IUser
    {
        bool IsProfileIdExists(int ProfileId);
        bool IsValidCompany(int CompanyId);
        bool IsValidAssessmentSet(int AssessmentId);
        int SaveCandidateData(int ProfileId, int CompanyId, int AssessmentId);
        int GenerateRandomQuestionNumber(int UserId, int? TestId, int? TypeId, string hostName, int? AssessmentId, int? ModuleId);
        CandidateBM GetCandidateData(int TestId);
        CandidateModel GetCandidateDetails(CandidateBM ObjUserData, int TestId);
        bool Check15DaysValidation(int UserId);
        bool SaveCandidateData(CandidateModel objUserModel);
        Task SentOTPViaMail(string EmailId, int OTP);
        void UpdateIdLogin(int TestId, bool IsLogin);
        string GetUrlValue(string configName);
        int GetLatestSetId(int TestId);
        int GetProgressExamSet(int TestId);
       // Tuple<bool, bool> MailRequireToSent(int TestId);
        HumanResourceRepo GetHumanResource(int CompanyId);
        int SaveCandidateRecord(string Title, string FirstName, string LastName, string email, string PhoneNumber, int ProfileId, int CompanyId, int AssessmentId);
       // int GetAssesmentIdBaseOnCompanyId(int CompanyId);

        dynamic GetDummyCandidateRecord();
        void Dispose();

    }
}
