using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestionWizardApi.CorporateModel.Model
{
    public class QuestionBM
    {
    }
    public class ClsQuestionModel
    {
        public int CurrentUserId { get; set; }
        public int TestId { get; set; }
        public int CurrentSetId { get; set; }
        public int CompanyId { get; set; }
        public int ModuleId { get; set; }
        public bool IsDesktop { get; set; }

        public bool IsMainType { get; set; }
        public bool IsMistyping { get; set; }
        public bool IsCentresofExpression { get; set; }
        public bool IsStressAndResilence { get; set; }
        public bool IsEnneagramInstincts { get; set; }
        public bool IsPersonalitytoPresence { get; set; }
        public bool IsProfessionalCompetencies { get; set; }

        public string CurrentSetName { get; set; }

        public int? NextSetId { get; set; }
        public string NextSetName { get; set; }

        public string NextTypeName { get; set; }
        public int? NextTypeId { get; set; }

        public string CurrentSetStatus { get; set; }
        public bool IsScordBoardDisplay { get; set; }
        public bool IsQuestionDisplay { get; set; }
        public int TotalQuestion { get; set; }
        public int CompletedQuestion { get; set; }
        public Boolean IsShowPrevButton { get; set; }
        public Boolean IsShowNextButton { get; set; }
        public Boolean IsShowSubmitButton { get; set; }
        public Boolean IsShowGoToNextSetButton { get; set; }
        public int LastQuestionId { get; set; }
        public int NextQuestion { get; set; }
        public int PrevQuestion { get; set; }
        public int ? CurrentQuestionId { get; set; }
        public List<ClsQuestion> lstQuestionModel { get; set; }
        public List<ClsTypeModel> ScoreBoard { get; set; }
        public Boolean IsTestComplete { get; set; }
        public Boolean IsAssessmentError { get; set; }
        public List<QuestionSubType> lstSubType { get; set; }
        public List<ClsSet6ScoreModel> ScoreCardForSet6 { get; set; }
        public int? SubModuleId { get; set; }
        public List<QuestionIdStatus> lstQuestionIdStatus { get; set; }
        public Boolean IsDisplayInstruction { get; set; }
        public string InstructionText { get; set; }
        public CompentencyScoreCard CompentencyScoreCard { get; set; }
    }

    public class CompentencyScoreCard
    {
        public List<ClsTypeModel> ScoreBoard { get; set; }

        public List<string> MultiBarScoreCard { get;set; }
    }
    public class Question
    {
        public int QuestionId { get; set; }
        public bool? isAnswer { get; set; }
    }

    public class QuestionIdStatus
    {
        public int QuestionId { get; set; }
        public string Status { get; set; }
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
        public bool Checked{get;set;}
        public bool Disable { get; set; } = false;
    }
    public class ClsTypeModel
    {
        public int TypeId { get; set; }
        public string TypeName { get; set; }
        public int Score { get; set; }
        public string ColorCode { get; set; }
    }

    public class ClsMultipleLineBarChart
    {
        public string Title { get; set; }
        public int TypeId { get; set; }
        public string TypeName { get; set; }
        public int Score { get; set; }
        public int Score1 { get; set; }
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

  
    public class ClsNextQuestionModel
    {
        public int UserId { get; set; }
        public int? Typeid { get; set; }
        public int TestId { get; set; }
        public int SetId { get; set; }
        public int currentSetId { get; set; }
        public List<string> ImgByte { get; set; }
    }
    public enum enumQuestionSet6Module
    {
        Blindspot = 1,
        Fixations = 2,
        Lines = 3
    }

  
    public enum enumQuestionSet3Type
    {
        ActionCentre = 10,
        FeelingCentre = 11,
        ThinkingCentre = 12
    }


    public class Repository
    {
        public int TypeId { get; set; }
        public string Description { get; set; }
    }

  
}
