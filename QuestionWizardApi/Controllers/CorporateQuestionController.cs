using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using System.Threading.Tasks;
using QuestionWizardApi.CorporateIBusinessLayer.Interface;
using QuestionWizardApi.CorporateModel.Model;
using System.Net.Http;
using System.Net;
using static QuestionWizardApi.Models.RequireHttpsAttribute;
using System.Net.Http.Headers;
using System.Threading;

namespace QuestionWizardApi.Areas.CorporateAssessment.Controllers
{
    [RoutePrefix("api/CorporateQuestion")]
    [CustomAuthorizeAttribute]
    public class CorporateQuestionController : ApiController
    { 
        private IUser UserSrv = null;
        private IQuestion QuesSrv = null;
        private IMail MailSrv = null;
        private IMailH1 _MailH1Srv = null;
        private IReportGeneration ReportSvc = null;

        public CorporateQuestionController(IQuestion QuesSrv, IUser UserSrv, IMail MailSrv, IMailH1 _MailH1Srv, IReportGeneration ReportSvc)
        {
            this.QuesSrv = QuesSrv;
            this.UserSrv = UserSrv;
            this.MailSrv = MailSrv;
            this._MailH1Srv = _MailH1Srv;
            this.ReportSvc = ReportSvc;
        }

        
        [HttpGet]
       
        //  [CacheFilter(TimeDuration = 100)]
        [Route("LoadInitialQuestionModel/{TestId}/{SetId}")]
        public async Task<IHttpActionResult> LoadInitialQuestionModel(int TestId, int SetId)
        {
            try
            {
                ClsQuestionModel QuestionModel = new ClsQuestionModel();
                QuestionModel = await QuesSrv.LoadQuestionModel(TestId, SetId, null);
                
                return Ok(new { isSucess = true, QuestionModel = QuestionModel });
            }
            catch (Exception ex)
            {

                //throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message));
              throw new NotImplementedException(ex.Message);
               //   return BadRequest(ex.Message.ToString());
            }
            finally
            {
                this.QuesSrv.Dispose();
            }
        }

        [HttpPost]
        [Route("SaveLoadNextQuestion")]
        public async Task<IHttpActionResult> SaveLoadNextQuestion(ClsQuestionModel QuestionModel)
        {
            try
            {
                ClsQuestionModel NextQuestionModel = new ClsQuestionModel();
                
                if (QuestionModel.CurrentSetId == (int)AssessmentModule.H1PartAAptitude)
                {
                    if(QuestionModel.lstQuestionModel[0].ResponseValue != null)
                    {
                        QuesSrv.SaveQuestion(QuestionModel.lstQuestionModel);
                    }
                    NextQuestionModel = await QuesSrv.LoadQuestionModel(QuestionModel.TestId, QuestionModel.CurrentSetId, QuestionModel.CurrentQuestionId);

                }
                else if(QuestionModel.CurrentSetId == (int)AssessmentModule.EnneagramInstincts || QuestionModel.CurrentSetId == (int)AssessmentModule.CenterOfExpression)
                {
                    QuesSrv.SubmitDataInRearrangeOrder(QuestionModel.lstQuestionModel);
                    NextQuestionModel = await QuesSrv.LoadQuestionModel(QuestionModel.TestId, QuestionModel.CurrentSetId, null);
                }
                else
                {
                    QuesSrv.SaveQuestion(QuestionModel.lstQuestionModel);
                    NextQuestionModel = await QuesSrv.LoadQuestionModel(QuestionModel.TestId, QuestionModel.CurrentSetId, null);
                }
                

                return Ok(new { isSucess = true, QuestionModel = NextQuestionModel });
            }
            catch (Exception ex)
            {
                throw new NotImplementedException(ex.Message);
                // return BadRequest(ex.Message.ToString());
            }
            finally
            {
                this.QuesSrv.Dispose();
            }
        }


        [HttpPost]
        [Route("LoadCurrentQuestion")]
        public async Task<IHttpActionResult> LoadCurrentQuestion(ClsQuestionModel QuestionModel)
        {
            try
            {
                ClsQuestionModel NextQuestionModel = new ClsQuestionModel();

                NextQuestionModel = await QuesSrv.LoadQuestionModel(QuestionModel.TestId, QuestionModel.CurrentSetId, QuestionModel.CurrentQuestionId);

                return Ok(new { isSucess = true, QuestionModel = NextQuestionModel });
            }
            catch (Exception ex)
            {
                throw new NotImplementedException(ex.Message);
                // return BadRequest(ex.Message.ToString());
            }
            finally
            {
                this.QuesSrv.Dispose();
            }
        }

