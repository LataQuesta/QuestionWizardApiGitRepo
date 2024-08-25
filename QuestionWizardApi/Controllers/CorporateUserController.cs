using QuestionWizardApi.CorporateIBusinessLayer.Interface;
using QuestionWizardApi.CorporateModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using static QuestionWizardApi.Models.RequireHttpsAttribute;

namespace QuestionWizardApi.Areas.CorporateAssessment.Controllers
{
   // [RoutePrefix("api/CorporateUser")]
    public class CorporateUserController : ApiController
    {
        
        private IMasterData MstUserSrv = null; 
        private IUser UserSrv = null;
        private IMail MailSrv = null;
        private IMailH1 MailH1Srv = null;

        public CorporateUserController(IMasterData MstUserSrv, IUser UserSrv, IMail MailSrv, IMailH1 MailH1Srv)
        {
            this.MstUserSrv = MstUserSrv;
            this.UserSrv = UserSrv;
            this.MailSrv = MailSrv;
            this.MailH1Srv = MailH1Srv;
        }

        [HttpGet]
      //[CustomAuthorizeAttribute]
        [Route("api/CorporateUser/GetUserClaim")]
        public IHttpActionResult GetuserClaim()
        {
            try
            {
                var identity = (ClaimsIdentity)User.Identity;
                if(identity != null)
                {
                    //string emailId = UserSrv.GetEmailId()//identity.FindFirst("username").Value;
                    int CurrentTestId = !string.IsNullOrEmpty(identity.FindFirst("username").Value) ? Convert.ToInt32(identity.FindFirst("username").Value) : 0; //UserSrv.GetLatestTestId(emailId);

                    var CandidateData = UserSrv.GetCandidateData(CurrentTestId);


                    string emailId = CandidateData.UserEmail;
                    string Name = CandidateData.FirstName;

                    int CurrentSetId = UserSrv.GetLatestSetId(CurrentTestId);
                    // var roles = identity.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value);

                    var UserDetail = new
                    {
                        Username = emailId,//identity.FindFirst("username").Value,
                        TestId = CurrentTestId,
                        SetId = CurrentSetId,
                        Name = Name
                    };
                    return Ok(new { userAuth = UserDetail });
                }
                else
                {
                    return NotFound();
                }
                
            }
            
            catch (Exception ex)
            {
                throw new NotImplementedException(ex.Message);
            }
            finally
            {
                this.UserSrv.Dispose();
            }
        }
        
