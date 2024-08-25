using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuestionWizardApi.Models.Model
{
    public class UserModel
    {

    }
    public class ClsCountry
    {
        public int countryId { get; set; }
        public string countryname { get; set; }
    }
    public class ClsState
    {
        public int stateId { get; set; }
        public int countryId { get; set; }
        public string statename { get; set; }
    }
    public class ClsQualification
    {
        public int QualificationId { get; set; }
        public string QualificationName { get; set; }
    }
    public class ClsProfession
    {
        public int ProfessionId { get; set; }
        public string ProfessionName { get; set; }
    }
    public class ClsAge
    {
        public int AgeId { get; set; }
        public string AgeName { get; set; }
    }
    public class ClsGender
    {
        public int GenderId { get; set; }
        public string GenderName { get; set; }
    }
    public class ClsMaritalStatus
    {
        public int MaritalId { get; set; }
        public string MaritalName { get; set; }
    }
    public class ClsEmployeeStatus
    {
        public int EmploymentId { get; set; }
        public string EmploymentName { get; set; }
    }
    public class ClsIndustry
    {
        public int IndustryId { get; set; }
        public string IndustryName { get; set; }
    }
    public class ClsUserModel
    {
        public int UserId { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserEmail { get; set; }
        public string PhoneNumber { get; set; }
        public int UserGender { get; set; }
        public int UserAge { get; set; }
        public int State { get; set; }
        public int Country { get; set; }
        public int Qualification { get; set; }
        public int Professional { get; set; }
        public int MaritalStatus { get; set; }
        public int EmployeeStatus { get; set; }
        public int ProfileId { get; set; }
        public bool IsOTPRequire { get; set; }
        public bool IsActive { get; set; }
        public bool IsInitialMail { get; set; }
        public bool IsAttachmentSent { get; set; }
    }

    public class CandidateModel
    {
        public int UserId { get; set; }
        public int TestId { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserEmail { get; set; }
        public string PhoneNumber { get; set; }
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
        public bool IsOTPRequire { get; set; }
        public bool IsActive { get; set; }
    }
}