        [HttpPost]
        [Route("SubmitSetofQuestion")]
        public IHttpActionResult SubmitSetofQuestion(ClsQuestionModel QuestionModel)
        {
            try
            {
               if(QuestionModel.CurrentSetId == (int)AssessmentModule.H1PartAMistyping || QuestionModel.CurrentSetId == (int)AssessmentModule.Mistyping)
                {
                    QuesSrv.SaveQuestion(QuestionModel.lstQuestionModel);

                    int TypeId = QuestionModel.lstQuestionModel.GroupBy(x => x.TypeId).Select(x => x.Key.Value).FirstOrDefault();

                     QuesSrv.UpdateMisTypeModule(TypeId, "C", QuestionModel.TestId, QuestionModel.CurrentSetId);

                    if (QuesSrv.CheckAllMistypingModuleCompleted(QuestionModel.TestId))
                    {
                        QuesSrv.UpdateExamStatus(QuestionModel.TestId, QuestionModel.CurrentSetId, "C");

                        QuesSrv.updateMainTypeInDB(QuestionModel.TestId);
                    }
                    else
                    {
                        int RefTypeId = 0;
                        QuesSrv.GenerateQuestionByDynamicMistyping(QuestionModel.CurrentUserId, QuestionModel.CurrentSetId, QuestionModel.TestId, ref RefTypeId);
                        if (RefTypeId > 0)
                        {
                            UserSrv.GenerateRandomQuestionNumber(UserId: QuestionModel.CurrentUserId, TestId: QuestionModel.TestId, TypeId: RefTypeId, hostName: "",AssessmentId:null, ModuleId: QuestionModel.CurrentSetId );
                        }
                        else if (RefTypeId == 0)
                        {
                           
                            QuesSrv.UpdateExamStatus(QuestionModel.TestId, QuestionModel.CurrentSetId, "C");

                            QuesSrv.updateMainTypeInDB(QuestionModel.TestId);
                        }
                    }
                }
                else if (QuestionModel.CurrentSetId == (int)AssessmentModule.EnneagramInstincts || QuestionModel.CurrentSetId == (int)AssessmentModule.CenterOfExpression)
                {
                    QuesSrv.SubmitDataInRearrangeOrder(QuestionModel.lstQuestionModel);
                    QuesSrv.UpdateExamStatus(QuestionModel.TestId, QuestionModel.CurrentSetId, "C");
                }
                else
                {
                    QuesSrv.SaveQuestion(QuestionModel.lstQuestionModel);
                    QuesSrv.UpdateExamStatus(QuestionModel.TestId, QuestionModel.CurrentSetId, "C");
                }
                return Ok(new { isSuccess = true, QuestionModel = QuestionModel });
            }
            catch (Exception ex)
            {
                throw new NotImplementedException(ex.Message);
                //return BadRequest(ex.Message.ToString());
            }
            finally
            {
                this.UserSrv.Dispose();
                this.QuesSrv.Dispose();
            }
        }


        [HttpPost]
        [Route("SaveAndNextSetOpen")]
        public IHttpActionResult SaveAndNextSetOpen(ClsNextQuestionModel ObjNextQuestionModel)
        {
            try
            {
                string hostname = System.Web.HttpContext.Current.Request.Url.Host;


                QuesSrv.SaveScoreImgOnAws(ObjNextQuestionModel, hostname);

                UserSrv.GenerateRandomQuestionNumber(UserId: ObjNextQuestionModel.UserId, TestId: ObjNextQuestionModel.TestId, TypeId: ObjNextQuestionModel.Typeid, hostName: "", AssessmentId: null, ModuleId: ObjNextQuestionModel.SetId);

                return Ok(new { isSuccess = true });
            }
            catch (Exception ex)
            {
                throw new NotImplementedException(ex.Message);
                //return BadRequest(ex.Message.ToString());
            }
            finally
            {
                this.UserSrv.Dispose();
                this.QuesSrv.Dispose();
            }
        }


