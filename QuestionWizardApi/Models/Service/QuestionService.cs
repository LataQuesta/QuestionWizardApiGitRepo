using QuestionWizardApi.Models.Repository;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace QuestionWizardApi.Models
{
    public class QuestionService : Repository<txnQuestion>
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        public QuestionService(QuestionDemoEntities context) : base(context) { }
        public QuestionDemoEntities DBEntities
        {
            get
            {
                return db as QuestionDemoEntities;
            }
        }


        public string GetCurrentExamStatus(int TestId, int SetId)
        {
            string ExamStatus = DBEntities.txnExamSetStatus.Where(x => x.TestId == TestId && x.SetId == SetId).Select(x => x.Status).FirstOrDefault();
            return ExamStatus;
        }

        public List<Question> GetAllQuestion(int TestId, int setId)
        {
            try
            {
                List<Question> Question = DBEntities.txnQuestions.Where(x => x.TestId == TestId && x.Setid == setId).Select(x => new Question
                {
                    QuestionId = (int)x.QuestionId,
                    isAnswer = x.IsAnswer.Value
                }).ToList();
                return Question;
            }
            catch (Exception ex)
            {
                throw;
            }

        }

      //  public List<ClsQuestion> GetAllQuestionDetails(int NextQuestion, int PrevQuestion, int TestId, int SetId)
        public List<ClsQuestion> GetAllQuestionDetails(List<int> NotCompletedQuest, int TestId, int SetId)
        {
            try
            {
                ClsQuestionModel objQuestion = new ClsQuestionModel();
                List<int> QuestionIds = new List<int>();
                List<int> FinalQuestionIds = new List<int>();
                QuestionIds = GetAllQuestion(TestId, SetId).Select(x => x.QuestionId).ToList();

                // int startIdx = QuestionIds.FindIndex(a => a == PrevQuestion);
                // int lastIdx = QuestionIds.FindIndex(a => a == NextQuestion);
                int j = 0;
                 foreach(var i in NotCompletedQuest)
                {
                    j++;
                    FinalQuestionIds.Add(i);
                    if(j == 8 && SetId != 3)
                    {
                        break;
                    }
                }

                objQuestion.lstQuestionModel = (from q in DBEntities.txnQuestions
                                                join mq in DBEntities.mstQuestions
                                                on q.QuestionId equals mq.QuestionId

                                                let ListQuestionRes = (from Ques in DBEntities.mstQuestions
                                                                       join Quesres in DBEntities.mstQuestionResponses
                                                                       on Ques.QuestionId equals Quesres.QuestionId
                                                                       join QuesSubType in DBEntities.mstQuestionSubTypes
                                                                       on Quesres.SubTypeId equals QuesSubType.SubTypeId into SubType
                                                                       from i in SubType.DefaultIfEmpty()
                                                                       where q.QuestionId == Quesres.QuestionId
                                                                       select new ClsQuestionResponse
                                                                       {
                                                                           ResponseId = Quesres.ResponseId,
                                                                           ResponseText = Quesres.ResponseText,
                                                                           Weight = Quesres.weight.HasValue ? Quesres.weight.Value : 0,
                                                                           SubTypeId = Quesres.SubTypeId.HasValue ? Quesres.SubTypeId.Value : 0,
                                                                           SubTypeName = i.SubTypeName == null ? null : i.SubTypeName,
                                                                           ResponseNumber = Quesres.ResponseNumber.Value
                                                                       }).ToList()
                                                let ResponseValue = DBEntities.txnQuestionResponses.Where(x => x.QuestionId == q.QuestionId && x.TestId == q.TestId).FirstOrDefault()
                                                where FinalQuestionIds.Contains((int)q.QuestionId)
                                                      && q.TestId == TestId && q.Setid == SetId && mq.IsActive == true
                                                select new ClsQuestion
                                                {
                                                    TestQuestionId = q.TestQuestionId,
                                                    QuestionId = q.QuestionId.HasValue ? q.QuestionId.Value : 0,
                                                    Question = mq.Question,
                                                    QuesId = "Question_" + q.QuestionId.Value,
                                                    ResponseTypeId = mq.ResponseTypeId.Value,
                                                    TypeId = mq.TypeId.Value,
                                                    lstQuestionRes = ListQuestionRes,
                                                    ResponseValue = ResponseValue.QuestionResponseId.Value,
                                                    ImpactScore = q.ImpactScore.Value,
                                                    TestId = q.TestId.Value
                                                }).ToList();

                return objQuestion.lstQuestionModel;
            }
            catch (Exception ex)
            {
                throw;
            }

        }


        public async Task<List<ClsTypeModel>> GetTypeWiseScoreBoard(int TestId, int SetId)
        {

            try
            {
                List<ClsTypeModel> lstTypeWiseScoreCard = new List<ClsTypeModel>();

                await Task.Run(() =>
                {
                    lstTypeWiseScoreCard = DBEntities.usp_TypeWiseScoreBoard(testId: TestId, sETId: SetId).Select(x => new ClsTypeModel
                    {
                        TypeId = x.TypeId.HasValue ? x.TypeId.Value : 0,
                        TypeName = x.TypeName,
                        Score = x.ImpactScore.HasValue ? x.ImpactScore.Value : 0
                    }).ToList();

                    var ScoreCardName = DBEntities.usp_GetScoreCardName(testId: TestId, setId: SetId).ToList();

                    foreach (var Name in ScoreCardName)
                    {
                        if (!(lstTypeWiseScoreCard.Any(x => x.TypeId == Name.Id)))
                        {
                            lstTypeWiseScoreCard.Add(new ClsTypeModel
                            {
                                TypeId = Name.Id.Value,
                                TypeName = Name.Name,
                                Score = 0
                            });
                        }
                    }

                });

                return lstTypeWiseScoreCard;
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        public async Task<List<ClsTypeModel>> GetSubTypeWiseScoreBoard(int TestId, int SetId)
        {
            try
            {
                List<ClsTypeModel> lstSubTypeWiseScoreCard = new List<ClsTypeModel>();
                await Task.Run(() =>
                {
                    lstSubTypeWiseScoreCard = DBEntities.usp_SubTypeWiseScoreBoard(TestId, SetId).Select(x => new ClsTypeModel
                    {
                        TypeId = x.SubTypeId.HasValue ? x.SubTypeId.Value : 0,
                        TypeName = x.SubTypeName,
                        Score = x.ImpactScore.HasValue ? x.ImpactScore.Value : 0
                    }).ToList();

                    var ScoreCardName = DBEntities.usp_GetScoreCardName(testId: TestId, setId: SetId).ToList();

                    foreach (var Name in ScoreCardName)
                    {
                        if (!(lstSubTypeWiseScoreCard.Any(x => x.TypeId == Name.Id)))
                        {
                            lstSubTypeWiseScoreCard.Add(new ClsTypeModel
                            {
                                TypeId = Name.Id.Value,
                                TypeName = Name.Name,
                                Score = 0
                            });
                        }
                    }
                });

                return lstSubTypeWiseScoreCard;
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        public int GetCurrentUserid(int TestId)
        {
            int UserId = DBEntities.txnUserTestDetails.Where(x => x.TestId == TestId).Select(x => x.UserId.Value).FirstOrDefault();
            return UserId;
        }




        public void SaveQuestion(List<ClsQuestion> lstQuestion)
        {
            try
            {
                foreach (var data in lstQuestion)
                {

                    if (!(DBEntities.txnQuestionResponses.Any(x => x.TestId == data.TestId.Value && x.QuestionId == data.QuestionId && x.TestQuestionId == data.TestQuestionId)))
                    {
                        txnQuestionResponse objQuesResponse = new txnQuestionResponse();
                        objQuesResponse.QuestionId = data.QuestionId;
                        objQuesResponse.QuestionResponseId = data.ResponseValue;
                        objQuesResponse.TestQuestionId = data.TestQuestionId;
                        objQuesResponse.TestId = data.TestId.Value;
                        objQuesResponse.TypeId = data.TypeId.Value;
                        DBEntities.txnQuestionResponses.Add(objQuesResponse);


                        txnQuestion Question = DBEntities.txnQuestions.Where(x => x.QuestionId == data.QuestionId && x.TestId == data.TestId && x.TestQuestionId == data.TestQuestionId).FirstOrDefault();

                        txnUserTestDetail UserDetails = DBEntities.txnUserTestDetails.Where(x => x.TestId == data.TestId.Value).FirstOrDefault();

                        if (Question != null)
                        {
                            var weight = DBEntities.mstQuestionResponses.Where(x => x.ResponseId == data.ResponseValue
                                                                                       && x.QuestionId == data.QuestionId).FirstOrDefault();

                            DateTime DateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
                            Question.ImpactScore = weight.weight;
                            Question.ResponseAt = DateTime;//DateTime.UtcNow;
                            Question.IsAnswer = true;
                            Question.ResponseBy = UserDetails.UserId;
                            DBEntities.Entry(Question).State = System.Data.Entity.EntityState.Modified;
                        }
                    }
                }

                DBEntities.SaveChanges();
            }
            catch (Exception ex)
            {
                throw;
            }

        }



        public void SubmitDataInRearrangeOrder(List<ClsQuestion> lstQuestion)
        {
            try
            {
                foreach (var Questiondata in lstQuestion)
                {
                    var SubTypeData = lstSubType(Questiondata.TypeId.Value).Select(x => x.SubTypeId);



                    txnQuestionResponse objQuesResponse = new txnQuestionResponse();
                    List<txnQuestionResponse> lstQuesResponse = new List<txnQuestionResponse>();
                    int Score;
                    for (int i = 0; i <= Questiondata.lstQuestionRes.Count - 1; i++)
                    {
                        objQuesResponse.QuestionId = Questiondata.QuestionId;
                        objQuesResponse.QuestionResponseId = Questiondata.lstQuestionRes[i].ResponseId;
                        objQuesResponse.TestQuestionId = Questiondata.TestQuestionId;
                        objQuesResponse.TestId = Questiondata.TestId.Value;
                        objQuesResponse.TypeId = Questiondata.TypeId.Value;
                        mstQuestionResponse SubTypeId = DBEntities.mstQuestionResponses.Where(x => x.ResponseId == objQuesResponse.QuestionResponseId).
                                        FirstOrDefault();

                        objQuesResponse.SubTypeId = SubTypeId.SubTypeId;//SubTypeData.ElementAt(i);
                        Score = i == 0 ? 3 : i == 1 ? 2 : i == 2 ? 1 : 0;
                        lstQuesResponse.Add(new txnQuestionResponse
                        {
                            QuestionId = objQuesResponse.QuestionId,
                            QuestionResponseId = objQuesResponse.QuestionResponseId,
                            TestQuestionId = objQuesResponse.TestQuestionId,
                            TestId = objQuesResponse.TestId,
                            TypeId = objQuesResponse.TypeId,
                            SubTypeId = objQuesResponse.SubTypeId,
                            impactscore = Score
                        });

                    }
                    // DBEntities.Set<txnQuestionResponse>().AddRange(lstQuesResponse);
                    //  DBEntities.txnQuestionResponses.AddRange(lstQuesResponse);
                    DBEntities.txnQuestionResponses.AddRange(lstQuesResponse.
                       Select(x => new txnQuestionResponse
                       {
                           QuestionId = x.QuestionId,
                           QuestionResponseId = x.QuestionResponseId,
                           TestQuestionId = x.TestQuestionId,
                           TestId = x.TestId,
                           TypeId = x.TypeId,
                           SubTypeId = x.SubTypeId,
                           impactscore = x.impactscore
                       }));
                    //DBEntities.SaveChanges();
                    txnQuestion Question = DBEntities.txnQuestions.Where(x => x.QuestionId == Questiondata.QuestionId && x.TestId == Questiondata.TestId && x.TestQuestionId == Questiondata.TestQuestionId).FirstOrDefault();

                    txnUserTestDetail UserDetails = DBEntities.txnUserTestDetails.Where(x => x.TestId == Questiondata.TestId.Value).FirstOrDefault();
                    if (Question != null)
                    {
                        DateTime DateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
                        Question.ResponseAt = DateTime; //DateTime.UtcNow;
                        Question.IsAnswer = true;
                        Question.ResponseBy = UserDetails.UserId;
                        DBEntities.Entry(Question).State = System.Data.Entity.EntityState.Modified;
                    }
                    DBEntities.SaveChanges();
                }

                // DBEntities.SaveChanges();
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        public void UpdateExamStatus(int TestId, int SetId, string Status)
        {
            try
            {
                txnExamSetStatu objExamStatus = DBEntities.txnExamSetStatus.Where(x => x.TestId == TestId && x.SetId == SetId).FirstOrDefault();
                objExamStatus.Status = Status;
                DateTime DateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
                objExamStatus.LastModifiedAt = DateTime;

                DBEntities.Entry(objExamStatus).State = System.Data.Entity.EntityState.Modified;

                DBEntities.SaveChanges();
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        public List<ClsQuestionSetStatusCode> GetExamSetStatusCode(int TestId)
        {
            try
            {
                List<ClsQuestionSetStatusCode> lstExamStatusCode = new List<ClsQuestionSetStatusCode>();

                lstExamStatusCode = (from i in DBEntities.txnExamSetStatus
                                     join j in DBEntities.mstExamSets
                                     on i.SetId equals j.SetId
                                     where i.TestId == TestId
                                     orderby j.SetOrder ascending
                                     select new ClsQuestionSetStatusCode
                                     {
                                         SetId = i.SetId.Value,
                                         SetName = j.SetName,
                                         PartialSetName = j.PartialSetName,
                                         StatusCode = i.Status
                                     }).ToList();

                foreach (var SetStatusCode in lstExamStatusCode)
                {
                    SetStatusCode.CompletePercentage = CompleteQuestionPercentage(TestId, SetStatusCode.SetId, SetStatusCode.StatusCode);
                }

                return lstExamStatusCode;
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        public int CompleteQuestionPercentage(int TestId, int SetId, string Statuscode)
        {
            try
            {
                int TotalQuestion, CompleteQuestion, Percentage = 0;
                if (Statuscode == "P")
                {
                    TotalQuestion = GetAllQuestion(TestId, SetId).Count;
                    CompleteQuestion = GetAllQuestion(TestId, SetId).Where(x => x.isAnswer != null).Count();
                    Percentage = (int)((double)CompleteQuestion / TotalQuestion * 100);
                }
                else if (Statuscode == "C")
                {
                    Percentage = 100;
                }
                else if (Statuscode == "NS")
                {
                    Percentage = 0;
                }

                return Percentage;
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        public int? GetQuestionTypeId(string TypeName)
        {
            try
            {
                int TypeId;
                var QuestionTypeId = DBEntities.mstQuestionTypes.Where(x => x.TypeName == TypeName).Select(x => x.TypeId).FirstOrDefault();
                TypeId = QuestionTypeId == null ? QuestionTypeId : Convert.ToInt32(QuestionTypeId);

                return TypeId;
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        public void CompeteUserTest(int TestId, int UserId)
        {
            try
            {
                txnUserTestDetail UserTestDetail = DBEntities.txnUserTestDetails.Where(x => x.TestId == TestId && x.UserId == UserId).FirstOrDefault();

                if (UserTestDetail != null)
                {
                    UserTestDetail.status = "C";
                    DateTime DateTimeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
                    UserTestDetail.LastModifiedAt = DateTimeNow;//DateTime.UtcNow;
                    DBEntities.Entry(UserTestDetail).State = System.Data.Entity.EntityState.Modified;

                    DBEntities.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public string GetUserTestStatus(int TestId)
        {
            try
            {
                string UserTestStatus = DBEntities.txnUserTestDetails.Where(x => x.TestId == TestId).Select(x => x.status).FirstOrDefault();

                return UserTestStatus;
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        public int GetTypeId(int TestId, int setId)
        {
            try
            {

                var TypeId = (from txnQue in DBEntities.txnQuestions
                              join mQue in DBEntities.mstQuestions
                              on txnQue.QuestionId equals mQue.QuestionId
                              where txnQue.TestId == TestId && mQue.SetId == setId
                              select mQue.TypeId).FirstOrDefault();
                //var TypeId = DBEntities.txnQuestions.GroupJoin(DBEntities.mstQuestions, r => r.QuestionId, p => p.QuestionId, (r, p) => new { r.mstQuestion.TypeId });
                return (int)TypeId;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public List<QuestionSubType> lstSubType(int TypeId)
        {
            try
            {
                List<QuestionSubType> lstSubType = DBEntities.mstQuestionSubTypes.Where(x => x.TypeId == TypeId).Select(x => new QuestionSubType
                {
                    SubTypeId = x.SubTypeId,
                    SubTypeName = x.SubTypeName,
                    TypeId = x.TypeId.Value
                }).ToList();

                return lstSubType;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public List<ClsSet6ScoreModel> lstSubModuleScore(int Testid, int setid)
        {
            try
            {
                List<ClsSet6ScoreModel> lstSubModuleScore = (from i in DBEntities.txnQuesResponseWithSubModules
                                                             join j in DBEntities.mstSubModules
                                                             on i.SubModuleId equals j.SubModuleId
                                                             where i.TestId == Testid && i.SetId == setid
                                                             select new ClsSet6ScoreModel
                                                             {
                                                                 SubModuleId = j.SubModuleId,
                                                                 SubModuleName = j.SubModuleName,
                                                                 PresenceScore = i.calcuation.Value,
                                                                 PersonalityScore = i.AvgOfScore.Value
                                                             }).ToList();

                return lstSubModuleScore;
            }
            catch (Exception ex)
            {
                throw;
            }

        }


        public Tuple<List<Question>, int> GetAllforSet6Question(int TestId, int setId)
        {
            try
            {
                int SubModuleId = 0;

                var SubModuleData = (from txnQues in DBEntities.txnQuestions
                                     join mstQues in DBEntities.mstQuestions
                                     on txnQues.QuestionId equals mstQues.QuestionId
                                     where txnQues.TestId == TestId && txnQues.Setid == setId && txnQues.IsAnswer == null
                                     group mstQues by mstQues.SubModuleId into subModuleid
                                     select new
                                     {
                                         SubModuleId = (int)subModuleid.Key
                                     }).ToList();

                SubModuleId = SubModuleData.Select(x => x.SubModuleId).FirstOrDefault();

                List<Question> Question = (from txnQues in DBEntities.txnQuestions
                                           join mstQues in DBEntities.mstQuestions
                                           on txnQues.QuestionId equals mstQues.QuestionId
                                           where txnQues.TestId == TestId && txnQues.Setid == setId && mstQues.SubModuleId == SubModuleId
                                           select new Question
                                           {
                                               QuestionId = (int)txnQues.QuestionId,
                                               isAnswer = txnQues.IsAnswer.Value
                                           }).ToList();
                ;
                return new Tuple<List<Question>, int>(Question, SubModuleId);
            }
            catch (Exception ex)
            {
                throw;
            }

        }




        public void CalucationOfQuestionSet6(int TestId, int SetId, List<ClsQuestion> lstQuestionModel, int SubModuleId)
        {
            try
            {
                float AvgOfScore;
                int FinalAvgOfScore = 0, CalcuationOfScore = 0;
                List<int> lstRating = new List<int>();
                DateTime DateTime;
                if (SubModuleId == (int)enumQuestionSet6Module.Lines || SubModuleId == (int)enumQuestionSet6Module.Fixations)
                {
                    foreach (var QuestionRes in lstQuestionModel)
                    {
                        txnQuestion Question = DBEntities.txnQuestions.Where(x => x.QuestionId == QuestionRes.QuestionId
                                                                                  && x.TestId == QuestionRes.TestId
                                                                                  && x.TestQuestionId == QuestionRes.TestQuestionId).FirstOrDefault();

                        int? Score = DBEntities.mstScoreModules.Where(x => x.SubModuleId == SubModuleId && x.Rating == QuestionRes.Rating).Select(x => x.Percentage).FirstOrDefault();

                        txnUserTestDetail UserDetails = DBEntities.txnUserTestDetails.Where(x => x.TestId == QuestionRes.TestId.Value).FirstOrDefault();

                        if (Question != null)
                        {

                            DateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
                            Question.ImpactScore = Score.HasValue ? Score.Value : 0;
                            Question.ResponseAt = DateTime;//DateTime.UtcNow;
                            Question.IsAnswer = true;
                            Question.ResponseBy = UserDetails.UserId;
                            DBEntities.Entry(Question).State = System.Data.Entity.EntityState.Modified;
                        }
                        DBEntities.SaveChanges();
                    }
                }


                var TrxQuestionData = (from txnq in DBEntities.txnQuestions
                                       join mstq in DBEntities.mstQuestions
                                       on txnq.QuestionId equals mstq.QuestionId
                                       where txnq.TestId == TestId && txnq.Setid == SetId && mstq.SubModuleId == SubModuleId
                                       select new
                                       {
                                           QuestionId = txnq.QuestionId,
                                           Question = mstq.Question,
                                           ImpactScore = txnq.ImpactScore,
                                           SetId = txnq.Setid
                                       }).ToList();

                var SumOfScore = (from QuestionData in TrxQuestionData
                                  group QuestionData by QuestionData.SetId into QuestionGroup
                                  select new
                                  {
                                      SetId = QuestionGroup.Key,
                                      TotalScore = QuestionGroup.Sum(x => x.ImpactScore),
                                  }).ToList();

                if (SumOfScore != null)
                {
                    if (SubModuleId == (int)enumQuestionSet6Module.Blindspot)
                    {


                        FinalAvgOfScore = (int)(0.5f + ((100f * SumOfScore.ElementAt(0).TotalScore.Value) / 27));
                       // AvgOfScore = SumOfScore.ElementAt(0).TotalScore.Value > 0 ? (float)SumOfScore.ElementAt(0).TotalScore / 30 : 0;
                       // AvgOfScore = AvgOfScore != 0 ? AvgOfScore * 100 : 0;
                      //  FinalAvgOfScore = (int)(AvgOfScore + 0.5f);

                        CalcuationOfScore = FinalAvgOfScore > 0 ? 100 - FinalAvgOfScore : 0;
                    }
                    else if (SubModuleId == (int)enumQuestionSet6Module.Fixations)
                    {
                        int FixCalcuationOfScore = (int)(0.5f + ((100f * SumOfScore.ElementAt(0).TotalScore.Value) / 3));
                        int FixFinalAvgOfScore = CalcuationOfScore > 0 ? 100 - CalcuationOfScore : 0;

                        float TempFixScore = SumOfScore.ElementAt(0).TotalScore.Value > 0 ? (float)SumOfScore.ElementAt(0).TotalScore / 3 : 0;
                        CalcuationOfScore = (int)(TempFixScore + 0.5f);

                        FinalAvgOfScore = CalcuationOfScore > 0 ? 100 - CalcuationOfScore : 0;
                    }
                    else if (SubModuleId == (int)enumQuestionSet6Module.Lines)
                    {
                       //   int  LineCalcuationOfScore = (int)(0.5f + ((100f * SumOfScore.ElementAt(0).TotalScore.Value) / 4));
                       // int LineFinalAvgOfScore = CalcuationOfScore > 0 ? 100 - CalcuationOfScore : 0;

                        float TempLineScore = SumOfScore.ElementAt(0).TotalScore.Value > 0 ? (float)SumOfScore.ElementAt(0).TotalScore / 4 : 0;
                        CalcuationOfScore = (int)(TempLineScore + 0.5f);

                        FinalAvgOfScore = CalcuationOfScore > 0 ? 100 - CalcuationOfScore : 0;
                    }
                }
                txnQuesResponseWithSubModule objQuestionWithSubModule = new txnQuesResponseWithSubModule();
                objQuestionWithSubModule.TestId = TestId;
                objQuestionWithSubModule.SubModuleId = SubModuleId;
                objQuestionWithSubModule.SumOfScore = SumOfScore.ElementAt(0).TotalScore.Value > 0 ? SumOfScore.ElementAt(0).TotalScore.Value : 0;
                objQuestionWithSubModule.AvgOfScore = FinalAvgOfScore;
                objQuestionWithSubModule.calcuation = CalcuationOfScore;
                objQuestionWithSubModule.SetId = (int)enumQuestionSet.Set6;
                int UserId = Convert.ToInt32(DBEntities.txnUserTestDetails.Where(x => x.TestId == TestId).Select(x => x.UserId).FirstOrDefault());
                DateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
                objQuestionWithSubModule.CreatedAt = DateTime; //DateTime.UtcNow;
                objQuestionWithSubModule.CreatedBy = UserId;
                objQuestionWithSubModule.LastModifiedAt = DateTime; //DateTime.UtcNow;
                objQuestionWithSubModule.LastModifiedBy = UserId;

                DBEntities.txnQuesResponseWithSubModules.Add(objQuestionWithSubModule);

                DBEntities.SaveChanges();
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        public Tuple<int, int> GetNoOfQuestionComplete(int TestId)
        {
            try
            {
                int TotalQuestion = 166, CompleteQuestion = 0;

                CompleteQuestion = DBEntities.txnQuestions.Where(x => x.TestId == TestId && x.IsAnswer == true).Count();


                CompleteQuestion = (int)(0.5f + ((100f * CompleteQuestion) / TotalQuestion));

                if (CompleteQuestion > 100)
                {
                    CompleteQuestion = 100;
                }
                return new Tuple<int, int>(TotalQuestion, CompleteQuestion);
            }
            catch (Exception ex)
            {
                throw;
            }

        }


        public string GetAttachmentNameForSend(int TestId)
        {
            try
            {
                string FileName = string.Empty;

                List<ClsTypeModel> lstSubTypeWiseScoreCard = new List<ClsTypeModel>();
                lstSubTypeWiseScoreCard = DBEntities.usp_SubTypeWiseScoreBoard(TestId, 2).Select(x => new ClsTypeModel
                {
                    TypeId = x.SubTypeId.HasValue ? x.SubTypeId.Value : 0,
                    TypeName = x.SubTypeName,
                    Score = x.ImpactScore.HasValue ? x.ImpactScore.Value : 0
                }).ToList();

                int TopScoreType = lstSubTypeWiseScoreCard.OrderByDescending(x => x.Score).Select(x => x.TypeId).FirstOrDefault();

                lstSubTypeWiseScoreCard = new List<ClsTypeModel>();
                lstSubTypeWiseScoreCard = DBEntities.usp_SubTypeWiseScoreBoard(TestId, 5).Select(x => new ClsTypeModel
                {
                    TypeId = x.SubTypeId.HasValue ? x.SubTypeId.Value : 0,
                    TypeName = x.SubTypeName,
                    Score = x.ImpactScore.HasValue ? x.ImpactScore.Value : 0
                }).OrderByDescending(x => x.Score).ToList();
                string SubTypeName = string.Empty;
                if (lstSubTypeWiseScoreCard.ElementAt(0).Score == lstSubTypeWiseScoreCard.ElementAt(1).Score)
                {

                    if (lstSubTypeWiseScoreCard.ElementAt(0).TypeName.ToUpper() == "SP" && lstSubTypeWiseScoreCard.ElementAt(1).TypeName.ToUpper() == "1-O-1")
                    {
                        SubTypeName = "1O1";
                    }
                    else if (lstSubTypeWiseScoreCard.ElementAt(0).TypeName.ToUpper() == "SO" && lstSubTypeWiseScoreCard.ElementAt(1).TypeName.ToUpper() == "1-O-1")
                    {
                        SubTypeName = "1O1";
                    }
                    else if (lstSubTypeWiseScoreCard.ElementAt(0).TypeName.ToUpper() == "SP" && lstSubTypeWiseScoreCard.ElementAt(1).TypeName.ToUpper() == "SO")
                    {
                        SubTypeName = "SO";
                    }

                    FileName = TopScoreType.ToString() + "-" + SubTypeName + "." + "pdf";

                }
                else
                {
                    SubTypeName = lstSubTypeWiseScoreCard.ElementAt(0).TypeName.ToUpper() == "1-O-1" ? "1O1" : lstSubTypeWiseScoreCard.ElementAt(0).TypeName.ToUpper();

                    FileName = TopScoreType.ToString() + "-" + SubTypeName + "." + "pdf";
                }

                return FileName;
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        public string GetExamStatus(int TestId, int SetId)
        {
            try
            {
                string ExamStatus = DBEntities.txnExamSetStatus.Where(x => x.TestId == TestId && x.SetId == SetId).Select(x => x.Status).FirstOrDefault();

                return ExamStatus;
            }
            catch (Exception ex)
            {
                throw;
            }
        }




        public Tuple<string, int> GetCalutaionScoreAndColurCode(int ModuleId, int TypeId, int Score, string TypeName)
        {
            try
            {
                string ColourCode = string.Empty;
                int FinalCalcuationOfScore = 0;
                switch (ModuleId)
                {
                    case (int)enumQuestionSet.Set1:
                        FinalCalcuationOfScore = (int)(0.5f + ((100f * Score) / 120));//(int)Math.Round((double)(100 * Score) / 120);
                        switch (TypeId)
                        {
                            case (int)enumQuestionSet1Type.EthicalPerfectionistEnneagramType:
                                ColourCode = "#F1A21F";
                                break;
                            case (int)enumQuestionSet1Type.EmpathicNurturerEnneagramType:
                                ColourCode = "#BECF31";
                                break;
                            case (int)enumQuestionSet1Type.AmbitiousAchieverEnneagramType:
                                ColourCode = "#748838";
                                break;
                            case (int)enumQuestionSet1Type.ExpressiveIndividualistEnneagramType:
                                ColourCode = "#108F53";
                                break;
                            case (int)enumQuestionSet1Type.PerceptiveSpecialistEnneagramType:
                                ColourCode = "#779FCA";
                                break;
                            case (int)enumQuestionSet1Type.HardworkingLoyalistEnneagramType:
                                ColourCode = "#1C6495";
                                break;
                            case (int)enumQuestionSet1Type.VersatileVisionaryEnneagramType:
                                ColourCode = "#70659D";
                                break;
                            case (int)enumQuestionSet1Type.CharismaticControllerEnneagramType:
                                ColourCode = "#CE2B2B";
                                break;
                            case (int)enumQuestionSet1Type.AccommodatingPeacemakerEnneagramType:
                                ColourCode = "#96422B";
                                break;
                        }
                        break;
                    case (int)enumQuestionSet.Set2:
                        switch (TypeId)
                        {
                            case (int)enumQuestionSet1Type.EthicalPerfectionistEnneagramType:
                                ColourCode = "#F1A21F";
                                break;
                            case (int)enumQuestionSet1Type.EmpathicNurturerEnneagramType:
                                ColourCode = "#BECF31";
                                break;
                            case (int)enumQuestionSet1Type.AmbitiousAchieverEnneagramType:
                                ColourCode = "#748838";
                                break;
                            case (int)enumQuestionSet1Type.ExpressiveIndividualistEnneagramType:
                                ColourCode = "#108F53";
                                break;
                            case (int)enumQuestionSet1Type.PerceptiveSpecialistEnneagramType:
                                ColourCode = "#779FCA";
                                break;
                            case (int)enumQuestionSet1Type.HardworkingLoyalistEnneagramType:
                                ColourCode = "#1C6495";
                                break;
                            case (int)enumQuestionSet1Type.VersatileVisionaryEnneagramType:
                                ColourCode = "#70659D";
                                break;
                            case (int)enumQuestionSet1Type.CharismaticControllerEnneagramType:
                                ColourCode = "#CE2B2B";
                                break;
                            case (int)enumQuestionSet1Type.AccommodatingPeacemakerEnneagramType:
                                ColourCode = "#96422B";
                                break;
                        }
                        break;
                    case (int)enumQuestionSet.Set3:
                        FinalCalcuationOfScore = (int)(0.5f + ((100f * Score) / 27));//(int)Math.Round((double)(100 * Score) / 120);
                        switch (TypeId)
                        {
                            case (int)enumQuestionSet3Type.ActionCentre:
                                ColourCode = "#BA392E";
                                break;
                            case (int)enumQuestionSet3Type.FeelingCentre:
                                ColourCode = "#A8B935";
                                break;
                            case (int)enumQuestionSet3Type.ThinkingCentre:
                                ColourCode = "#5582B4";
                                break;
                        }
                        break;
                    case (int)enumQuestionSet.Set4:

                        switch (TypeId)
                        {
                            case (int)enumQuestionSet4Type.EnvironmentalStressResponse:
                                FinalCalcuationOfScore = (int)(0.5f + ((100f * Score) / 40));
                                FinalCalcuationOfScore = 100 - FinalCalcuationOfScore ;
                                ColourCode = FinalCalcuationOfScore > 70 ? ColourCode = "#BA3930" : ((FinalCalcuationOfScore > 31) && (FinalCalcuationOfScore < 69)) ? ColourCode = "#F89D52" : FinalCalcuationOfScore < 30 ? ColourCode = "#ACB9CA" : string.Empty;
                                break;
                            case (int)enumQuestionSet4Type.PhysicalStressResponse:
                                FinalCalcuationOfScore = (int)(0.5f + ((100f * Score) / 40));
                                FinalCalcuationOfScore = 100 - FinalCalcuationOfScore;
                                ColourCode = FinalCalcuationOfScore > 70 ? ColourCode = "#BA3930" : ((FinalCalcuationOfScore > 31) && (FinalCalcuationOfScore < 69)) ? ColourCode = "#F89D52" : FinalCalcuationOfScore < 30 ? ColourCode = "#ACB9CA" : string.Empty;
                                break;
                            case (int)enumQuestionSet4Type.InterpersonalStressResponse:
                                FinalCalcuationOfScore = (int)(0.5f + ((100f * Score) / 40));
                                FinalCalcuationOfScore = 100 - FinalCalcuationOfScore;
                                ColourCode = FinalCalcuationOfScore > 70 ? ColourCode = "#BA3930" : ((FinalCalcuationOfScore > 31) && (FinalCalcuationOfScore < 69)) ? ColourCode = "#F89D52" : FinalCalcuationOfScore < 30 ? ColourCode = "#ACB9CA" : string.Empty;
                                break;
                            case (int)enumQuestionSet4Type.PsychologicalStressResponse:
                                FinalCalcuationOfScore = (int)(0.5f + ((100f * Score) / 40));
                                FinalCalcuationOfScore = 100 - FinalCalcuationOfScore;
                                ColourCode = FinalCalcuationOfScore > 70 ? ColourCode = "#BA3930" : ((FinalCalcuationOfScore > 31) && (FinalCalcuationOfScore < 69)) ? ColourCode = "#F89D52" : FinalCalcuationOfScore < 30 ? ColourCode = "#ACB9CA" : string.Empty;
                                break;
                            case (int)enumQuestionSet4Type.RelationshipStressResponse:
                                FinalCalcuationOfScore = (int)(0.5f + ((100f * Score) / 40));
                                FinalCalcuationOfScore = 100 - FinalCalcuationOfScore;
                                ColourCode = FinalCalcuationOfScore > 70 ? ColourCode = "#BA3930" : ((FinalCalcuationOfScore > 31) && (FinalCalcuationOfScore < 69)) ? ColourCode = "#F89D52" : FinalCalcuationOfScore < 30 ? ColourCode = "#ACB9CA" : string.Empty;
                                break;
                            case (int)enumQuestionSet4Type.ProfessionalStressResponse:
                                FinalCalcuationOfScore = (int)(0.5f + ((100f * Score) / 40));
                                FinalCalcuationOfScore = 100 - FinalCalcuationOfScore;
                                ColourCode = FinalCalcuationOfScore > 70 ? ColourCode = "#BA3930" : ((FinalCalcuationOfScore > 31) && (FinalCalcuationOfScore < 69)) ? ColourCode = "#F89D52" : FinalCalcuationOfScore < 30 ? ColourCode = "#ACB9CA" : string.Empty;
                                break;
                            case (int)enumQuestionSet4Type.Optimism:
                                FinalCalcuationOfScore = (int)(0.5f + ((100f * Score) / 60));
                                FinalCalcuationOfScore = 100 - FinalCalcuationOfScore;
                                if (FinalCalcuationOfScore > 70)
                                {
                                    ColourCode = "#00712E";
                                }
                                else if ((FinalCalcuationOfScore > 31) && (FinalCalcuationOfScore < 70))
                                {
                                    ColourCode = "#F89D52";
                                }
                                else if (FinalCalcuationOfScore < 31)
                                {
                                    ColourCode = "#ACB9CA";
                                }
                                break;
                        }
                        break;
                    case (int)enumQuestionSet.Set5:
                        FinalCalcuationOfScore = (int)(0.5f + ((100f * Score) / 15));
                        switch (TypeName)
                        {
                            case "SP":
                                ColourCode = "#D14F47";
                                break;
                            case "1-O-1":
                                ColourCode = "#D46B4E";
                                break;
                            case "SO":
                                ColourCode = "#C19F9D";
                                break;
                        }
                        break;
                }



                return new Tuple<string, int>(ColourCode, FinalCalcuationOfScore);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }








    }
}