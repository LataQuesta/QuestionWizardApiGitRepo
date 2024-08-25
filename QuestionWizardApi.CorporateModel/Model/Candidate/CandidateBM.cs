using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestionWizardApi.CorporateModel.Model
{
    public class CandidateBM
    {
        public int UserId { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string UserEmail { get; set; }
        public string PhoneNumber { get; set; }
        public Nullable<DateTime> UserDateOfBirth { get; set; }
        public Nullable<int> UserGender { get; set; }
        public Nullable<int> UserAge { get; set; }
        public Nullable<int> UserState { get; set; }
        public Nullable<int> UserCountry { get; set; }
        public Nullable<int> UserQualification { get; set; }
        public Nullable<int> UserProfessional { get; set; }
        public Nullable<int> UserMaritalStatus { get; set; }
        public Nullable<int> UserEmployeeStatus { get; set; }
        public Nullable<int> UserExperience { get; set; }
        public Nullable<System.DateTime> CreatedAt { get; set; }
        public Nullable<int> ProfileId { get; set; }
        public Nullable<bool> IsOTPRequire { get; set; }
        public string TransId { get; set; }
        public string TransStatus { get; set; }
        public Nullable<System.DateTime> LastModified { get; set; }
        public Nullable<bool> IsInitialMailRequire { get; set; }
        public Nullable<bool> IsAttachmentSent { get; set; }
        public Nullable<bool> IsFinalMailRequire { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<bool> IsMobileDevice { get; set; }
        public Nullable<bool> IsDesktopDevice { get; set; }
        public Nullable<bool> IsTabDevice { get; set; }
        public string BrowserName { get; set; }
        public Nullable<bool> IsLogin { get; set; }
        public int MainType { get; set; }
        public int FellingCenter { get; set; }
        public int ThinkingCenter { get; set; }
        public int ActionCenter { get; set; }
        public int UserTestId { get; set; }
        public string status { get; set; }
        public int CompanyId { get; set; }
        public int ModuleId { get; set; }
        public Nullable<DateTime> AssessmentCompleteDate { get; set; }
    }


    public class CandidateModel
    {
        public int UserId { get; set; }
        public int TestId { get; set; }
        public string AssessmentId { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserEmail { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public int UserGender { get; set; }
        public int UserAge { get; set; }
        public int State { get; set; }
        public int Country { get; set; }
        public int Qualification { get; set; }
        public int Professional { get; set; }
        public string GenderTxt { get; set; }
        public int MaritalStatus { get; set; }
        public string[] Industry { get; set; }
        public string QualificationTxt { get; set; }
        public int EmployeeStatus { get; set; }
        public int Experience { get; set; }
        public bool IsOTPRequire { get; set; }
        public bool IsActive { get; set; }
        public bool IsMobileDevice { get; set; }
        public bool IsDesktopDevice { get; set; }
        public bool IsTabDevice { get; set; }
        public string BrowserName { get; set; }
        public bool IsLogin { get; set; }
        public int MainType { get; set; }
        public int FellingCenter { get; set; }
        public int ThinkingCenter { get; set; }
        public int ActionCenter { get; set; }
        public int CompanyId { get; set; }
        public int ModuleId { get; set; }
    }
    public class OTPModel
    {
        public string PhoneNumber { get; set; }
        public string email { get; set; }
    }


    public class HumanResourceRepo
    {
        public int HrId { get; set; }
        public string HrName { get; set; }
        public string HrEmail { get; set; }
        public string HrPhoneNumber { get; set; }
        public Nullable<bool> IsBulkSentRequire { get; set; }
        public Nullable<bool> IsReportSentToCandidate { get; set; }
        public Nullable<bool> IsReportSentToHr { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<int> CompanyId { get; set; }
        public Nullable<int> LinkCount { get; set; }
        public Nullable<bool> IsLinkSentToCandidate { get; set; }
        public Nullable<bool> IsLinkSentToHr { get; set; }

    }
}
