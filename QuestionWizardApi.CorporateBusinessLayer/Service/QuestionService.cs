using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QuestionWizardApi.CorporateData;
using QuestionWizardApi.CorporateModel.Model;
using QuestionWizardApi.CorporateIBusinessLayer.Interface;
using System.IO;
using Amazon.S3;
using Amazon.Runtime;
using Amazon.S3.Model;
using Newtonsoft.Json;
using iTextSharp.text.pdf.qrcode;

namespace QuestionWizardApi.CorporateBusinessLayer.Service
{
    public class QuestionService : IDisposable, IQuestion
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        CorporateAssessmentEntities DBEntities = new CorporateAssessmentEntities();
        UserService UserSvc = new UserService();
        MailService MailSvc = new MailService();

        int[] ArrSubSet1 =
                   {
                        (int)MainType.EmpathicNurturerEnneagramType2,
                        (int)MainType.AmbitiousAchieverEnneagramType3,
                        (int)MainType.IntenseIndividualistEnneagramType4
                    };
        int[] ArrSubSet2 =
        {
                        (int)MainType.PerceptiveSpecialistEnneagramType5,
                        (int)MainType.DutifulLoyalistEnneagramType6,
                        (int)MainType.VersatileVisionaryEnneagramType7
                    };
        int[] ArrSubSet3 =
        {
                        (int)MainType.CharismaticControllerEnneagramType8,
                        (int)MainType.ReceptivePeacemakerEnneagramType9,
                        (int)MainType.EthicalPerfectionistEnneagramType1
                    };


        ~QuestionService()
        {
            Dispose(false);
        }