        [HttpGet]
        [Route("api/CorporateUser/GetState/{CountryId}")]
        public async Task<IHttpActionResult> GetState(int CountryId)
        {
            try
            {
                List<StateBM> lstState = await MstUserSrv.GetState(CountryId);

                return Ok(new { State = lstState });
            }
            catch (Exception ex)
            {
                throw new NotImplementedException(ex.Message);
            }
            finally
            {
                this.MstUserSrv.Dispose();
            }
        }

      
        [HttpGet]
        [NonAction]
        [Route("api/Questa/AssessmentLink/{ProfileId}/{CompanyId}/{AssessmentId}")]
       // [Route("api/Questa/LinkGeneration/{ProfileId}/{CompanyId}/{AssessmentId}")]
        public IHttpActionResult LinkGeneration(int ProfileId,int CompanyId,int AssessmentId)
        {
            try
            {
                //int CompanyId = 2;
                if (!UserSrv.IsProfileIdExists(ProfileId))
                {
                    var response = new HttpResponseMessage(HttpStatusCode.NotFound)
                    {
                        Content = new StringContent("Please select valid profile Id. Please contact to developer", System.Text.Encoding.UTF8, "text/plain"),
                        StatusCode = HttpStatusCode.NotFound
                    };
                    throw new HttpResponseException(response);
                }
                else if (!UserSrv.IsValidCompany(CompanyId))
                {
                    var response = new HttpResponseMessage(HttpStatusCode.NotFound)
                    {
                        Content = new StringContent("Company not exits in database. Please contact to developer", System.Text.Encoding.UTF8, "text/plain"),
                        StatusCode = HttpStatusCode.NotFound
                    };
                    throw new HttpResponseException(response);
                }
                else if (!UserSrv.IsValidAssessmentSet(AssessmentId))
                {
                    var response = new HttpResponseMessage(HttpStatusCode.NotFound)
                    {
                        Content = new StringContent("Assessment Set not exits in database. Please contact to developer", System.Text.Encoding.UTF8, "text/plain"),
                        StatusCode = HttpStatusCode.NotFound
                    };
                    throw new HttpResponseException(response);
                }
                else
                {
                    HumanResourceRepo ObjHr = UserSrv.GetHumanResource(CompanyId);
                    string url = "EmailFlg_" + System.Web.HttpContext.Current.Request.Url.Host;
                    string QuestionUrl = UserSrv.GetUrlValue(url);

                    string TempURL = string.Empty;
                    
                    if (ObjHr.IsBulkSentRequire.Value)
                    {
                       // QuestionUrl = "<br/>";
                        for (int i =0;i<=ObjHr.LinkCount - 1;i++)
                        {
                            int ExamId = UserSrv.SaveCandidateData(ProfileId, CompanyId, AssessmentId);

                            string URL = QuestionUrl + ExamId;

                            // QuestionUrl = QuestionUrl + ExamId ;
                            TempURL = TempURL + "<a href=" + URL + " target='_self'>" + URL + "</a> <br/>";
                          //  QuestionUrl += ExamId + "<br/>";
                        }
                    }
                    else
                    {
                        int ExamId = UserSrv.SaveCandidateData(ProfileId, CompanyId, AssessmentId);

                        string URL = QuestionUrl + ExamId;

                        // QuestionUrl = QuestionUrl + ExamId ;
                        TempURL = TempURL + "<a href=" + URL + " target='_self'>" + URL + "</a>";
                    }


                    MailBM ObjMailSenderBody = MailH1Srv.GetSenderBody(ProfileId, CompanyId);

                    Task.Run(async () => await this.MailH1Srv.InitialMailSent(ObjMailSenderBody, TempURL));
                    
                    return Ok(new {  URL = TempURL, isSuccess = true });
                }

            }
            catch (Exception ex)
            {
                throw new NotImplementedException(ex.Message);
                //return BadRequest(ex.Message.ToString());
            }
            finally
            {
                this.UserSrv.Dispose();
                this.MailSrv.Dispose();
            }
        }

        [HttpGet]
        [NonAction]
         [Route("api/Questa/CreateLinkBaseOnCompanyId/{ProfileId}/{CompanyId}/{AssessmentId}")]
       // [Route("api/Questa/LinkBaseOnCompanyId/{ProfileId}/{CompanyId}/{AssessmentId}")]
        public IHttpActionResult CreateLinkBaseOnCompanyId(int ProfileId, int CompanyId, int AssessmentId)
        {
            try
            {
                int i = 1;
                dynamic CandidateRecord = UserSrv.GetDummyCandidateRecord();

                foreach(var item in CandidateRecord)
                {
                    HumanResourceRepo ObjHr = UserSrv.GetHumanResource(CompanyId);
                    string url = "EmailFlg_" + System.Web.HttpContext.Current.Request.Url.Host;
                    string QuestionUrl = UserSrv.GetUrlValue(url);

                    string TempURL = string.Empty;

                    int ExamId = UserSrv.SaveCandidateData(ProfileId, CompanyId, AssessmentId);

                    string URL = QuestionUrl + ExamId;

                    // QuestionUrl = QuestionUrl + ExamId ;
                    TempURL = TempURL + "<a href=" + URL + " target='_self'>" + URL + "</a>";

                    MailBM ObjMailSenderBody = MailH1Srv.GetSenderBody(ProfileId, CompanyId);

                    ObjMailSenderBody.RecevierEmailAddress = item.GetType().GetProperty("CandidateEmail").GetValue(item, null);

                    ObjMailSenderBody.RecevierName = item.GetType().GetProperty("CandidateName").GetValue(item, null);

                    this.MailH1Srv.InitialMailSent(ObjMailSenderBody, TempURL);
                    i++;
                }

                return Ok(new {isSuccess = true, LinkCount = i });
            }
            catch (Exception ex)
            {
                throw new NotImplementedException(ex.Message);
            }
            finally
            {
                this.UserSrv.Dispose();
                this.MailSrv.Dispose();
            }
        }