        [HttpGet]
     //   [CacheFilter(TimeDuration = 100)]
        [Route("GetQuestionSetStatusCode/{TestId}")]
        public async Task<IHttpActionResult> GetQuestionSetStatusCode(int TestId)
        {
            try
            {
                List<ClsQuestionSetStatusCode> lstExamStatusCode = new List<ClsQuestionSetStatusCode>();
                int NoOfQuestion =0, NoOfQuestionComplete=0, SetId=0;

                //  lstExamStatusCode = QuesSrv.GetExamSetStatusCode(TestId);
                SetId = UserSrv.GetProgressExamSet(TestId);
                
                await Task.Run(() =>
                {
                   
                    NoOfQuestion = QuesSrv.GetNoOfQuestionComplete(TestId).Item1;
                    NoOfQuestionComplete = QuesSrv.GetNoOfQuestionComplete(TestId).Item2;

                });
                return Ok(new { isSuccess = true,
                                ExamStatusCode = lstExamStatusCode,
                                NoOfQuestion = NoOfQuestion,
                                NoOfQuestionComplete = NoOfQuestionComplete,
                                ProgressSetId = SetId
                });

            }
            catch (Exception ex)
            {
                throw new NotImplementedException(ex.Message);
                //return BadRequest(ex.Message.ToString());
            }
            finally
            {
                this.UserSrv.Dispose();
                this.QuesSrv.Dispose();
            }
        }

