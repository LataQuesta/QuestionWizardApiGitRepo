using QuestionWizardApi.CorporateModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestionWizardApi.CorporateIBusinessLayer.Interface
{
    public interface IQuestion
    {

        Task<ClsQuestionModel> LoadQuestionModel(int TestId, int SetId, int? QuestionId);

        void updateMainTypeInDB(int TestId);
        string GetCurrentExamStatus(int TestId, int SetId);
        List<Question> GetAllQuestion(int TestId, int setId);
        List<ClsQuestion> GetAllQuestionDetails(List<int> NotCompletedQuest, int TestId, int SetId);
        List<ClsTypeModel> GetTypeWiseScoreBoard(int TestId, int SetId);
        List<ClsTypeModel> GetSubTypeWiseScoreBoard(int TestId, int SetId, int? TypeId);
        void SaveQuestion(List<ClsQuestion> lstQuestion);
        void SubmitDataInRearrangeOrder(List<ClsQuestion> lstQuestion);
        void UpdateExamStatus(int TestId, int SetId, string Status);
        void CompeteUserTest(int TestId, int UserId, string Status);
        string GetUserTestStatus(int TestId);
        int GetTypeId(int TestId, int setId);
        List<QuestionSubType> lstSubType(int TypeId);
        List<ClsSet6ScoreModel> lstSubModuleScore(int Testid, int setid);
        Tuple<List<Question>, int> GetAllforSet6Question(int TestId, int setId);
        Tuple<int, int> GetNoOfQuestionComplete(int TestId);
        string GetAttachmentNameForSend(int TestId);
        string GetExamStatus(int TestId, int SetId);
        bool CheckAllMistypingModuleCompleted(int TestId);
        void SaveScoreImgOnAws(ClsNextQuestionModel ObjScoreModel, string hostname);
        bool IsAllAnswerAptitudeQuestion(int TestId, int SetId);
        void UpdateMisTypeModule(int TypeId, string Status, int TestId,int SetId);
        void GenerateQuestionByDynamicMistyping(int UserId, int SetId, int TestId, ref int RefTypeId);
        List<QuestionIdStatus> GetQuestionNumberAndStatus(int TestId, int setId);
        void UploadPdfOnS3Bucket(byte[] PdfByteArray, int TestId);
        void Dispose();
    }
}
