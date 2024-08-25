using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuestionWizardApi.Models
{
    public class ClsQuestionModel
    {
        public int CurrentUserId { get; set; }
        public int TestId { get; set; }
        public int CurrentSetId { get; set; }
        public string CurrentSetName { get; set; }
        public int? NextSetId { get; set; }
        public string NextTypeName { get; set; }
        public int? NextTypeId { get; set; }
        public string CurrentSetStatus { get; set; }
        public bool IsScordBoardDisplay { get; set; }
        public bool IsQuestionDisplay { get; set; }
        public int TotalQuestion { get; set; }
        public int CompletedQuestion { get; set; }
        public Boolean IsShowNextButton { get; set; }
        public Boolean IsShowSubmitButton { get; set; }
        public Boolean IsShowGoToNextSetButton { get; set; }
        public int LastQuestionId { get; set; }
        public int NextQuestion { get; set; }
        public int PrevQuestion { get; set; }
        public List<ClsQuestion> lstQuestionModel { get; set; }
        public List<ClsTypeModel> ScoreBoard { get; set; }
        public Boolean IsTestComplete { get; set; }
        public List<QuestionSubType> lstSubType { get; set; }
        public List<ClsSet6ScoreModel> ScoreCardForSet6 { get; set; }
        public int? SubModuleId { get; set; }
    }
    
    public class Question
    {
        public int QuestionId { get; set; }
        public bool? isAnswer { get; set; }
    }
    public class ClsQuestion
    {
        public int TestQuestionId { get; set; }
        public int QuestionId { get; set; }
        public string Question { get; set; }
        public string QuesId { get; set; }
        public int? TypeId { get; set; }
        public int? ResponseTypeId { get; set; }
        public int? ResponseValue { get; set; }
        public List<ClsQuestionResponse> lstQuestionRes { get; set; }
        public int? ImpactScore { get; set; }
        public int? TestId { get; set; }
        public int Rating { get; set; }
    }

    public class ClsQuestionResponse
    {
        public int ResponseId { get; set; }
        public string ResponseText { get; set; }
        public int Weight { get; set; }
        public int SubTypeId { get; set; }
        public string SubTypeName { get; set; }
        public int ResponseNumber { get; set; }
    }
    public class ClsTypeModel
    {
        public int TypeId { get; set; }
        public string TypeName { get; set; }
        public int Score { get; set; }
        public string ColorCode { get; set; }
    }
    public class ClsSet6ScoreModel
    {
        public int SubModuleId { get; set; }
        public string SubModuleName { get; set; }
        public int PersonalityScore { get; set; }
         public int PresenceScore { get; set; }
    }
    public class ClsQuestionSetStatusCode
    {
        public int SetId { get; set; }
        public string SetName { get; set; }
        public string PartialSetName { get; set; }
        public string StatusCode { get; set; }
        public int CompletePercentage { get; set; }
    }

    public class QuestionSubType
    {
        public int SubTypeId { get; set; }
        public string SubTypeName { get; set; }
        public int TypeId { get; set; }
    }

    public enum enumQuestionSet
    {
        Set1 = 1,
        Set2 = 2,
        Set3 = 3,
        Set4 = 4,
        Set5 = 5,
        Set6 = 6
    }
    public class ClsNextQuestionModel
    {
        public int UserId { get; set; }
        public int? Typeid { get; set; }
        public int TestId { get; set; }
        public int SetId { get; set; }
    }
    public enum enumQuestionSet6Module
    {
        Blindspot = 1,
        Fixations = 2,
        Lines = 3
    }

    public enum enumQuestionSet1Type
    {
        EthicalPerfectionistEnneagramType = 1,
        EmpathicNurturerEnneagramType = 2,
        AmbitiousAchieverEnneagramType = 3,
        ExpressiveIndividualistEnneagramType = 4,
        PerceptiveSpecialistEnneagramType = 5,
        HardworkingLoyalistEnneagramType = 6,
        VersatileVisionaryEnneagramType = 7,
        CharismaticControllerEnneagramType = 8,
        AccommodatingPeacemakerEnneagramType = 9
    }

    public enum enumQuestionSet3Type
    {
        ActionCentre = 10,
        FeelingCentre = 11,
        ThinkingCentre = 12
    }

    public enum enumQuestionSet4Type
    {
        EnvironmentalStressResponse = 47,
        PhysicalStressResponse = 48,
        InterpersonalStressResponse = 49,
        PsychologicalStressResponse = 50,
        RelationshipStressResponse = 51,
        ProfessionalStressResponse = 52,
        Optimism = 53
    }
    //public enum enumQuestionType
    //{
    //    Type1 = 1,
    //    Type2 = 2,
    //    Type3 = 3,
    //    Type4 = 4,
    //    Type5 = 5,
    //    Type6 = 6,
    //    Type7 = 7,
    //    Type8 = 8,
    //    Type9 = 9
    //}



    //public class QuestionTab
    //{
    //    public int TabId { get; set; }
    //    public int NextQ { get; set; }
    //    public int PrevQ { get; set; }
    //}
    //public class QuestionModel
    //{
    //    public int NextQuestion { get; set; }
    //    public int PrevQuestion { get; set; }
    //    public int TestId { get; set; }
    //    public int CompletedQuestion { get; set; }
    //    public int TotalQuestion { get; set; }
    //    public Boolean IsShowNextButton { get; set; }
    //    public Boolean IsShowSubmitButton { get; set; }
    //    public int? SetId { get; set; }
    //    public string ExamSet1Status { get; set; }
    //    public string ExamSet2Status { get; set; }
    //    public List<ClsQuestion> lstQuestionModel { get; set; }
    //}

}