        [HttpGet]
        [Route("GetFileNameBaseOnStandardReport/{TestId}")]
        public IHttpActionResult GetFileNameForStan(int TestId)
        {
            var Data = _MailH1Srv.GetFileName(TestId);

            return Ok(new { Data.Item1, Data.Item2 });

        }
        [HttpPost]
      //  [CacheFilter(TimeDuration = 100)]
        [Route("CompleteCorporateTest")]
        public async Task<IHttpActionResult> CompleteUserTestAsync(ClsNextQuestionModel ObjNextQuestionModel)
        {
            try
            {
                string hostname = System.Web.HttpContext.Current.Request.Url.Host;

                QuesSrv.SaveScoreImgOnAws(ObjNextQuestionModel, hostname);

                CandidateBM UserModel = UserSrv.GetCandidateData(ObjNextQuestionModel.TestId);

                HumanResourceRepo ObjHr = UserSrv.GetHumanResource(UserModel.CompanyId);

                string Name = char.ToUpper(UserModel.FirstName[0]) + UserModel.FirstName.Substring(1) + " " + char.ToUpper(UserModel.LastName[0]) + UserModel.LastName.Substring(1);

                ReportSvc.GenerateCandidateReport(ObjNextQuestionModel.TestId, UserModel.ModuleId, Name, hostname);


                if(hostname.Equals("localhost"))
                {
                    QuesSrv.CompeteUserTest(ObjNextQuestionModel.TestId, ObjNextQuestionModel.UserId, "C");

                    return Ok(new { isSuccess = true });
                }

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://localhost:7298/");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    //GET Method
                    HttpResponseMessage response = await client.GetAsync("api/EmailSender/FinalEmailSend/" + ObjNextQuestionModel.TestId + "/" + UserModel.ModuleId);
                    if (response.IsSuccessStatusCode)
                    {
                        var responseJsonString =  response.Content.ReadAsStringAsync().Result;




                        /* comment by  on 03 May 2024

                        TempMailBody objMailContent = Newtonsoft.Json.JsonConvert.DeserializeObject<TempMailBody>(responseJsonString);


                        await Task.Run(async () =>
                        {
                            ReportSvc.GenerateCandidateReport(ObjNextQuestionModel.TestId, UserModel.ModuleId, objMailContent.CandidateModel.RecevierName)
                                    .ContinueWith(x => _MailH1Srv.FinalSentMail(objMailContent.CandidateModel, ObjNextQuestionModel.TestId.ToString(),
                                                        objMailContent.CandidateModel.SendToCandidate))
                                    .ContinueWith(y => _MailH1Srv.FinalSentMail(objMailContent.HrModel, ObjNextQuestionModel.TestId.ToString(),
                                                       objMailContent.HrModel.SendToHr));
                        });

                        */

                        //var task = ReportSvc.GenerateCandidateReport(ObjNextQuestionModel.TestId, UserModel.ModuleId, objMailContent.CandidateModel.RecevierName)
                        //            .ContinueWith(x => _MailH1Srv.FinalSentMail(objMailContent.CandidateModel, ObjNextQuestionModel.TestId.ToString(),
                        //                                objMailContent.CandidateModel.SendToCandidate))
                        //            .ContinueWith(y => _MailH1Srv.FinalSentMail(objMailContent.HrModel, ObjNextQuestionModel.TestId.ToString(),
                        //                               objMailContent.HrModel.SendToHr));
                        //task.Wait();


                       
                        QuesSrv.CompeteUserTest(ObjNextQuestionModel.TestId, ObjNextQuestionModel.UserId, "C");

                        return Ok(new { isSuccess = true });
                    }
                }


                //MailBM objMailBody = _MailH1Srv.GetFinalSenderBody(UserModel.ProfileId.Value, UserModel.CompanyId);

                //if(UserModel.ModuleId == 1 || UserModel.ModuleId == 2 || UserModel.ModuleId == 5 || UserModel.ModuleId == 6 || UserModel.ModuleId == 9 || UserModel.ModuleId == 10 || UserModel.ModuleId == 11)
                //{
                //    objMailBody.RecevierName = Name;
                //    if (objMailBody != null)
                //    {
                //        Task.Run(async () =>
                //        {
                //            try
                //            {
                //                await ReportSvc.GenerateCandidateReport(ObjNextQuestionModel.TestId, UserModel.ModuleId, objMailBody.RecevierName);
                //                await _MailH1Srv.FinalSentMail(objMailBody, ObjNextQuestionModel.TestId.ToString(), ObjHr.IsReportSentToHr.Value);
                //            }
                //            catch (Exception ex)
                //            {
                //                throw;
                //            }
                //        });

                //        //Task.Factory.StartNew(() =>
                //        //{
                //        //    ReportSvc.GenerateCandidateReport(ObjNextQuestionModel.TestId, UserModel.ModuleId, objMailBody.RecevierName);
                //        //}).ContinueWith(x =>
                //        //{
                //        //    _MailH1Srv.FinalSentMail(objMailBody, ObjNextQuestionModel.TestId.ToString(), ObjHr.IsReportSentToHr.Value);
                //        //});
                //        //Task.WhenAll(ReportSvc.GenerateCandidateReport(ObjNextQuestionModel.TestId, UserModel.ModuleId, objMailBody.RecevierName),
                //        //    _MailH1Srv.FinalSentMail(objMailBody, ObjNextQuestionModel.TestId.ToString(), ObjHr.IsReportSentToHr.Value)
                //        //    );


                //        // Task.Run(async () => await _MailH1Srv.FinalSentMail(objMailBody, ObjNextQuestionModel.TestId.ToString(), ObjHr.IsReportSentToHr.Value));
                //    }
                //}
                //else if(UserModel.ModuleId == 3 || UserModel.ModuleId == 4)
                //{
                //    objMailBody = _MailH1Srv.GetFinalSenderBodyToCandidate(UserModel.ProfileId.Value, UserModel.UserId);
                //    if (objMailBody != null)
                //    {
                //        Task.Run(async () =>
                //        {
                //            await ReportSvc.GenerateCandidateReport(ObjNextQuestionModel.TestId, UserModel.ModuleId, objMailBody.RecevierName);
                //            await _MailH1Srv.FinalSentMail(objMailBody, ObjNextQuestionModel.TestId.ToString(), false);
                //        });

                //        //Task.WhenAll(ReportSvc.GenerateCandidateReport(ObjNextQuestionModel.TestId, UserModel.ModuleId, objMailBody.RecevierName),
                //        //    _MailH1Srv.FinalSentMail(objMailBody, ObjNextQuestionModel.TestId.ToString(), false)
                //        //    );
                //        //_MailH1Srv.FinalSentMail(objMailBody, ObjNextQuestionModel.TestId.ToString(), false);
                //        // Task.Run(async () => await _MailH1Srv.FinalSentMail(objMailBody, ObjNextQuestionModel.TestId.ToString(), false));
                //    }
                //}


                return Ok();
                
            }
            catch (Exception ex)
            {
                throw new NotImplementedException(ex.Message);
                //return BadRequest(ex.Message.ToString());
            }
            finally
            {
                this.MailSrv.Dispose();
                this.QuesSrv.Dispose();
            }
        }

      


        

    }
}