        [HttpGet]
        [NonAction]
        [Route("api/CorporateUser/GenerateModule1To7Link/{Title}/{FirstName}/{LastName}/{email}/{PhoneNumber}/{ProfileId}/{CompanyId}/{ModuleId}")]
        public IHttpActionResult GenerateModule1To7Link(string Title, string FirstName, string LastName, string email, string PhoneNumber, int ProfileId, int CompanyId, int ModuleId)
        {
            try
            {
                //int CompanyId = 2;
                if (string.IsNullOrEmpty(Title))
                {
                    var response = new HttpResponseMessage(HttpStatusCode.NotFound)
                    {
                        Content = new StringContent("Please Select title", System.Text.Encoding.UTF8, "text/plain"),
                        StatusCode = HttpStatusCode.NotFound
                    };
                    throw new HttpResponseException(response);
                }
                else if (string.IsNullOrEmpty(FirstName))
                {
                    var response = new HttpResponseMessage(HttpStatusCode.NotFound)
                    {
                        Content = new StringContent("Please Select First Name", System.Text.Encoding.UTF8, "text/plain"),
                        StatusCode = HttpStatusCode.NotFound
                    };
                    throw new HttpResponseException(response);
                }
                else if (string.IsNullOrEmpty(LastName))
                {
                    var response = new HttpResponseMessage(HttpStatusCode.NotFound)
                    {
                        Content = new StringContent("Please Select Last Name", System.Text.Encoding.UTF8, "text/plain"),
                        StatusCode = HttpStatusCode.NotFound
                    };
                    throw new HttpResponseException(response);
                }
                else if (string.IsNullOrEmpty(email))
                {
                    var response = new HttpResponseMessage(HttpStatusCode.NotFound)
                    {
                        Content = new StringContent("Please Select Email", System.Text.Encoding.UTF8, "text/plain"),
                        StatusCode = HttpStatusCode.NotFound
                    };
                    throw new HttpResponseException(response);
                }
                else if (string.IsNullOrEmpty(PhoneNumber))
                {
                    var response = new HttpResponseMessage(HttpStatusCode.NotFound)
                    {
                        Content = new StringContent("Please Select Phone Number", System.Text.Encoding.UTF8, "text/plain"),
                        StatusCode = HttpStatusCode.NotFound
                    };
                    throw new HttpResponseException(response);
                }
                else if (!UserSrv.IsProfileIdExists(ProfileId))
                {
                    var response = new HttpResponseMessage(HttpStatusCode.NotFound)
                    {
                        Content = new StringContent("Please select valid profile Id. Please contact to developer", System.Text.Encoding.UTF8, "text/plain"),
                        StatusCode = HttpStatusCode.NotFound
                    };
                    throw new HttpResponseException(response);
                }
                else if (!UserSrv.IsValidCompany(CompanyId))
                {
                    var response = new HttpResponseMessage(HttpStatusCode.NotFound)
                    {
                        Content = new StringContent("Company not exits in database. Please contact to developer", System.Text.Encoding.UTF8, "text/plain"),
                        StatusCode = HttpStatusCode.NotFound
                    };
                    throw new HttpResponseException(response);
                }
                else
                {

                    int ExamId = UserSrv.SaveCandidateRecord(Title, FirstName, LastName, email, PhoneNumber, ProfileId, CompanyId, ModuleId);
                    string url = "EmailFlg_" + System.Web.HttpContext.Current.Request.Url.Host;
                    string QuestionUrl = UserSrv.GetUrlValue(url);

                    QuestionUrl += ExamId;


                    MailBM objMailBody = this.MailSrv.GetMailConfig(ExamId, ProfileId, true, false);
                    if (objMailBody != null)
                    {
                        this.MailSrv.SentMail(objMailBody, ExamId.ToString(), QuestionUrl);
                    }

                    return Ok(new { ExamId = ExamId, URL = QuestionUrl, isSuccess = true });
                }

            }
            catch (Exception ex)
            {
                throw new NotImplementedException(ex.Message);
                //return BadRequest(ex.Message.ToString());
            }
            finally
            {
                this.UserSrv.Dispose();
                this.MailSrv.Dispose();
            }
        }