        public async Task<ClsQuestionModel> LoadQuestionModel(int TestId, int SetId, int? QuestionId)
        {
            ClsQuestionModel QuestionModel = new ClsQuestionModel();
            List<Question> Question = new List<Question>();
            List<int> DisplayQuestionId = new List<int>();
            //List<int> NotCompleteQuestionId = new List<int>();
            try
            {
                CandidateBM UserModel = UserSvc.GetCandidateData(TestId);

                QuestionModel.CurrentUserId = UserModel.UserId;
                QuestionModel.TestId = TestId;
                SetId = UserSvc.GetLatestSetId(QuestionModel.TestId);

                QuestionModel.CurrentSetId = SetId;
                QuestionModel.CurrentSetName = GetSetName(QuestionModel.CurrentSetId);
                QuestionModel.CompanyId = UserModel.CompanyId;
                QuestionModel.ModuleId = UserModel.ModuleId;

                QuestionModel.CurrentSetStatus = GetCurrentExamStatus(TestId, SetId);
                QuestionModel.IsTestComplete = GetUserTestStatus(TestId) == "C" ? true : false;
                QuestionModel.IsAssessmentError = GetUserTestStatus(TestId) == "E" ? true : false;

                bool? IsDesktop = DBEntities.txnCandidates.Where(x => x.UserId == UserModel.UserId).Select(x => x.IsDesktopDevice).FirstOrDefault();

                if (IsDesktop == null)
                {
                    QuestionModel.IsDesktop = false;

                }
                else
                {
                    if (IsDesktop.Value)
                    {
                        QuestionModel.IsDesktop = IsDesktop.Value;
                    }
                    else
                    {
                        QuestionModel.IsDesktop = false;
                    }

                }

                if (QuestionModel.IsAssessmentError)
                {
                    QuestionModel.IsScordBoardDisplay = false;
                    QuestionModel.IsQuestionDisplay = false;
                }
                else if (QuestionModel.IsTestComplete)
                {
                    QuestionModel.IsScordBoardDisplay = false;
                    QuestionModel.IsQuestionDisplay = false;
                }
                else if (QuestionModel.CurrentSetStatus == "C")
                {
                    QuestionModel.IsScordBoardDisplay = true;
                    QuestionModel.IsQuestionDisplay = false;

                    GetQuestionScoreCard(ref QuestionModel);
                }
                else if (QuestionModel.CurrentSetStatus == "P")
                {
                    QuestionModel.IsScordBoardDisplay = false;
                    QuestionModel.IsQuestionDisplay = true;

                    Question = GetAllQuestion(TestId, SetId);

                    var NotCompleteQuestionId = Question.Where(x => x.isAnswer == null).Select(x => x.QuestionId).ToList();
                    var CompletedQuestionId = Question.Where(x => x.isAnswer != null).Select(x => x.QuestionId).ToList();

                    QuestionModel.TotalQuestion = Question.Count;
                    QuestionModel.CompletedQuestion = CompletedQuestionId.Count;
                    if (QuestionModel.CurrentSetId == (int)AssessmentModule.H1PartAAptitude)
                    {
                        QuestionModel.lstQuestionIdStatus = GetQuestionNumberAndStatus(QuestionModel.TestId, (int)AssessmentModule.H1PartAAptitude);
                    }

                    var ExamSetStatus = DBEntities.txnExamSetStatus.Where(x => x.TestId == TestId && x.ModuleId == QuestionModel.CurrentSetId).FirstOrDefault();
                    if (ExamSetStatus.IsDisplayInstruction == null)
                    {
                        QuestionModel.IsDisplayInstruction = true;
                        //QuestionModel.InstructionText = DBEntities.mstInstructions.Where(x => x.SetId == QuestionModel.CurrentSetId).Select(x => x.InstructionText).FirstOrDefault();
                    }
                    else
                    {
                        QuestionModel.IsDisplayInstruction = false;
                        QuestionModel.InstructionText = "";
                    }

                    var H1PartAData = GetQuestionSeriesData(TestId, QuestionModel.CurrentSetId, Question, QuestionId);

                    if (ExamSetStatus.IsDisplayInstruction == null)
                    {
                        ExamSetStatus.IsDisplayInstruction = true;
                    }
                    else { ExamSetStatus.IsDisplayInstruction = false; }

                    DBEntities.Entry(ExamSetStatus).State = System.Data.Entity.EntityState.Modified;
                    DBEntities.SaveChanges();

                    QuestionModel.IsShowNextButton = H1PartAData.Item2;
                    QuestionModel.IsShowPrevButton = H1PartAData.Item3;
                    QuestionModel.IsShowSubmitButton = H1PartAData.Item4;
                    QuestionModel.NextQuestion = H1PartAData.Item5;
                    QuestionModel.PrevQuestion = H1PartAData.Item6;

                    DisplayQuestionId = H1PartAData.Item1;


                    var TaskQuestionData = Task.Run(() => GetAllQuestionDetails(DisplayQuestionId, TestId, SetId));
                    QuestionModel.lstQuestionModel = await TaskQuestionData;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return QuestionModel;
        }

        public Tuple<int, string> GetNextModuleDetails(int TestId, int ModuleId, int AssessmentId)
        {
            List<string> Status = new List<string>()
            {
                "C","P"
            };
            var ExamSetStatus = DBEntities.txnExamSetStatus.Where(x => x.TestId == TestId && x.ModuleId != ModuleId && !Status.Contains(x.Status) && x.Status == "NS").ToList();

            int SetId = 0;
            string SetName = string.Empty;

            List<int> ModuleIds = ExamSetStatus.Select(x => x.ModuleId.Value).ToList();

            if (ExamSetStatus != null && ExamSetStatus.Count != 0)
            {
                var AssessmentModuleDetails = DBEntities.mstAssessmentModules.Where(x => ModuleIds.Contains(x.ModuleId)).OrderBy(x => x.ModuleOrder).FirstOrDefault();
                SetId = AssessmentModuleDetails.ModuleId;
                SetName = AssessmentModuleDetails.ModuleName;
            }

            return new Tuple<int, string>(SetId, SetName);
        }

        public void GetQuestionScoreCard(ref ClsQuestionModel ObjQuestionModel)
        {
            try
            {
                int TestId = ObjQuestionModel.TestId;
                int Count = DBEntities.txnExamSetStatus.Where(x => x.TestId == TestId).Count();
                var AssessmentModuleDetails = GetNextModuleDetails(TestId, ObjQuestionModel.CurrentSetId, ObjQuestionModel.ModuleId);
                ObjQuestionModel.CompentencyScoreCard = new CompentencyScoreCard();
                ObjQuestionModel.CompentencyScoreCard.MultiBarScoreCard = new List<string>();

                switch (ObjQuestionModel.CurrentSetId)
                {
                    case (int)AssessmentModule.H1PartAMainType:
                    case (int)AssessmentModule.MainType:
                    case (int)AssessmentModule.QLEAPAndQMAPMainType:
                        {

                            ObjQuestionModel.ScoreBoard = GetTypeWiseScoreCard(ObjQuestionModel.TestId, ObjQuestionModel.CurrentSetId);


                            if (!DBEntities.txnDynamicMisTypings.Any(x => x.TestId == TestId))
                            {
                                GenerateDynamicMistyping(ObjQuestionModel.ScoreBoard, ObjQuestionModel.TestId, ObjQuestionModel.CurrentUserId, ObjQuestionModel.ModuleId, ObjQuestionModel.CurrentSetId);
                            }

                            ObjQuestionModel.ScoreBoard = GetTypeWiseScoreBoard(ObjQuestionModel.TestId, ObjQuestionModel.CurrentSetId);


                            if (DBEntities.txnUserTestDetails.Any(x => x.TestId == TestId && x.status == "E"))
                            {
                                ObjQuestionModel.IsShowGoToNextSetButton = false;
                                ObjQuestionModel.IsScordBoardDisplay = false;
                                ObjQuestionModel.IsQuestionDisplay = false;
                                ObjQuestionModel.IsTestComplete = false;
                                ObjQuestionModel.IsAssessmentError = true;
                                Task.Run(async () => await MailSvc.SentFInalWhenError(TestId));
                            }
                            else
                            {
                                ObjQuestionModel.NextSetId = AssessmentModuleDetails.Item1;
                                ObjQuestionModel.NextSetName = AssessmentModuleDetails.Item2;

                                txnDynamicMisTyping MainDyamicMisTyping = DBEntities.txnDynamicMisTypings.Where(x => x.TestId == TestId).OrderBy(x => x.MisTypingId).FirstOrDefault();
                                MainDyamicMisTyping.status = "P";
                                DBEntities.Entry(MainDyamicMisTyping).State = System.Data.Entity.EntityState.Modified;
                                DBEntities.SaveChanges();

                                ObjQuestionModel.NextTypeName = DBEntities.txnDynamicMisTypings.Where(x => x.TestId == TestId && x.status == "P").OrderBy(x => x.MisTypingId).Select(x => x.MisTypeName).FirstOrDefault();
                                ObjQuestionModel.NextTypeId = DBEntities.txnDynamicMisTypings.Where(x => x.TestId == TestId && x.status == "P").OrderBy(x => x.MisTypingId).Select(x => x.MisTypeId).FirstOrDefault();
                                ObjQuestionModel.IsShowGoToNextSetButton = true;
                            }

                        }
                        break;
                    case (int)AssessmentModule.H1PartAMistyping:
                    case (int)AssessmentModule.Mistyping:
                        {

                            int MainTypeId = DBEntities.txnDynamicMisTypings.Where(x => x.TestId == TestId && x.Type == (int)enumType.MainType && x.status == "C" && x.HighestType != null).OrderByDescending(x => x.MisTypingId).Select(x => x.MisTypeId.Value).FirstOrDefault();

                            ObjQuestionModel.ScoreBoard = GetSubTypeWiseScoreBoard(ObjQuestionModel.TestId, ObjQuestionModel.CurrentSetId, MainTypeId);


                            //ObjQuestionModel.ScoreBoard = GetTypeWiseScoreBoard(ObjQuestionModel.TestId, ObjQuestionModel.CurrentSetId);

                            //  int TestId = ObjQuestionModel.TestId;
                            //int MainTypeId = DBEntities.txnDynamicMisTypings.Where(x => x.TestId == TestId && x.Type == (int)enumType.MainType && x.status == "C" && x.HighestType != null).OrderByDescending(x => x.MisTypingId).Select(x => x.MisTypeId.Value).FirstOrDefault();

                            //   ObjQuestionModel.ScoreBoard = GetSubTypeWiseScoreBoard(ObjQuestionModel.TestId, ObjQuestionModel.CurrentSetId, MainTypeId);

                            ObjQuestionModel.NextSetId = AssessmentModuleDetails.Item1;
                            ObjQuestionModel.NextSetName = AssessmentModuleDetails.Item2;
                            if (AssessmentModuleDetails.Item1 == (int)AssessmentModule.EnneagramInstincts)
                            {
                                var MainType = DBEntities.txnDynamicMisTypings.Where(x => x.TestId == TestId && x.Type == 1).OrderByDescending(i => i.MisTypingId).Select(x => x.HighestType).FirstOrDefault();

                                ObjQuestionModel.NextTypeName = "Type" + MainType;
                                ObjQuestionModel.NextTypeId = GetQuestionTypeId(ObjQuestionModel.NextTypeName);
                            }
                            if (ObjQuestionModel.ModuleId == 5)
                            {
                                ObjQuestionModel.IsShowGoToNextSetButton = false;
                            }
                            else
                            {
                                ObjQuestionModel.IsShowGoToNextSetButton = true;
                            }

                        }
                        break;
                    case (int)AssessmentModule.H1PartAAptitude:
                        {
                            ObjQuestionModel.ScoreBoard = new List<ClsTypeModel>();

                            var ScoreCard = GetTypeWiseScoreBoard(ObjQuestionModel.TestId, ObjQuestionModel.CurrentSetId);
                            ObjQuestionModel.ScoreBoard.AddRange(ScoreCard.Select(x => new ClsTypeModel
                            {
                                TypeId = x.TypeId,
                                TypeName = x.TypeName,
                                Score = CalculatedApptitudeScore(x.Score),
                                ColorCode = GetColorCode(null, CalculatedApptitudeScore(x.Score))
                            }).ToList());
                            //  int TestId = ObjQuestionModel.TestId;
                            //   int MainTypeId = DBEntities.txnDynamicMisTypings.Where(x => x.TestId == TestId && x.Type == (int)enumType.MainType && x.status == "C" && x.HighestType != null).OrderByDescending(x => x.MisTypingId).Select(x => x.MisTypeId.Value).FirstOrDefault();

                            //     ObjQuestionModel.ScoreBoard = GetSubTypeWiseScoreBoard(ObjQuestionModel.TestId, ObjQuestionModel.CurrentSetId, MainTypeId);

                            ObjQuestionModel.NextSetId = AssessmentModuleDetails.Item1;
                            ObjQuestionModel.NextSetName = AssessmentModuleDetails.Item2;


                            ObjQuestionModel.IsShowGoToNextSetButton = false;
                        }
                        break;

                    case (int)AssessmentModule.H1PartACompetency:
                        {

                            ObjQuestionModel.ScoreBoard = new List<ClsTypeModel>();
                            int SetId = ObjQuestionModel.CurrentSetId;
                            var Set11ScoreCard = GetTypeWiseScoreBoard(ObjQuestionModel.TestId, ObjQuestionModel.CurrentSetId);

                            ObjQuestionModel.ScoreBoard.AddRange(Set11ScoreCard.Where(x => x.TypeId != 62).Select(x => new ClsTypeModel
                            {
                                TypeId = x.TypeId,
                                TypeName = x.TypeName,
                                Score = GetCompetenciesCalculation(x.TypeId, x.Score, TestId, 4), //QueSrv.CalculationOfSet1(x.Score),
                                ColorCode = GetColorCode(null, GetCompetenciesCalculation(x.TypeId, x.Score, TestId, 4))
                            }).ToList());

                            if (ObjQuestionModel.ModuleId == (int)enumModule.H1PartA)
                            {
                                ObjQuestionModel.IsShowGoToNextSetButton = true;

                                ObjQuestionModel.NextSetId = AssessmentModuleDetails.Item1;
                                ObjQuestionModel.NextSetName = AssessmentModuleDetails.Item2;
                            }
                            else
                            {
                                ObjQuestionModel.IsShowGoToNextSetButton = false;
                            }

                        }
                        break;
                    case (int)AssessmentModule.EnneagramInstincts:
                        {
                            ObjQuestionModel.ScoreBoard = GetSubTypeWiseScoreBoard(ObjQuestionModel.TestId, ObjQuestionModel.CurrentSetId, null);


                            ObjQuestionModel.NextSetId = AssessmentModuleDetails.Item1;
                            ObjQuestionModel.NextSetName = AssessmentModuleDetails.Item2;

                            var MainType = DBEntities.txnDynamicMisTypings.Where(x => x.TestId == TestId && x.Type == 1).OrderByDescending(i => i.MisTypingId).Select(x => x.HighestType).FirstOrDefault();

                            //    ObjQuestionModel.NextTypeName = "Type" + MainType;
                            ObjQuestionModel.NextTypeId = MainType;

                            ObjQuestionModel.IsShowGoToNextSetButton = true;
                        }
                        break;
                    case (int)AssessmentModule.CenterOfExpression:
                        {
                            ObjQuestionModel.ScoreBoard = GetSubTypeWiseScoreBoard(ObjQuestionModel.TestId, ObjQuestionModel.CurrentSetId, null);


                            ObjQuestionModel.NextSetId = AssessmentModuleDetails.Item1;
                            ObjQuestionModel.NextSetName = AssessmentModuleDetails.Item2;

                            ObjQuestionModel.IsShowGoToNextSetButton = true;
                        }
                        break;
                    case (int)AssessmentModule.PersonalityToPresence:
                        {
                            ObjQuestionModel.ScoreBoard = new List<ClsTypeModel>();
                            var ScoreCardOfSet8 = GetTypeWiseScoreCard(ObjQuestionModel.TestId, ObjQuestionModel.CurrentSetId);

                            ObjQuestionModel.ScoreBoard.AddRange(ScoreCardOfSet8.Where(x => x.TypeId != 62).Select(x => new ClsTypeModel
                            {
                                TypeId = x.TypeId,
                                TypeName = x.TypeName.Contains("Fixations") ? "Fixations" : "Blindspot",
                                Score = CalcuatePersonlityToPersenceScore(x.Score)
                            }).ToList());


                            ObjQuestionModel.NextSetId = AssessmentModuleDetails.Item1;
                            ObjQuestionModel.NextSetName = AssessmentModuleDetails.Item2;

                            ObjQuestionModel.IsShowGoToNextSetButton = true;
                        }
                        break;
                    case (int)AssessmentModule.StressAndResilience:
                        {
                            ObjQuestionModel.ScoreBoard = GetTypeWiseScoreCard(ObjQuestionModel.TestId, ObjQuestionModel.CurrentSetId);

                            ObjQuestionModel.NextSetId = AssessmentModuleDetails.Item1;
                            ObjQuestionModel.NextSetName = AssessmentModuleDetails.Item2;

                            if (ObjQuestionModel.ModuleId == 6)
                            {
                                ObjQuestionModel.IsShowGoToNextSetButton = false;
                            }
                            else if(ObjQuestionModel.ModuleId == 11)
                            {
                                ObjQuestionModel.IsShowGoToNextSetButton = false;
                            }
                            else
                            {
                                ObjQuestionModel.IsShowGoToNextSetButton = true;
                            }
                        }
                        break;
                    case (int)AssessmentModule.Competency:

                        {
                            ObjQuestionModel.CompentencyScoreCard = new CompentencyScoreCard();

                            ObjQuestionModel.CompentencyScoreCard.ScoreBoard = new List<ClsTypeModel>();
                            int SetId = ObjQuestionModel.CurrentSetId;
                            CandidateBM UserModel = UserSvc.GetCandidateData(Convert.ToInt32(TestId));

                            UserModel.MainType = DBEntities.txnDynamicMisTypings.Where(x => x.TestId == TestId && x.Type == 1).OrderByDescending(i => i.MisTypingId).Select(x => x.HighestType.Value).FirstOrDefault();

                            List<int> lstTypeId = new List<int>() { 177 };
                            ObjQuestionModel.CompentencyScoreCard.MultiBarScoreCard = new List<string>();
                            var CompetencyScoreCard = (from ScoreObject in GetTypeWiseScoreBoard(TestId, SetId)
                                                       join ClusterMap in DBEntities.txnClusterMapToCompetencies on ScoreObject.TypeId equals ClusterMap.TypeId
                                                       join MasterCluster in DBEntities.mstClusters on ClusterMap.ClusterId equals MasterCluster.ClusterId
                                                       where !lstTypeId.Contains(ScoreObject.TypeId)
                                                       select new
                                                       {
                                                           TypeId = MasterCluster.ClusterId,
                                                           TypeName = MasterCluster.ClusterName,
                                                           Score = CalculateOfCompetencies(ScoreObject.TypeId, ScoreObject.Score, TestId, SetId),
                                                           ColorCode = ""
                                                       });

                            ObjQuestionModel.CompentencyScoreCard.ScoreBoard = (from a in CompetencyScoreCard
                                                                                join b in DBEntities.mstClusters on a.TypeId equals b.ClusterId
                                                                                group a by a.TypeId into P
                                                                                let TypeId = P.Select(x => x.TypeId).FirstOrDefault()
                                                                                let TypeName = P.Select(x => x.TypeName).FirstOrDefault()
                                                                                let ScoreByClusterWise = SumOfQleadAllCompentency(TypeId, (int)Math.Round(P.Sum(x => x.Score)))
                                                                                select new ClsTypeModel
                                                                                {
                                                                                    TypeId = TypeId,
                                                                                    TypeName = TypeName,
                                                                                    Score = ScoreByClusterWise,
                                                                                    ColorCode = GetColorCode(TypeId, ScoreByClusterWise)
                                                                                }).OrderBy(x => x.TypeName).ToList();

                            foreach (var Cluster in ObjQuestionModel.CompentencyScoreCard.ScoreBoard)
                            {
                                var EachCompentencyData = (from ScoreObject in GetTypeWiseScoreBoard(TestId, SetId)
                                                           join ClusterMap in DBEntities.txnClusterMapToCompetencies on ScoreObject.TypeId equals ClusterMap.TypeId
                                                           join MasterCluster in DBEntities.mstClusters on ClusterMap.ClusterId equals MasterCluster.ClusterId
                                                           where MasterCluster.ClusterId == Cluster.TypeId
                                                           select new ClsMultipleLineBarChart
                                                           {
                                                               Title = Cluster.TypeName,
                                                               TypeId = ScoreObject.TypeId,
                                                               TypeName = ScoreObject.TypeName,
                                                               Score = CalcuateIndividualCompetencies(ScoreObject.TypeId, TestId, SetId, ScoreObject.Score),
                                                               Score1 = GetNormativeScore(UserModel.MainType, ScoreObject.TypeId),
                                                           }).OrderBy(x => x.TypeName).ToList();

                                var JsonData = JsonConvert.SerializeObject(EachCompentencyData);

                                ObjQuestionModel.CompentencyScoreCard.MultiBarScoreCard.Add(JsonData);
                            }

                            ObjQuestionModel.IsShowGoToNextSetButton = false;
                        }
                        break;
                    case (int)AssessmentModule.QTamCompetency:
                        {
                            ObjQuestionModel.CompentencyScoreCard = new CompentencyScoreCard();

                            ObjQuestionModel.CompentencyScoreCard.ScoreBoard = new List<ClsTypeModel>();
                            int SetId = ObjQuestionModel.CurrentSetId;
                            CandidateBM UserModel = UserSvc.GetCandidateData(Convert.ToInt32(TestId));


                            UserModel.MainType = DBEntities.txnDynamicMisTypings.Where(x => x.TestId == TestId && x.Type == 1).OrderByDescending(i => i.MisTypingId).Select(x => x.HighestType.Value).FirstOrDefault();

                            List<int> lstTypeId = new List<int>() { 194 };
                            ObjQuestionModel.CompentencyScoreCard.MultiBarScoreCard = new List<string>();

                            var CompetencyScoreCard = (from ScoreObject in GetTypeWiseScoreBoard(TestId, SetId)
                                                       join ClusterMap in DBEntities.txnClusterMapToCompetencies on ScoreObject.TypeId equals ClusterMap.TypeId 
                                                       join MasterCluster in DBEntities.mstClusters on ClusterMap.ClusterId equals MasterCluster.ClusterId 
                                                       where !lstTypeId.Contains(ScoreObject.TypeId)
                                                       select new
                                                       {
                                                           TypeId = MasterCluster.ClusterId,
                                                           TypeName = MasterCluster.ClusterName,
                                                           Score = CalculateOfCompetencies(ScoreObject.TypeId, ScoreObject.Score, TestId, SetId),
                                                           ColorCode = ""
                                                       });

                            ObjQuestionModel.CompentencyScoreCard.ScoreBoard = (from a in CompetencyScoreCard
                                                           join b in DBEntities.mstClusters on a.TypeId equals b.ClusterId
                                                           group a by a.TypeId into P
                                                           let TypeId = P.Select(x => x.TypeId).FirstOrDefault()
                                                           let TypeName = P.Select(x => x.TypeName).FirstOrDefault()
                                                           let ScoreByClusterWise = SumOfAllCompentency(TypeId, (int)Math.Round(P.Sum(x=>x.Score)))
                                                           select new ClsTypeModel { 
                                                                TypeId = TypeId,
                                                                TypeName = TypeName,
                                                                Score = ScoreByClusterWise,
                                                                ColorCode = GetColorCode(TypeId, ScoreByClusterWise)
                                                           }).OrderBy(x => x.TypeName).ToList();

                            foreach(var Cluster in ObjQuestionModel.CompentencyScoreCard.ScoreBoard)
                            {
                                var EachCompentencyData = (from ScoreObject in GetTypeWiseScoreBoard(TestId, SetId)
                                                           join ClusterMap in DBEntities.txnClusterMapToCompetencies on ScoreObject.TypeId equals ClusterMap.TypeId
                                                           join MasterCluster in DBEntities.mstClusters on ClusterMap.ClusterId equals MasterCluster.ClusterId
                                                           where MasterCluster.ClusterId == Cluster.TypeId
                                                           select new ClsMultipleLineBarChart
                                                           {
                                                               Title = Cluster.TypeName,
                                                               TypeId = ScoreObject.TypeId,
                                                               TypeName = ScoreObject.TypeName,
                                                               Score = CalcuateIndividualCompetencies(ScoreObject.TypeId, TestId, SetId, ScoreObject.Score),
                                                               Score1 = GetNormativeScore(UserModel.MainType, ScoreObject.TypeId),
                                                           }).OrderBy(x => x.TypeName).ToList();

                                var JsonData = JsonConvert.SerializeObject(EachCompentencyData);
                                
                                ObjQuestionModel.CompentencyScoreCard.MultiBarScoreCard.Add(JsonData);
                            }

                            ObjQuestionModel.IsShowGoToNextSetButton = false;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public int CalcuateIndividualCompetencies(int TypeId, int TestId, int SetId, int Score)
        {
            int NumberOfQuestion = DBEntities.txnQuestions.Join(DBEntities.mstQuestions,
                                                   x => x.QuestionId, y => y.QuestionId, (x, y) => new { x, y })
                                                   .Where(i => i.x.TestId == TestId && i.x.ModuleId == SetId && i.y.TypeId == TypeId).Count();

            NumberOfQuestion = NumberOfQuestion * 10;

            decimal CalculateScore = decimal.Divide(Score, NumberOfQuestion) * 100; 
            return (int)Math.Round(decimal.ToDouble(CalculateScore));

        }

        public int GetNormativeScore(int MainTypeId, int TypeId)
        {
            string ColumnName = "Type" + MainTypeId;
            var NormativeData = DBEntities.Mst_QMP_Normatives.Where(x => x.TypeId == TypeId).Select(y=> ColumnName == "Type1" ? y.Type1 :
                                                                                                        ColumnName == "Type2" ? y.Type2 :
                                                                                                        ColumnName == "Type3" ? y.Type3 :
                                                                                                        ColumnName == "Type4" ? y.Type4 :
                                                                                                        ColumnName == "Type5" ? y.Type5 :
                                                                                                        ColumnName == "Type6" ? y.Type6 :
                                                                                                        ColumnName == "Type7" ? y.Type7 :
                                                                                                        ColumnName == "Type8" ? y.Type8 :
                                                                                                        ColumnName == "Type9" ? y.Type9 : null).FirstOrDefault();
            int NormativeScore = Convert.ToInt32(NormativeData);
            return NormativeScore;
        }

        public int SumOfAllCompentency(int ClusterId, int Score)
        {
            int SumOfAllCompent = DBEntities.txnClusterMapToCompetencies.Where(x => x.ClusterId == ClusterId).Sum(x => x.WeightageScore.Value);

            decimal CalculateScore =  decimal.Divide(Score, SumOfAllCompent) * 100;

            return (int)Math.Round(decimal.ToDouble(CalculateScore));

        }
        public int SumOfQleadAllCompentency(int ClusterId, int Score)
        {
            int SumOfAllCompent = DBEntities.txnClusterMapToCompetencies.Where(x => x.ClusterId == ClusterId).Sum(x => x.Weightage.Value);

            decimal CalculateScore = decimal.Divide(Score, SumOfAllCompent) * 100;

            return (int)Math.Round(decimal.ToDouble(CalculateScore));

        }
        public int CalcuatePersonlityToPersenceScore(int Score)
        {
            int FinalCalcuationOfScore;

            decimal CalculateScore = decimal.Divide(Score, 300) * 100;
            FinalCalcuationOfScore = CalculateScore == 0 ? 0 : (int)Math.Round(CalculateScore);
            return FinalCalcuationOfScore;
        }
        public int CalculatedApptitudeScore(int Score)
        {
            int FinalCalcuationOfScore;

            decimal CalculateScore = decimal.Divide(Score, 10) * 100;
            FinalCalcuationOfScore = CalculateScore == 0 ? 0 : (int)Math.Round(CalculateScore);
            return FinalCalcuationOfScore;
        }

        public string GetStatusOfCompentencies(int TypeId, int Score, int TestId, int SetId)
        {
            int FinalScore = GetCompetenciesCalculation(TypeId, Score, TestId, SetId);

            if (FinalScore >= 70)
            {
                return "high";
            }
            else if (FinalScore >= 40)
            {
                return "moderate";
            }
            else if (FinalScore < 40)
            {
                return "low";
            }
            else
            {
                return "";
            }
        }

        public string GetStatusOfApptitude(int Score)
        {
            int FinalScore = CalculatedApptitudeScore(Score);

            if (FinalScore >= 70)
            {
                return "high";
            }
            else if (FinalScore >= 40)
            {
                return "moderate";
            }
            else if (FinalScore < 40)
            {
                return "low";
            }
            else
            {
                return "";
            }
        }

        public string GetColorCode(int? TypeId, int Score)
        {
            if (TypeId != null)
            {
                switch (TypeId)
                {
                    case 1:
                        return "#2386B8";
                    case 29:
                        return "#F2D949";
                    case 2:
                        return "#92C4D6";
                    case 30:
                        return "#C0B6AA";
                    case 3:
                        return "#61A0B2";
                    case 31:
                        return "#6C6F74";
                    case 4:
                        return "#3D7D92";
                    case 32:
                        return "#3B3A40";
                    case 5:
                        return "#235F75";
                    default:
                        return "";
                }
            }
            else
            {
                if (Score >= 70)
                {
                    return "#74C25C";
                }
                else if (Score >= 40)
                {
                    return "#FFA500";
                }
                else if (Score < 40)
                {
                    return "#FF0000";
                }
                else
                {
                    return "";
                }
            }
        }

        public void GenerateErrorInAssessment(int TestId, int UserId)
        {
            CompeteUserTest(TestId, UserId, "E");
            var ExamStatus = DBEntities.txnExamSetStatus.Where(x => x.TestId == TestId && x.Status == "NS").ToList();
            ExamStatus.ForEach(m => m.Status = "E");
            DBEntities.SaveChanges();

        }

        public void AddDynamicMistyping(int? FirstElement, int? SecondElemet, int TestId, int NameOfSubSet)
        {
            string MisTypingName;
            int MisTypingId;
            if (FirstElement != null && SecondElemet != null)
            {
                if (FirstElement < SecondElemet)
                {
                    MisTypingName = FirstElement + "w" + SecondElemet;
                }
                else
                {
                    MisTypingName = SecondElemet + "w" + FirstElement;
                }
                MisTypingId = GetQuestionTypeId(MisTypingName).Value;


                SaveMisTyping("Type" + FirstElement, "Type" + SecondElemet, MisTypingName, MisTypingId, null, TestId, NameOfSubSet);
            }
            else
            {
                SaveMisTyping("", "Type" + SecondElemet, "", 0, null, TestId, (int)enumType.MainType);
            }
        }

        public void SaveDynamicMistyping(List<int> lstTypeId, int TestId, int NameOfSubSet)
        {
            var lstSubset1 = ArrSubSet1.Intersect(lstTypeId).ToList();
            var lstSubset2 = ArrSubSet2.Intersect(lstTypeId).ToList();
            var lstSubset3 = ArrSubSet3.Intersect(lstTypeId).ToList();

            List<int> TypeId = new List<int>();
            List<int> UnMatchTypeId = new List<int>();
            if (lstSubset1.Count == 2)
            {
                TypeId = lstSubset1.OrderBy(x => x).ToList();

                AddDynamicMistyping(TypeId.ElementAt(0), TypeId.ElementAt(1), TestId, NameOfSubSet);

                UnMatchTypeId = lstTypeId.Except(TypeId).ToList();
            }
            else if (lstSubset2.Count == 2)
            {
                TypeId = lstSubset2.OrderBy(x => x).ToList();

                AddDynamicMistyping(TypeId.ElementAt(0), TypeId.ElementAt(1), TestId, NameOfSubSet);

                UnMatchTypeId = lstTypeId.Except(TypeId).ToList();
            }
            else if (lstSubset3.Count == 2)
            {
                TypeId = lstSubset3.OrderBy(x => x).ToList();

                AddDynamicMistyping(TypeId.ElementAt(0), TypeId.ElementAt(1), TestId, NameOfSubSet);

                UnMatchTypeId = lstTypeId.Except(TypeId).ToList();
            }
            else
            {
                TypeId = lstTypeId.OrderBy(x => x).ToList();

                AddDynamicMistyping(TypeId.ElementAt(0), TypeId.ElementAt(1), TestId, NameOfSubSet);
                TypeId.Remove(TypeId.ElementAt(1));
                TypeId.Remove(TypeId.ElementAt(0));


                if (TypeId.Count > 0)
                {
                    foreach (int i in TypeId)
                    {
                        AddDynamicMistyping(null, i, TestId, NameOfSubSet);
                    }
                }
            }

            if (UnMatchTypeId.Count > 0)
            {
                foreach (int value in UnMatchTypeId)
                {
                    AddDynamicMistyping(null, value, TestId, NameOfSubSet);
                }
            }
        }

        public void GenerateDynamicMistyping(List<ClsTypeModel> ScoreCard, int TestId, int UserId, int ModuleId, int SetId)
        {
            try
            {
                string MisTypingName;
                int MisTypingId;

                List<int> FirstHighestScoreType = new List<int>();
                List<int> SecondHighestScoreType = new List<int>();
                List<int> UnmatchValue = new List<int>();
                List<int> OrderListTypeId = new List<int>();
                int[] ArrSubSet1 =
                    {
                        (int)MainType.EmpathicNurturerEnneagramType2,
                        (int)MainType.AmbitiousAchieverEnneagramType3,
                        (int)MainType.IntenseIndividualistEnneagramType4
                    };
                int[] ArrSubSet2 =
                {
                        (int)MainType.PerceptiveSpecialistEnneagramType5,
                        (int)MainType.DutifulLoyalistEnneagramType6,
                        (int)MainType.VersatileVisionaryEnneagramType7
                    };
                int[] ArrSubSet3 =
                {
                        (int)MainType.CharismaticControllerEnneagramType8,
                        (int)MainType.ReceptivePeacemakerEnneagramType9,
                        (int)MainType.EthicalPerfectionistEnneagramType1
                    };
                var results = ScoreCard
                                   .OrderByDescending(o => o.Score)
                                   .GroupBy(o => o.Score)
                                   .SelectMany((l, i) => l.Select(v => new { Name = v.TypeName, Id = v.TypeId, Score = v.Score, Position = i + 1 }));

                int FirstHighestScore = results.Where(x => x.Position == 1).Count();
                int SecondHighestScore = results.Where(x => x.Position == 2).Count();

                var Subset1 = ScoreCard.Where(x => ArrSubSet1.Contains(x.TypeId)).OrderByDescending(o => o.Score)
                              .GroupBy(o => o.Score)
                              .SelectMany((l, i) => l.Select(v => new { Name = v.TypeName, Id = v.TypeId, Score = v.Score, Position = i + 1 }));
                var Subset2 = ScoreCard.Where(x => ArrSubSet2.Contains(x.TypeId)).OrderByDescending(o => o.Score)
                               .GroupBy(o => o.Score)
                               .SelectMany((l, i) => l.Select(v => new { Name = v.TypeName, Id = v.TypeId, Score = v.Score, Position = i + 1 }));
                var Subset3 = ScoreCard.Where(x => ArrSubSet3.Contains(x.TypeId)).OrderByDescending(o => o.Score)
                               .GroupBy(o => o.Score)
                               .SelectMany((l, i) => l.Select(v => new { Name = v.TypeName, Id = v.TypeId, Score = v.Score, Position = i + 1 }));

                int SubSet1_Count = Subset1.Where(x => x.Position == 1).Count();
                int SubSet2_Count = Subset2.Where(x => x.Position == 1).Count();
                int SubSet3_Count = Subset3.Where(x => x.Position == 1).Count();



                if (FirstHighestScore >= 4 || SecondHighestScore >= 4)
                {
                    GenerateErrorInAssessment(TestId, UserId);
                    return;
                }
                else if (SubSet1_Count == 3 || SubSet2_Count == 3 || SubSet3_Count == 3)
                {
                    GenerateErrorInAssessment(TestId, UserId);
                    return;
                }


                List<int> ListOfTypeId = new List<int>();
                switch (FirstHighestScore)
                {
                    case 1:
                        switch (SecondHighestScore)
                        {
                            case 1:
                                var TopTwoScore = results.Take(2).OrderBy(x => x.Id);

                                AddDynamicMistyping(TopTwoScore.ElementAt(0).Id, TopTwoScore.ElementAt(1).Id, TestId, (int)enumType.MainType);

                                break;
                            case 2:
                            case 3:
                                ListOfTypeId = results.Where(x => x.Position == 2).Select(i => i.Id).ToList();

                                SaveDynamicMistyping(ListOfTypeId, TestId, (int)enumType.MainType);

                                int FirstHighestValue = results.Where(x => x.Position == 1).Select(i => i.Id).FirstOrDefault();

                                SaveMisTyping("", "Type" + FirstHighestValue, "", 0, null, TestId, (int)enumType.MainType);
                                break;
                        }
                        break;
                    case 2:
                    case 3:
                        ListOfTypeId = results.Where(x => x.Position == 1).Select(i => i.Id).ToList();

                        SaveDynamicMistyping(ListOfTypeId, TestId, (int)enumType.MainType);

                        break;
                }


                #region Subset 1 Logic 

                if (Subset1.Where(x => x.Position == 1).Count() == 1 && Subset1.Where(x => x.Position == 2).Count() == 1 && Subset1.Where(x => x.Position == 3).Count() == 1)
                {
                    var TopTwoScore = Subset1.Take(2).OrderBy(x => x.Id);
                    MisTypingName = TopTwoScore.ElementAt(0).Id + "w" + TopTwoScore.ElementAt(1).Id;
                    MisTypingId = GetQuestionTypeId(MisTypingName).Value;
                    SaveMisTyping("Type" + Subset1.ElementAt(0).Id, "Type" + Subset1.ElementAt(1).Id, MisTypingName, MisTypingId, Subset1.ElementAt(0).Id, TestId, (int)enumType.FeelingCentre);
                }
                else if (Subset1.Where(x => x.Position == 1).Count() == 1 && Subset1.Where(x => x.Position == 2).Count() == 2)
                {
                    var TopTwoScore = Subset1.Take(2).OrderBy(x => x.Id);
                    MisTypingName = TopTwoScore.ElementAt(0).Id + "w" + TopTwoScore.ElementAt(1).Id;
                    MisTypingId = GetQuestionTypeId(MisTypingName).Value;
                    SaveMisTyping("Type" + Subset1.ElementAt(0).Id, "Type" + Subset1.ElementAt(1).Id, MisTypingName, MisTypingId, Subset1.ElementAt(0).Id, TestId, (int)enumType.FeelingCentre);
                }
                else
                {
                    var TopTwoScore = Subset1.Take(2).OrderBy(x => x.Id);
                    MisTypingName = TopTwoScore.ElementAt(0).Id + "w" + TopTwoScore.ElementAt(1).Id;
                    MisTypingId = GetQuestionTypeId(MisTypingName).Value;
                    SaveMisTyping("Type" + TopTwoScore.ElementAt(0).Id, "Type" + TopTwoScore.ElementAt(1).Id, MisTypingName, MisTypingId, null, TestId, (int)enumType.FeelingCentre);
                }

                #endregion

                #region Subset 2 Logic

                if (Subset2.Where(x => x.Position == 1).Count() == 1 && Subset2.Where(x => x.Position == 2).Count() == 1 && Subset2.Where(x => x.Position == 3).Count() == 1)
                {
                    var TopTwoScore = Subset2.Take(2).OrderBy(x => x.Id);
                    MisTypingName = TopTwoScore.ElementAt(0).Id + "w" + TopTwoScore.ElementAt(1).Id;
                    MisTypingId = GetQuestionTypeId(MisTypingName).Value;
                    SaveMisTyping("Type" + Subset2.ElementAt(0).Id, "Type" + Subset2.ElementAt(1).Id, MisTypingName, MisTypingId, Subset2.ElementAt(0).Id, TestId, (int)enumType.ThinkingCentre);
                }
                else if (Subset2.Where(x => x.Position == 1).Count() == 1 && Subset2.Where(x => x.Position == 2).Count() == 2)
                {
                    var TopTwoScore = Subset2.Take(2).OrderBy(x => x.Id);
                    MisTypingName = TopTwoScore.ElementAt(0).Id + "w" + TopTwoScore.ElementAt(1).Id;
                    MisTypingId = GetQuestionTypeId(MisTypingName).Value;
                    SaveMisTyping("Type" + Subset2.ElementAt(0).Id, "Type" + Subset2.ElementAt(1).Id, MisTypingName, MisTypingId, Subset2.ElementAt(0).Id, TestId, (int)enumType.ThinkingCentre);
                }
                else
                {
                    var TopTwoScore = Subset2.Take(2).OrderBy(x => x.Id);
                    MisTypingName = TopTwoScore.ElementAt(0).Id + "w" + TopTwoScore.ElementAt(1).Id;
                    MisTypingId = GetQuestionTypeId(MisTypingName).Value;
                    SaveMisTyping("Type" + TopTwoScore.ElementAt(0).Id, "Type" + TopTwoScore.ElementAt(1).Id, MisTypingName, MisTypingId, null, TestId, (int)enumType.ThinkingCentre);
                }

                #endregion

                #region Subset 3 Logic 

                if (Subset3.Where(x => x.Position == 1).Count() == 1 && Subset3.Where(x => x.Position == 2).Count() == 1 && Subset3.Where(x => x.Position == 3).Count() == 1)
                {
                    var TopTwoScore = Subset3.Take(2).OrderBy(x => x.Id);
                    MisTypingName = TopTwoScore.ElementAt(0).Id + "w" + TopTwoScore.ElementAt(1).Id;
                    MisTypingId = GetQuestionTypeId(MisTypingName).Value;
                    SaveMisTyping("Type" + Subset3.ElementAt(0).Id, "Type" + Subset3.ElementAt(1).Id, MisTypingName, MisTypingId, Subset3.ElementAt(0).Id, TestId, (int)enumType.ActionCentre);
                }
                else if (Subset3.Where(x => x.Position == 1).Count() == 1 && Subset3.Where(x => x.Position == 2).Count() == 2)
                {
                    var TopTwoScore = Subset3.Take(2).OrderBy(x => x.Id);
                    MisTypingName = TopTwoScore.ElementAt(0).Id + "w" + TopTwoScore.ElementAt(1).Id;
                    MisTypingId = GetQuestionTypeId(MisTypingName).Value;
                    SaveMisTyping("Type" + Subset3.ElementAt(0).Id, "Type" + Subset3.ElementAt(1).Id, MisTypingName, MisTypingId, Subset3.ElementAt(0).Id, TestId, (int)enumType.ActionCentre);
                }
                else
                {
                    var TopTwoScore = Subset3.Take(2).OrderBy(x => x.Id);
                    MisTypingName = TopTwoScore.ElementAt(0).Id + "w" + TopTwoScore.ElementAt(1).Id;
                    MisTypingId = GetQuestionTypeId(MisTypingName).Value;
                    SaveMisTyping("Type" + TopTwoScore.ElementAt(0).Id, "Type" + TopTwoScore.ElementAt(1).Id, MisTypingName, MisTypingId, null, TestId, (int)enumType.ActionCentre);
                }

                #endregion

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public void updateMainTypeInDB(int TestId)
        {
            var UserId = DBEntities.txnUserTestDetails.Where(x => x.TestId == TestId).Select(x => x.UserId).FirstOrDefault();

            txnCandidate objCandidate = DBEntities.txnCandidates.Where(x => x.UserId == UserId).FirstOrDefault();

            int MainType = DBEntities.txnDynamicMisTypings.Where(x => x.TestId == TestId && x.Type == 1).OrderByDescending(i => i.MisTypingId).Select(x => x.HighestType.Value).FirstOrDefault();

            objCandidate.MainType = MainType;

            DBEntities.Entry(objCandidate).State = System.Data.Entity.EntityState.Modified;
            DBEntities.SaveChanges();

        }


        public Tuple<List<int>, bool, bool, bool, int, int> GetQuestionSeriesData(int TestId, int CurrentSetId, List<Question> Question, int? QuestionId)
        {
            try
            {
                List<int> DisplayQuestion = new List<int>();
                bool IsDisplayNextBtn = true;
                bool IsDisplayPreBtn = true;
                bool IsDisplaySubmitBtn = true;
                var InCompletedQuestionIds = Question.Where(x => x.isAnswer == null).Select(x => x.QuestionId).ToList();
                var CompletedQuestionIds = Question.Where(x => x.isAnswer != null).Select(x => x.QuestionId).ToList();
                int NextQuestionId = 0, PreQuestionId = 0;

                switch (CurrentSetId)
                {
                    case (int)AssessmentModule.H1PartAMainType:
                    case (int)AssessmentModule.MainType:
                    case (int)AssessmentModule.QLEAPAndQMAPMainType:
                    case (int)AssessmentModule.EnneagramInstincts:
                    case (int)AssessmentModule.H1PartAMistyping:
                    case (int)AssessmentModule.Mistyping:
                    case (int)AssessmentModule.H1PartACompetency:
                    case (int)AssessmentModule.PersonalityToPresence:
                    case (int)AssessmentModule.CenterOfExpression:
                    case (int)AssessmentModule.StressAndResilience:
                    case (int)AssessmentModule.Competency:
                    case (int)AssessmentModule.QTamCompetency:
                        {
                            IsDisplayPreBtn = InCompletedQuestionIds.Count == Question.Select(x => x.QuestionId).ToList().Count ? false : true;

                            IsDisplayNextBtn = InCompletedQuestionIds.Count != CompletedQuestionIds.Count ? true : false;

                            IsDisplaySubmitBtn = CompletedQuestionIds.Count + 1 == Question.Select(x => x.QuestionId).ToList().Count ? true : false;

                            if (IsDisplaySubmitBtn)
                            {
                                IsDisplayNextBtn = false;
                            }
                            else
                            {
                                IsDisplayNextBtn = true;
                            }

                            DisplayQuestion.Add(InCompletedQuestionIds.ElementAt(0));
                        }
                        break;
                    case (int)AssessmentModule.H1PartAAptitude:
                        {
                            int? NxtQuesId, PrevQuesId;
                            if (QuestionId != null)
                            {
                                var CurrentQuestionIdPoistion = DBEntities.txnQuestions.Where(x => x.TestId == TestId && x.ModuleId == CurrentSetId && x.QuestionId == QuestionId).Select(x => x.QuesOrder).FirstOrDefault();

                                NxtQuesId = DBEntities.txnQuestions.Where(x => x.TestId == TestId && x.ModuleId == CurrentSetId && x.QuesOrder == CurrentQuestionIdPoistion + 1).Select(x => x.QuestionId).FirstOrDefault();
                                PrevQuesId = DBEntities.txnQuestions.Where(x => x.TestId == TestId && x.ModuleId == CurrentSetId && x.QuesOrder == CurrentQuestionIdPoistion - 1).Select(x => x.QuestionId).FirstOrDefault();

                            }
                            else
                            {
                                var CurrentQuestionIdPoistion = DBEntities.txnQuestions.Where(x => x.TestId == TestId && x.ModuleId == CurrentSetId && x.IsAnswer == null)
                                                                    .OrderBy(i => i.QuesOrder).Select(x => x.QuesOrder).FirstOrDefault();

                                NxtQuesId = DBEntities.txnQuestions.Where(x => x.TestId == TestId && x.ModuleId == CurrentSetId && x.QuesOrder == CurrentQuestionIdPoistion + 1).Select(x => x.QuestionId).FirstOrDefault();
                                PrevQuesId = DBEntities.txnQuestions.Where(x => x.TestId == TestId && x.ModuleId == CurrentSetId && x.QuesOrder == CurrentQuestionIdPoistion - 1).Select(x => x.QuestionId).FirstOrDefault();
                            }


                            NextQuestionId = NxtQuesId.HasValue ? NxtQuesId.Value : 0;
                            //NextQuestionId = DBEntities.txnQuestions.Where(x => x.TestId == TestId && x.Setid == CurrentSetId && x.IsAnswer == null && x.QuestionId != NextQuestionId)
                            //                                        .OrderBy(i => i.QuesOrder).Select(x => x.QuestionId.Value).FirstOrDefault();
                            PreQuestionId = PrevQuesId.HasValue ? PrevQuesId.Value : 0;

                            if (QuestionId != null)
                            {
                                DisplayQuestion.Add(QuestionId.Value);
                            }
                            else
                            {
                                var QuesId = DBEntities.txnQuestions.Where(x => x.TestId == TestId && x.ModuleId == CurrentSetId && x.IsAnswer == null)
                                                                    .OrderBy(x => x.QuesOrder).Select(x => x.QuestionId.Value).FirstOrDefault();
                                DisplayQuestion.Add(QuesId);
                            }

                            int TempQuestionId = DisplayQuestion.ElementAt(0);
                            var GetPositionQues = DBEntities.txnQuestions.Where(x => x.TestId == TestId && x.ModuleId == CurrentSetId && x.QuestionId == TempQuestionId)
                                                                        .Select(x => x.QuesOrder).FirstOrDefault();

                            IsDisplayPreBtn = GetPositionQues > 0 ? true : false;
                            IsDisplayNextBtn = GetPositionQues < Question.Select(x => x.QuestionId).ToList().Count ? true : false;
                            IsDisplaySubmitBtn = GetPositionQues == 29 ? true : false;

                            if (IsDisplaySubmitBtn)
                            {
                                IsDisplayNextBtn = false;
                            }
                        }
                        break;
                }

                return new Tuple<List<int>, bool, bool, bool, int, int>(DisplayQuestion, IsDisplayNextBtn, IsDisplayPreBtn, IsDisplaySubmitBtn, NextQuestionId, PreQuestionId);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public void SaveQuestion(List<ClsQuestion> lstQuestion)
        {
            try
            {
                Dictionary<string, int> RatingWiseScore = new Dictionary<string, int>();
                RatingWiseScore.Add("-4", 10);
                RatingWiseScore.Add("-3", 25);
                RatingWiseScore.Add("-2", 40);
                RatingWiseScore.Add("-1", 50);
                RatingWiseScore.Add("0", 0);
                RatingWiseScore.Add("1", 60);
                RatingWiseScore.Add("2", 75);
                RatingWiseScore.Add("3", 90);
                RatingWiseScore.Add("4", 100);
                txnQuestion Question = new txnQuestion();
                txnUserTestDetail UserDetails = new txnUserTestDetail();
                dynamic weight;
                using (var scope = DBEntities.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (var data in lstQuestion)
                        {
                            Question = DBEntities.txnQuestions.Where(x => x.QuestionId == data.QuestionId && x.TestId == data.TestId && x.TxnQuestionId == data.TestQuestionId).FirstOrDefault();
                            UserDetails = DBEntities.txnUserTestDetails.Where(x => x.TestId == data.TestId.Value).FirstOrDefault();

                            if (data.ResponseTypeId == 4)
                            {
                                List<txnQuestionResponse> lstQuesResponse = new List<txnQuestionResponse>();

                                foreach (ClsQuestionResponse QuesResp in data.lstQuestionRes)
                                {
                                    if (QuesResp.Checked)
                                    {
                                        lstQuesResponse.Add(new txnQuestionResponse
                                        {
                                            QuestionId = data.QuestionId,
                                            QuestionResponseId = QuesResp.ResponseId,
                                            TxnQuestionId = data.TestQuestionId,
                                            TestId = data.TestId.Value,
                                            TypeId = data.TypeId.Value
                                        });
                                    }
                                }


                                DBEntities.txnQuestionResponses.AddRange(lstQuesResponse.
                                Select(x => new txnQuestionResponse
                                {
                                    QuestionId = x.QuestionId,
                                    QuestionResponseId = x.QuestionResponseId,
                                    TxnQuestionId = x.TxnQuestionId,
                                    TestId = x.TestId,
                                    TypeId = x.TypeId
                                }));


                                weight = null;
                                if (lstQuesResponse.Any(x => x.QuestionResponseId == 3567))
                                {
                                    weight = 1;
                                }
                                else
                                {
                                    switch (lstQuesResponse.Count)
                                    {
                                        case 1:
                                            weight = 3;
                                            break;
                                        case 2:
                                            weight = 5;
                                            break;
                                        case 3:
                                            weight = 7;
                                            break;
                                        case 4:
                                            weight = 9;
                                            break;
                                    }
                                }
                                if (Question != null)
                                {
                                    DateTime DateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
                                    Question.ImpactScore = weight == null ? null : weight; ;
                                    Question.ResponseAt = DateTime;//DateTime.UtcNow;
                                    Question.IsAnswer = true;
                                    Question.ResponseBy = UserDetails.UserId;
                                    DBEntities.Entry(Question).State = System.Data.Entity.EntityState.Modified;
                                }

                            }
                            else
                            {
                                if (!(DBEntities.txnQuestionResponses.Any(x => x.TestId == data.TestId.Value && x.QuestionId == data.QuestionId && x.TxnQuestionId == data.TestQuestionId)))
                                {
                                    txnQuestionResponse objQuesResponse = new txnQuestionResponse();
                                    objQuesResponse.QuestionId = data.QuestionId;
                                    objQuesResponse.QuestionResponseId = data.ResponseValue;
                                    objQuesResponse.TxnQuestionId = data.TestQuestionId;
                                    objQuesResponse.TestId = data.TestId.Value;
                                    objQuesResponse.TypeId = data.TypeId.Value;
                                    DBEntities.txnQuestionResponses.Add(objQuesResponse);


                                    if (Question != null)
                                    {
                                        if (data.ResponseTypeId == 3)
                                        {
                                            weight = RatingWiseScore.Where(x => x.Key == data.Rating.ToString()).Select(x => x.Value).FirstOrDefault();
                                        }
                                        else
                                        {
                                            weight = DBEntities.mstQuestionResponses.Where(x => x.ResponseId == data.ResponseValue
                                                                                                   && x.QuestionId == data.QuestionId).Select(x => x.weight).FirstOrDefault();
                                        }
                                        DateTime DateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
                                        Question.ImpactScore = weight == null ? null : weight;
                                        Question.ResponseAt = DateTime;//DateTime.UtcNow;
                                        Question.IsAnswer = true;
                                        Question.ResponseBy = UserDetails.UserId;
                                        DBEntities.Entry(Question).State = System.Data.Entity.EntityState.Modified;
                                    }
                                }
                            }

                        }

                        // do some stuff 
                        DBEntities.SaveChanges();


                        scope.Commit();
                    }
                    catch (Exception)
                    {
                        scope.Rollback();
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }

        }



        public void GenerateQuestionByDynamicMistyping(int UserId, int SetId, int TestId, ref int RefTypeId)
        {
            string MisTypingName;
            int MisTypingId;
            List<txnDynamicMisTyping> MainType = DBEntities.txnDynamicMisTypings.Where(x => x.TestId == TestId && x.Type == (int)enumType.MainType && x.status == "NS").ToList();
            List<txnDynamicMisTyping> ActionCenter = DBEntities.txnDynamicMisTypings.Where(x => x.TestId == TestId && x.Type == (int)enumType.ActionCentre && x.status == "NS").ToList();
            List<txnDynamicMisTyping> ThinkingCentre = DBEntities.txnDynamicMisTypings.Where(x => x.TestId == TestId && x.Type == (int)enumType.ThinkingCentre && x.status == "NS").ToList();
            List<txnDynamicMisTyping> FeelingCentre = DBEntities.txnDynamicMisTypings.Where(x => x.TestId == TestId && x.Type == (int)enumType.FeelingCentre && x.status == "NS").ToList();

            foreach (txnDynamicMisTyping MainDynamicMis in MainType)
            {
                int? HighestTypeId = DBEntities.txnDynamicMisTypings.Where(x => x.TestId == TestId && x.Type == (int)enumType.MainType && x.status == "C").OrderByDescending(x => x.MisTypingId).Select(x => x.HighestType).FirstOrDefault();
                //    MainDynamicMis.TypeA = "Type" + HighestTypeId.Value;
                int Type = 0;

                if (MainDynamicMis.TypeB == "")
                {
                    Type = Convert.ToInt32(MainDynamicMis.TypeA.Replace("Type", ""));
                    MainDynamicMis.TypeB = "Type" + HighestTypeId.Value;
                }
                else if (MainDynamicMis.TypeA == "")
                {
                    Type = Convert.ToInt32(MainDynamicMis.TypeB.Replace("Type", ""));
                    MainDynamicMis.TypeA = "Type" + HighestTypeId.Value;
                }
                //  int TypeA = Convert.ToInt32(MainDynamicMis.TypeB.Replace("Type", ""));
                if (Type < HighestTypeId.Value)
                {
                    MisTypingName = Type + "w" + HighestTypeId.Value;
                }
                else
                {
                    MisTypingName = HighestTypeId.Value + "w" + Type;
                }

                MisTypingId = GetQuestionTypeId(MisTypingName).Value;

                if (!(DBEntities.txnDynamicMisTypings.Any(x => x.TestId == TestId && x.status == "C" && x.MisTypeId == MisTypingId && x.Type == (int)enumType.MainType)))
                {
                    MainDynamicMis.MisTypeId = MisTypingId;
                    MainDynamicMis.MisTypeName = MisTypingName;
                    MainDynamicMis.status = "P";
                    DBEntities.Entry(MainDynamicMis).State = System.Data.Entity.EntityState.Modified;
                    DBEntities.SaveChanges();

                    RefTypeId = MisTypingId;

                    return;
                }
                else
                {
                    MainDynamicMis.MisTypeId = MisTypingId;
                    MainDynamicMis.MisTypeName = MisTypingName;
                    MainDynamicMis.status = "C";
                    DBEntities.Entry(MainDynamicMis).State = System.Data.Entity.EntityState.Modified;
                    DBEntities.SaveChanges();
                }

            }


            foreach (txnDynamicMisTyping ActionDynamicMis in ActionCenter)
            {
                if (!(DBEntities.txnDynamicMisTypings.Any(x => x.TestId == TestId && x.status == "C" && x.MisTypeId == ActionDynamicMis.MisTypeId)))
                {
                    ActionDynamicMis.status = "P";
                    DBEntities.Entry(ActionDynamicMis).State = System.Data.Entity.EntityState.Modified;
                    DBEntities.SaveChanges();

                    RefTypeId = ActionDynamicMis.MisTypeId.Value;

                    return;
                }
                else
                {
                    ActionDynamicMis.status = "C";
                    DBEntities.Entry(ActionDynamicMis).State = System.Data.Entity.EntityState.Modified;
                    DBEntities.SaveChanges();
                }
            }


            foreach (txnDynamicMisTyping FeelingDynamicMis in FeelingCentre)
            {
                if (!(DBEntities.txnDynamicMisTypings.Any(x => x.TestId == TestId && x.status == "C" && x.MisTypeId == FeelingDynamicMis.MisTypeId)))
                {
                    FeelingDynamicMis.status = "P";
                    DBEntities.Entry(FeelingDynamicMis).State = System.Data.Entity.EntityState.Modified;
                    DBEntities.SaveChanges();

                    RefTypeId = FeelingDynamicMis.MisTypeId.Value;

                    return;
                }
                else
                {
                    FeelingDynamicMis.status = "C";
                    DBEntities.Entry(FeelingDynamicMis).State = System.Data.Entity.EntityState.Modified;
                    DBEntities.SaveChanges();
                }
            }


            foreach (txnDynamicMisTyping ThinkingDynamicMis in ThinkingCentre)
            {
                if (!(DBEntities.txnDynamicMisTypings.Any(x => x.TestId == TestId && x.status == "C" && x.MisTypeId == ThinkingDynamicMis.MisTypeId)))
                {
                    ThinkingDynamicMis.status = "P";
                    DBEntities.Entry(ThinkingDynamicMis).State = System.Data.Entity.EntityState.Modified;
                    DBEntities.SaveChanges();

                    RefTypeId = ThinkingDynamicMis.MisTypeId.Value;

                    return;
                }
                else
                {
                    ThinkingDynamicMis.status = "C";
                    DBEntities.Entry(ThinkingDynamicMis).State = System.Data.Entity.EntityState.Modified;
                    DBEntities.SaveChanges();
                }
            }


        }

        public void UpdateMisTypeModule(int TypeId, string Status, int TestId, int SetId)
        {
            try
            {
                string SubTypeName = GetSubTypeWiseScoreBoard(TestId, SetId, TypeId).Select(x => x.TypeName).FirstOrDefault();
                int HighestTypeId = Convert.ToInt32(SubTypeName.Replace("MType", ""));


                var ObjDynamixMis = DBEntities.txnDynamicMisTypings.Where(x => x.MisTypeId == TypeId && x.TestId == TestId).ToList();
                ObjDynamixMis.ForEach(m =>
                {
                    m.status = Status;
                    m.HighestType = HighestTypeId;
                });

                DBEntities.SaveChanges();

            }
            catch (Exception ex)
            {
                throw;
            }
        }



        public void SaveMisTyping(string TypeA, string TypeB, string MisTypeName, int MistTypeId, int? HighestType, int TestId, int Type)
        {
            try
            {
                DateTime DateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

                txnDynamicMisTyping objMistTyping = new txnDynamicMisTyping();
                objMistTyping.TypeA = TypeA;
                objMistTyping.TypeB = TypeB;
                objMistTyping.MisTypeName = MisTypeName;
                objMistTyping.MisTypeId = MistTypeId;
                objMistTyping.TestId = TestId;
                objMistTyping.Type = Type;
                objMistTyping.status = HighestType.HasValue ? "C" : "NS";
                objMistTyping.HighestType = HighestType.HasValue ? HighestType.Value : HighestType;
                objMistTyping.CreateAt = DateTime;
                objMistTyping.LastModifiedAt = DateTime;

                DBEntities.txnDynamicMisTypings.Add(objMistTyping);
                DBEntities.SaveChanges();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public bool CheckAllMistypingModuleCompleted(int TestId)
        {
            try
            {
                var CountMistyping = DBEntities.txnDynamicMisTypings.Where(x => x.TestId == TestId).Count();

                var CompletedMistyping = DBEntities.txnDynamicMisTypings.Where(x => x.TestId == TestId && x.status == "C").Count();

                if (CountMistyping == CompletedMistyping)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public string GetSetName(int SetId)
        {
            return DBEntities.mstAssessmentModules.Where(x => x.ModuleId == SetId).Select(x => x.PartialModuleName).FirstOrDefault();
        }

        public string GetCurrentExamStatus(int TestId, int SetId)
        {
            string ExamStatus = DBEntities.txnExamSetStatus.Where(x => x.TestId == TestId && x.ModuleId == SetId).Select(x => x.Status).FirstOrDefault();
            return ExamStatus;
        }

        public List<Question> GetAllQuestion(int TestId, int setId)
        {
            try
            {
                List<Question> Question = DBEntities.txnQuestions.Where(x => x.TestId == TestId && x.ModuleId == setId).Select(x => new Question
                {
                    QuestionId = (int)x.QuestionId,
                    isAnswer = x.IsAnswer.Value
                }).OrderBy(x => Guid.NewGuid()).ToList();
                return Question;
            }
            catch (Exception ex)
            {
                throw;
            }

        }
        public List<QuestionIdStatus> GetQuestionNumberAndStatus(int TestId, int setId)
        {
            try
            {
                List<QuestionIdStatus> Question = DBEntities.txnQuestions.Where(x => x.TestId == TestId && x.ModuleId == setId).OrderBy(x => x.QuesOrder).Select(x => new QuestionIdStatus
                {
                    QuestionId = x.QuestionId.Value,
                    Status = x.IsAnswer == null ? "N" : "C"
                }).ToList();

                return Question;
            }
            catch (Exception ex)
            {
                throw;
            }



        }
        public bool IsAllAnswerAptitudeQuestion(int TestId, int SetId)
        {
            int AnsweredQuestion = DBEntities.txnQuestions.Where(x => x.TestId == TestId && x.ModuleId == SetId && x.IsAnswer == true).Count();
            int AllQuestion = DBEntities.txnQuestions.Where(x => x.TestId == TestId && x.ModuleId == SetId).Count();

            return AnsweredQuestion == AllQuestion ? true : false;
        }

        //  public List<ClsQuestion> GetAllQuestionDetails(int NextQuestion, int PrevQuestion, int TestId, int SetId)
        public List<ClsQuestion> GetAllQuestionDetails(List<int> DisplayQuestionIds, int TestId, int SetId)
        {
            try
            {
                ClsQuestionModel objQuestion = new ClsQuestionModel();

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
                                                                           ResponseNumber = Quesres.ResponseNumber.Value,
                                                                           Checked = false
                                                                       }).ToList()
                                                let ResponseValue = DBEntities.txnQuestionResponses.Where(x => x.QuestionId == q.QuestionId && x.TestId == q.TestId).FirstOrDefault()
                                                where DisplayQuestionIds.Contains((int)q.QuestionId)
                                                      && q.TestId == TestId && q.ModuleId == SetId && mq.IsActive == true
                                                select new ClsQuestion
                                                {
                                                    TestQuestionId = q.TxnQuestionId,
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

        public List<ClsTypeModel> GetTypeWiseScoreBoard(int TestId, int SetId)
        {

            try
            {
                List<ClsTypeModel> lstTypeWiseScoreCard = new List<ClsTypeModel>();

                lstTypeWiseScoreCard = DBEntities.usp_TypeWiseScoreBoard(testId: TestId, sETId: SetId).Select(x => new ClsTypeModel
                {
                    TypeId = x.TypeId.HasValue ? x.TypeId.Value : 0,
                    TypeName = x.TypeName,
                    Score = x.ImpactScore.HasValue ? x.ImpactScore.Value : 0,
                    ColorCode = x.colorCode
                }).ToList();

                if (SetId != (int)AssessmentModule.H1PartAAptitude)
                {
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
                }


                return lstTypeWiseScoreCard;
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        public List<ClsTypeModel> GetTypeWiseScoreCard(int TestId, int SetId)
        {

            try
            {
                List<ClsTypeModel> lstTypeWiseScoreCard = new List<ClsTypeModel>();

                lstTypeWiseScoreCard = DBEntities.usp_GetRowScoreTypeWise(testId: TestId, sETId: SetId).Select(x => new ClsTypeModel
                {
                    TypeId = x.TypeId.HasValue ? x.TypeId.Value : 0,
                    TypeName = x.TypeName,
                    Score = x.ImpactScore.HasValue ? x.ImpactScore.Value : 0,
                    ColorCode = x.colorCode
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

                return lstTypeWiseScoreCard;
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        public List<ClsTypeModel> GetSubTypeWiseScoreBoard(int TestId, int SetId, int? TypeId)
        {
            try
            {
                List<ClsTypeModel> lstSubTypeWiseScoreCard = new List<ClsTypeModel>();

                lstSubTypeWiseScoreCard = DBEntities.usp_SubTypeWiseScoreBoard(TestId, SetId, TypeId).Select(x => new ClsTypeModel
                {
                    TypeId = x.SubTypeId.HasValue ? x.SubTypeId.Value : 0,
                    TypeName = x.SubTypeName,
                    Score = x.ImpactScore.HasValue ? x.ImpactScore.Value : 0,
                    ColorCode = x.colorCode
                }).OrderByDescending(x => x.Score).ToList();

                if (SetId == (int)AssessmentModule.H1PartAMistyping || SetId == (int)AssessmentModule.Mistyping)
                {
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
                }


                return lstSubTypeWiseScoreCard;
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
                        objQuesResponse.TxnQuestionId = Questiondata.TestQuestionId;
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
                            TxnQuestionId = objQuesResponse.TxnQuestionId,
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
                           TxnQuestionId = x.TxnQuestionId,
                           TestId = x.TestId,
                           TypeId = x.TypeId,
                           SubTypeId = x.SubTypeId,
                           impactscore = x.impactscore
                       }));
                    //DBEntities.SaveChanges();
                    txnQuestion Question = DBEntities.txnQuestions.Where(x => x.QuestionId == Questiondata.QuestionId && x.TestId == Questiondata.TestId && x.TxnQuestionId == Questiondata.TestQuestionId).FirstOrDefault();

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
                txnExamSetStatu objExamStatus = DBEntities.txnExamSetStatus.Where(x => x.TestId == TestId && x.ModuleId == SetId).FirstOrDefault();
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


        public int? GetQuestionTypeId(string TypeName)
        {
            try
            {
                int TypeId = DBEntities.mstQuestionTypes.Where(x => x.TypeName == TypeName).Select(x => x.TypeId).FirstOrDefault();

                return TypeId;
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        public void CompeteUserTest(int TestId, int UserId, string Status)
        {
            try
            {
                txnUserTestDetail UserTestDetail = DBEntities.txnUserTestDetails.Where(x => x.TestId == TestId && x.UserId == UserId).FirstOrDefault();

                if (UserTestDetail != null)
                {
                    UserTestDetail.status = Status;
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

        //public List<ClsSet6ScoreModel> lstSubModuleScore(int Testid, int setid)
        //{
        //    try
        //    {
        //        List<ClsSet6ScoreModel> lstSubModuleScore = (from i in DBEntities.txnQuesResponseWithSubModules
        //                                                     join j in DBEntities.mstSubModules
        //                                                     on i.SubModuleId equals j.SubModuleId
        //                                                     where i.TestId == Testid && i.SetId == setid
        //                                                     select new ClsSet6ScoreModel
        //                                                     {
        //                                                         SubModuleId = j.SubModuleId,
        //                                                         SubModuleName = j.SubModuleName,
        //                                                         PresenceScore = i.calcuation.Value,
        //                                                         PersonalityScore = i.AvgOfScore.Value
        //                                                     }).ToList();

        //        return lstSubModuleScore;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }

        //}

        public Tuple<List<Question>, int> GetAllforSet6Question(int TestId, int setId)
        {
            try
            {
                int SubModuleId = 0;

                var SubModuleData = (from txnQues in DBEntities.txnQuestions
                                     join mstQues in DBEntities.mstQuestions
                                     on txnQues.QuestionId equals mstQues.QuestionId
                                     where txnQues.TestId == TestId && txnQues.ModuleId == setId && txnQues.IsAnswer == null
                                     group mstQues by mstQues.SubModuleId into subModuleid
                                     select new
                                     {
                                         SubModuleId = (int)subModuleid.Key
                                     }).ToList();

                SubModuleId = SubModuleData.Select(x => x.SubModuleId).FirstOrDefault();

                List<Question> Question = (from txnQues in DBEntities.txnQuestions
                                           join mstQues in DBEntities.mstQuestions
                                           on txnQues.QuestionId equals mstQues.QuestionId
                                           where txnQues.TestId == TestId && txnQues.ModuleId == setId && mstQues.SubModuleId == SubModuleId
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

        public Tuple<int, int> GetNoOfQuestionComplete(int TestId)
        {
            try
            {
                CandidateBM UserModel = UserSvc.GetCandidateData(TestId);

                int? TotalQuestion = DBEntities.mstAssessmentSets.Where(x => x.AssessmentId == UserModel.ModuleId).Select(x => x.TotalQuestion).SingleOrDefault();

                List<int> MisTyping =
                new List<int> {
                    (int)AssessmentModule.H1PartAMistyping,
                    (int)AssessmentModule.Mistyping
                };

                if (DBEntities.txnQuestions.Any(x => x.TestId == TestId && MisTyping.Contains(x.ModuleId.Value)))
                {
                    TotalQuestion += DBEntities.txnQuestions.Where(x => x.TestId == TestId && MisTyping.Contains(x.ModuleId.Value)).Count();
                }


                int CompleteQuestion = 0;

                CompleteQuestion = DBEntities.txnQuestions.Where(x => x.TestId == TestId && x.IsAnswer == true).Count();

                return new Tuple<int, int>(TotalQuestion.Value, CompleteQuestion);
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
                int MainTypeId = DBEntities.txnDynamicMisTypings.Where(x => x.TestId == TestId && x.Type == (int)enumType.MainType && x.status == "C").OrderByDescending(x => x.MisTypingId).Select(x => x.MisTypeId.Value).FirstOrDefault();

                lstSubTypeWiseScoreCard = DBEntities.usp_SubTypeWiseScoreBoard(TestId, 2, MainTypeId).Select(x => new ClsTypeModel
                {
                    TypeId = x.SubTypeId.HasValue ? x.SubTypeId.Value : 0,
                    TypeName = x.SubTypeName,
                    Score = x.ImpactScore.HasValue ? x.ImpactScore.Value : 0
                }).ToList();

                int TopScoreType = lstSubTypeWiseScoreCard.OrderByDescending(x => x.Score).Select(x => x.TypeId).FirstOrDefault();

                lstSubTypeWiseScoreCard = new List<ClsTypeModel>();
                lstSubTypeWiseScoreCard = DBEntities.usp_SubTypeWiseScoreBoard(TestId, 5, null).Select(x => new ClsTypeModel
                {
                    TypeId = x.SubTypeId.HasValue ? x.SubTypeId.Value : 0,
                    TypeName = x.SubTypeName,
                    Score = x.ImpactScore.HasValue ? x.ImpactScore.Value : 0
                }).OrderByDescending(x => x.Score).ToList();
                string SubTypeName = string.Empty;
                if (lstSubTypeWiseScoreCard.ElementAt(0).Score == lstSubTypeWiseScoreCard.ElementAt(1).Score)
                {

                    if ((lstSubTypeWiseScoreCard.ElementAt(0).TypeName.ToUpper() == "SP" && lstSubTypeWiseScoreCard.ElementAt(1).TypeName.ToUpper() == "1-O-1") || (lstSubTypeWiseScoreCard.ElementAt(1).TypeName.ToUpper() == "SP" && lstSubTypeWiseScoreCard.ElementAt(0).TypeName.ToUpper() == "1-O-1"))
                    {
                        SubTypeName = "1O1";
                    }
                    else if ((lstSubTypeWiseScoreCard.ElementAt(0).TypeName.ToUpper() == "SO" && lstSubTypeWiseScoreCard.ElementAt(1).TypeName.ToUpper() == "1-O-1") || (lstSubTypeWiseScoreCard.ElementAt(1).TypeName.ToUpper() == "SO" && lstSubTypeWiseScoreCard.ElementAt(0).TypeName.ToUpper() == "1-O-1"))
                    {
                        SubTypeName = "1O1";
                    }
                    else if ((lstSubTypeWiseScoreCard.ElementAt(0).TypeName.ToUpper() == "SP" && lstSubTypeWiseScoreCard.ElementAt(1).TypeName.ToUpper() == "SO") || (lstSubTypeWiseScoreCard.ElementAt(1).TypeName.ToUpper() == "SP" && lstSubTypeWiseScoreCard.ElementAt(0).TypeName.ToUpper() == "SO"))
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
                string ExamStatus = DBEntities.txnExamSetStatus.Where(x => x.TestId == TestId && x.ModuleId == SetId).Select(x => x.Status).FirstOrDefault();

                return ExamStatus;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public int GetCompetenciesCalculation(int TypeId, int SumOfScore, int TestId, int SetId)
        {
            string ColourCode = string.Empty;
            int FinalCalcuationOfScore = 0;
            int NumberOfQuestion = 0;
            int? WeightOfCompetencies;
            CandidateBM UserModel = UserSvc.GetCandidateData(TestId);


            NumberOfQuestion = DBEntities.txnQuestions.Join(DBEntities.mstQuestions,
                                                       x => x.QuestionId, y => y.QuestionId, (x, y) => new { x, y })
                                                       .Where(i => i.x.TestId == TestId && i.x.ModuleId == SetId && i.y.TypeId == TypeId).Count();

            NumberOfQuestion = NumberOfQuestion * 10;
            WeightOfCompetencies = DBEntities.txnCompetencyByCompanies.Where(x => x.TypeId == TypeId && x.CompanyId == UserModel.CompanyId).Select(x => x.Score).FirstOrDefault();
            //  decimal CalculateScore = decimal.Divide(SumOfScore, NumberOfQuestion) * WeightOfCompetencies.Value;
            //  FinalCalcuationOfScore = CalculateScore == 0 ? 0 : (int) Math.Round(CalculateScore);
            decimal CalculateScore = decimal.Divide(SumOfScore, NumberOfQuestion) * 100;
            FinalCalcuationOfScore = CalculateScore == 0 ? 0 : (int)Math.Round(CalculateScore);
            return FinalCalcuationOfScore;
        }

        public int GetClusterId(int TypeId)
        {
            int ClusterId = DBEntities.txnClusterMapToCompetencies.Where(x => x.TypeId == TypeId).Select(x => x.ClusterId.Value).FirstOrDefault();

            return ClusterId;
        }

        public double CalculateOfCompetencies(int TypeId, int SumOfScore, int TestId, int SetId)
        {

            string ColourCode = string.Empty;
            double FinalCalcuationOfScore = 0;
            int NumberOfQuestion = 0;
            int? WeightOfCompetencies;
            CandidateBM UserModel = UserSvc.GetCandidateData(TestId);

            if (SetId == (int)AssessmentModule.Competency)
            {
                NumberOfQuestion = DBEntities.txnQuestions.Join(DBEntities.mstQuestions,
                                                   x => x.QuestionId, y => y.QuestionId, (x, y) => new { x, y })
                                                   .Where(i => i.x.TestId == TestId && i.x.ModuleId == SetId && i.y.TypeId == TypeId).Count();

                NumberOfQuestion = NumberOfQuestion * 10;

                WeightOfCompetencies = DBEntities.txnClusterMapToCompetencies.Where(x => x.TypeId == TypeId).Select(x => x.Weightage).FirstOrDefault();
                decimal CalculateScore = decimal.Divide(SumOfScore, NumberOfQuestion) * WeightOfCompetencies.Value;
                FinalCalcuationOfScore = decimal.ToDouble(CalculateScore);
            }
            else if (SetId == (int)AssessmentModule.QTamCompetency)
            {
                NumberOfQuestion = DBEntities.txnQuestions.Join(DBEntities.mstQuestions,
                                                     x => x.QuestionId, y => y.QuestionId, (x, y) => new { x, y })
                                                     .Where(i => i.x.TestId == TestId && i.x.ModuleId == SetId && i.y.TypeId == TypeId).Count();

                NumberOfQuestion = NumberOfQuestion * 10;

                WeightOfCompetencies = DBEntities.txnClusterMapToCompetencies.Where(x => x.TypeId == TypeId).Select(x => x.WeightageScore).FirstOrDefault();

              //  decimal CalculateScore = WeightOfCompetencies.Value * SumOfScore / NumberOfQuestion; //decimal.Divide(Score, SumOfAllCompent) * 100;
              //  FinalCalcuationOfScore = (int)Math.Round(decimal.ToDouble(CalculateScore));

                decimal CalculateScore = decimal.Divide(SumOfScore, NumberOfQuestion) * WeightOfCompetencies.Value;
                FinalCalcuationOfScore = decimal.ToDouble(CalculateScore);
            }
            //else if(SetId == (int)AssessmentModule.QTamCompetency)
            //{
            //    NumberOfQuestion = DBEntities.txnQuestions.Join(DBEntities.mstQuestions,
            //                                       x => x.QuestionId, y => y.QuestionId, (x, y) => new { x, y })
            //                                       .Where(i => i.x.TestId == TestId && i.x.ModuleId == SetId && i.y.TypeId == TypeId).Count();

            //    NumberOfQuestion = NumberOfQuestion * 10;

            //    decimal CalculateScore = decimal.Divide(SumOfScore, NumberOfQuestion) * 100;

            //    FinalCalcuationOfScore = decimal.ToDouble(CalculateScore);
            //}

            return FinalCalcuationOfScore;
        }
        public void SaveScoreImgOnAws(ClsNextQuestionModel ObjScoreModel,string hostname)
        {
            try
            {
                // byte[] imageBytes=new byte[];
                int i = 0;
                foreach (var Image in ObjScoreModel.ImgByte)
                {
                    i++;
                    byte[] imageBytes = Convert.FromBase64String(Image.Replace("data:image/png;base64,", ""));

                    string FName;
                    if(ObjScoreModel.currentSetId == 14)
                    {
                        FName = ObjScoreModel.currentSetId + "_" + i;
                    }
                    else if(ObjScoreModel.currentSetId == 15)
                    {
                        FName = ObjScoreModel.currentSetId + "_" + i;
                    }
                    else
                    {
                        FName = ObjScoreModel.currentSetId.ToString();
                    }

                    string FileName = ObjScoreModel.TestId + "_" + FName + ".png";


                    if(hostname.Equals("localhost"))
                    {
                        using (var fs = new FileStream(@"E:\Shared\Data\"+FileName, FileMode.Create, FileAccess.Write))
                        {
                            fs.Write(imageBytes, 0, imageBytes.Length);
                        }
                    }
                    else
                    {
                        string accessKey = "AKIAYNH52N4VGKFTABAE";
                        string secretKey = "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";
                        string bucketName = "h1modulewisescorecard";

                        using (MemoryStream memStream = new MemoryStream())
                        {
                            memStream.Write(imageBytes, 0, imageBytes.Length);

                            AmazonS3Client s3 = new AmazonS3Client(new BasicAWSCredentials(accessKey, secretKey), Amazon.RegionEndpoint.APSouth1);

                            using (Amazon.S3.Transfer.TransferUtility tranUtility =
                                         new Amazon.S3.Transfer.TransferUtility(s3))
                            {
                                tranUtility.Upload(memStream, bucketName, FileName);
                                tranUtility.Dispose();
                            }
                            s3.Dispose();
                            memStream.Close();
                        }
                    }
                }
             //   byte[] imageBytes = Convert.FromBase64String(ObjScoreModel.ImgByte.Replace("data:image/png;base64,", ""));
              


            }
            catch
            {
                throw;
            }
        }


        public string GetConsistencyStatus(int TestId)
        {
            List<int> lstQuestionNo1 = new List<int>() { 1034, 1040 };
            var QuestionNo1 = DBEntities.txnQuestions.Where(x => x.TestId == TestId && lstQuestionNo1.Contains(x.QuestionId.Value)).OrderBy(x => x.QuestionId).Select(x => x.ImpactScore).ToList();

            string QuestionResponseno1 = string.Join(",", QuestionNo1);

            List<int> lstQuestionNo2 = new List<int>() { 1035, 1041 };
            var QuestionNo2 = DBEntities.txnQuestions.Where(x => x.TestId == TestId && lstQuestionNo2.Contains(x.QuestionId.Value)).OrderBy(x => x.QuestionId).Select(x => x.ImpactScore).ToList();
            string QuestionResponseno2 = string.Join(",", QuestionNo2);

            List<int> lstQuestionNo3 = new List<int>() { 1036, 1042 };
            var QuestionNo3 = DBEntities.txnQuestions.Where(x => x.TestId == TestId && lstQuestionNo3.Contains(x.QuestionId.Value)).OrderBy(x => x.QuestionId).Select(x => x.ImpactScore).ToList();
            string QuestionResponseno3 = string.Join(",", QuestionNo3);
            List<string> InConsistentPairs = new List<string>() { "9,1", "9,3", "9,5", "7,3", "7,1", "5,9", "5,1", "3,9", "3,7", "1,5", "1,7", "1,9" };

            List<string> ConsistentPairs = new List<string>() { "9,9", "9,7", "7,9", "7,7", "7,5", "5,7", "5,5", "5,3", "3,5", "3,3", "3,1", "1,3", "1,1" };

            //string QuesNo1Status, QuesNo2Status, QuesNo3Status;

            List<string> lstConsistentStatus = new List<string>();



            if (ConsistentPairs.Any(x => x.Contains(QuestionResponseno1)))
            {
                lstConsistentStatus.Add("C");
            }
            else
            {
                lstConsistentStatus.Add("I");
            }

            if (ConsistentPairs.Any(x => x.Contains(QuestionResponseno2)))
            {
                lstConsistentStatus.Add("C");
            }
            else
            {
                lstConsistentStatus.Add("I");
            }

            if (ConsistentPairs.Any(x => x.Contains(QuestionResponseno3)))
            {
                lstConsistentStatus.Add("C");
            }
            else
            {
                lstConsistentStatus.Add("I");
            }


            int ConsistentCount = lstConsistentStatus.Where(x => x == "C").Count();
            int InConsistentCount = lstConsistentStatus.Where(x => x == "I").Count();


            if (ConsistentCount == 3)
            {
                return "High";
            }
            else if (InConsistentCount == 3)
            {
                return "Low";
            }
            else if (InConsistentCount == 2)
            {
                return "Low";
            }
            else if (ConsistentCount == 2)
            {
                return "Moderate";
            }

            return "";
        }


        public void UploadPdfOnS3Bucket(byte[] PdfByteArray, int TestId)
        {
            try
            {
                // byte[] imageBytes = Convert.FromBase64String(ObjScoreModel.ImgByte.Replace("data:image/png;base64,", ""));
                string FileName = TestId + "_QESAR" + ".pdf";
                string accessKey = "AKIAYNH52N4VGKFTABAE";
                string secretKey = "ZqSQpsOs3oh1cB2AJFPOSh2VEcwW+iAMpgqoy1zy";
                string bucketName = "qesarreport";

                using (MemoryStream memStream = new MemoryStream())
                {
                    memStream.Write(PdfByteArray, 0, PdfByteArray.Length);

                    AmazonS3Client s3 = new AmazonS3Client(new BasicAWSCredentials(accessKey, secretKey), Amazon.RegionEndpoint.APSouth1);

                    using (Amazon.S3.Transfer.TransferUtility tranUtility =
                                 new Amazon.S3.Transfer.TransferUtility(s3))
                    {
                        tranUtility.Upload(memStream, bucketName, FileName);
                        tranUtility.Dispose();
                    }
                    s3.Dispose();
                    memStream.Close();
                }


            }
            catch
            {
                throw;
            }
        }

        public Tuple<int, string> GetFileNameForStandardReport(int TestId)
        {
            int MainType = 0, ActionCenter = 0, FellingCenter = 0, ThinkingCenter = 0;

            List<int> lstTypeId = new List<int>();

            MainType = DBEntities.txnDynamicMisTypings.Where(x => x.TestId == TestId && x.Type == 1).OrderByDescending(x => x.MisTypingId).Select(x => x.HighestType.Value).FirstOrDefault();



            FellingCenter = DBEntities.txnDynamicMisTypings.Where(x => x.TestId == TestId && x.Type == 2).OrderByDescending(x => x.MisTypingId).Select(x => x.HighestType.Value).FirstOrDefault();

            if (FellingCenter != MainType)
            {
                lstTypeId.Add(FellingCenter);
            }

            ThinkingCenter = DBEntities.txnDynamicMisTypings.Where(x => x.TestId == TestId && x.Type == 3).OrderByDescending(x => x.MisTypingId).Select(x => x.HighestType.Value).FirstOrDefault();

            if (ThinkingCenter != MainType)
            {
                lstTypeId.Add(ThinkingCenter);
            }

            ActionCenter = DBEntities.txnDynamicMisTypings.Where(x => x.TestId == TestId && x.Type == 4).OrderByDescending(x => x.MisTypingId).Select(x => x.HighestType.Value).FirstOrDefault();

            if (ActionCenter != MainType)
            {
                lstTypeId.Add(ActionCenter);
            }

            var TypeIdOrder = lstTypeId.OrderBy(x => x).ToList();

            string fileName = MainType + "_" + TypeIdOrder.ElementAt(0) + "_" + TypeIdOrder.ElementAt(1);




            var TypeId = new Tuple<int, string>(MainType, fileName);

            return TypeId;
        }


        public List<ClsTypeModel> GetScoreCardOfCompentency(int TestId,int SetId)
        {
            List<int> _lstConsentency = new List<int> { 194, 177 };

            List<ClsTypeModel> CompentencyScoreCard = (from ScoreObject in GetTypeWiseScoreBoard(TestId, SetId)
                                                       where !_lstConsentency.Contains(ScoreObject.TypeId)
                                                       select new ClsTypeModel
                                                       {
                                                           TypeId = ScoreObject.TypeId,
                                                           TypeName = ScoreObject.TypeName,
                                                           Score = CalcuateIndividualCompetencies(ScoreObject.TypeId, TestId, SetId, ScoreObject.Score),
                                                           ColorCode = ""
                                                       }).ToList();

            return CompentencyScoreCard;
        }



        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                // Console.WriteLine("This is the first call to Dispose. Necessary clean-up will be done!");

                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    // Console.WriteLine("Explicit call: Dispose is called by the user.");
                }
                else
                {
                    // Console.WriteLine("Implicit call: Dispose is called through finalization.");
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // Console.WriteLine("Unmanaged resources are cleaned up here.");

                // TODO: set large fields to null.

                disposedValue = true;
            }
            else
            {
                // Console.WriteLine("Dispose is called more than one time. No need to clean up!");
            }
        }



        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }

        public List<ClsSet6ScoreModel> lstSubModuleScore(int Testid, int setid)
        {
            throw new NotImplementedException();
        }
        #endregion

    }
}