        [HttpGet]
        [Route("api/CorporateUser/GetCandiateData/{TestId}")]
        public IHttpActionResult GetCandiateData(int TestId)
        {
            try
            {
                bool IsDisableAllControl = false;
                CandidateModel objCandidate = new CandidateModel();
                CandidateBM UserModel = this.UserSrv.GetCandidateData(TestId);

                if (UserModel != null)
                {
                    objCandidate =  this.UserSrv.GetCandidateDetails(UserModel, TestId);
                    if (UserModel.UserGender != null)
                    {
                        IsDisableAllControl = true;
                    }
                    return Ok(new { IsSuccess = true, IsDisableAllControl = IsDisableAllControl, CandidateData = objCandidate });
                }
                else
                {
                    return Ok(new { IsSuccess = false });
                }
            }
            catch (Exception ex)
            {
                throw new NotImplementedException(ex.Message);
               // return BadRequest(ex.Message.ToString());
            }
            finally
            {
                this.UserSrv.Dispose();
            }
        }

        [HttpPost]
        [Route("api/CorporateUser/SaveCandidateDetails")]
        public IHttpActionResult SaveCandidateDetails(CorporateModel.Model.CandidateModel objCandidate)
        {
            try
            {
                objCandidate.DateOfBirth = objCandidate.DateOfBirth == null ? (DateTime?)null :  objCandidate.DateOfBirth.Value.AddDays(1);
                if (objCandidate.IsActive)
                {
                    if (UserSrv.Check15DaysValidation(objCandidate.UserId))
                    {
                        return Ok(new { Error = "Please Note : The link you clicked on has expired,as ie was valid for a duration of 15 days from the date of payment Please contact us at support@questa.in for further assistance. ", IsDayPassed = true, isSuccess = false });
                    }
                    else
                    {
                        bool IsSucess = UserSrv.SaveCandidateData(objCandidate);
                        return Ok(new { ExamId = objCandidate.TestId, isSuccess = IsSucess });
                        
                    }
                }
                else
                {
                    return Ok(new { valid = "candidate is not active",Isvalid=true });
                }
               
            }
            catch (Exception ex)
            {
                throw new NotImplementedException(ex.Message);
                //return BadRequest(ex.Message.ToString());
            }
            finally
            {
                this.UserSrv.Dispose();
            }
        }

        [HttpGet]
       // [CacheFilter(TimeDuration = 100)]
        [Route("api/CorporateUser/GetMaster")]
        public async Task<IHttpActionResult> GetMaster()
        {
            try
            {
                MasterBM Obj = await MstUserSrv.GetMasterData();
             //   throw new NullReferenceException("Student object is null.");
                List<CountryBM> lstCountries = Obj.Countries;
                List<QualificationBM> lstQualification = Obj.Qualification;
                List<ProfessionBM> lstProfession = Obj.Profession;
                List<AgeBM> lstAge = Obj.Ages;
                List<GenderBM> lstgender = Obj.Gender;
                List<MaritalStatusBM> lstMaritalStatus = Obj.MaritalStatus;
                List<EmployeeStatusBM> lstEmployeeStatus = Obj.EmployeeStatus;
                List<IndustryBM> Industry = Obj.Industry;
                List<string> lstIndustry = Industry.Select(x => x.IndustryName).ToList();
                List<ExperienceBM> lstExperience = Obj.Experience;

                return  Ok(new { Countries = lstCountries, Qualification = lstQualification,
                                Profession = lstProfession,
                                Age = lstAge,
                                Gender = lstgender,
                                MaritalStatus = lstMaritalStatus,
                                EmployeeStatus = lstEmployeeStatus,
                                Industry = lstIndustry,
                    Experience = lstExperience
                });
            }
            catch (Exception ex)
            {
                throw new NotImplementedException(ex.Message);
            }
            finally
            {
                this.MstUserSrv.Dispose();
            }
        }

        [HttpPost]
        [Route("api/CorporateUser/GetOTP")]
        public async Task<IHttpActionResult> GetOTP(OTPModel ObjOTP)
        {
            try
            {
                Random rnd = new Random();
                int otp = rnd.Next(10000, 99999);
                if (ObjOTP.PhoneNumber.Length <= 10)
                {
                    string SenderNumber = ObjOTP.PhoneNumber.Substring(ObjOTP.PhoneNumber.Length - 10);

                    //string awsAccessKey = "AKIAYNH52N4VNI5BKC4D";
                    //string awsSecretKey = "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";

                    //var awsCredential = new BasicAWSCredentials(awsAccessKey, awsSecretKey);
                    //AmazonSimpleNotificationServiceClient smsClient = new AmazonSimpleNotificationServiceClient(awsCredential, Amazon.RegionEndpoint.APSouth1);
                    //PublishRequest pubRequest = new PublishRequest();
                    //pubRequest.Message = "<#>"+ otp + " is your One Time Verification (OTP) code to confirm your phone no at Questa Enneagram";
                    //pubRequest.Subject = "OTP Auth";
                    //pubRequest.PhoneNumber = SenderNumber;
                    //pubRequest.MessageAttributes.Add("AWS.SNS.SMS.SMSType", new MessageAttributeValue
                    //{ StringValue = "Transactional", DataType = "String" });

                    //PublishResponse pubResponse = smsClient.Publish(pubRequest);

                //    string API = "http://sms.ssdweb.in/api/sendhttp.php?authkey=365516Anl0xCI0kn56110dd2eP1&";

                  //  API = "http://sms.ssdweb.in/api/sendhttp.php?authkey=365516Anl0xCI0kn56110dd2eP1&";
                   // API = "mobiles=" + SenderNumber + "&";

                 //   API = API + "mobiles=" + SenderNumber + "&";
                    string Msg = otp + " is your One Time Verification (OTP) code to attempt the Self-Discovery Assessment at Questa Enneagram.This is valid for a period of 10 minutes.You are one step away from discovering your Enneagram Personality Type!&";

                    //   API = API + "sender=Questa&route=4&country=91&DLT_TE_ID=1407161114341996906";


                    string API = "http://sms.ssdweb.in/api/sendhttp.php?authkey=365516Anl0xCI0kn56110dd2eP1&mobiles=" + SenderNumber + "&message=" + Msg + "sender=Questa&route=4&country=91&DLT_TE_ID=1407161114341996906";

                 //   string url = System.Web.HttpUtility.UrlEncode(API);



                    string MessageId = string.Empty;
                    using (var client = new WebClient()) //WebClient  
                    {
                        client.Headers.Add("Content-Type:application/json"); //Content-Type  
                        client.Headers.Add("Accept:application/json");

                        await Task.Run(() => this.UserSrv.SentOTPViaMail(ObjOTP.email, otp));
                        //this.UserSrv.SentOTPViaMail(ObjOTP.email, otp);

                        MessageId = client.DownloadString(API); //URI  
                        
                    }


                    if (!string.IsNullOrEmpty(MessageId))
                    {
                        return Ok(new { OTPNumber = otp, IsSend = true });
                    }
                    else
                    {
                        return Ok(new { Log = "OTP authencation failure issue contact to admin", IsSend = true });
                    }
                }
                else
                {
                    return Ok(new { Log = "Please check mobile number for sharing OTP", IsSend = true });
                }


            }
            catch (Exception ex)
            {
                throw new NotImplementedException(ex.Message);
            }
        }

        [HttpGet]
        [Route("api/CorporateUser/UpdateIsLogin/{IsLogin}/{TestId}")]
        public IHttpActionResult UpdateIsLogin(bool IsLogin, int TestId)
        {
            try
            {
                UserSrv.UpdateIdLogin(TestId, IsLogin);

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
            }
        }

       

    }
}
