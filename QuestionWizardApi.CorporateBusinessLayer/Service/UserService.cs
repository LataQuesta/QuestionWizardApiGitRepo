using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using QuestionWizardApi.CorporateData;
using QuestionWizardApi.CorporateModel.Model;
using System.Data.Entity.Core.Objects;
using QuestionWizardApi.CorporateIBusinessLayer.Interface;
using System.Net.Mail;

namespace QuestionWizardApi.CorporateBusinessLayer.Service
{
    public class UserService : IDisposable, IUser
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        CorporateAssessmentEntities DBEntities = new CorporateAssessmentEntities();
        ~UserService()
        {
            Dispose(false);
        }

        /// <summary>
        /// Check Profile Exists IN DB
        /// </summary>
        /// <param name="ProfileId"></param>
        /// <returns></returns>
        public bool IsProfileIdExists(int ProfileId)
        {
            try
            {

                if (DBEntities.mstProfileSelecteds.Any(x => x.ProfileId == ProfileId && x.IsActive == true))
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


        /// <summary>
        /// Check Company Exists in DB
        /// </summary>
        /// <param name="CompanyId"></param>
        /// <returns></returns>
        public bool IsValidCompany(int CompanyId)
        {
            try
            {

                if (DBEntities.mstCompanies.Any(x => x.CompanyId == CompanyId && x.IsActive == true))
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

        /// <summary>
        /// Check Assessment Set Exists in DB
        /// </summary>
        /// <param name="AssessmentId"></param>
        /// <returns></returns>
        public bool IsValidAssessmentSet(int AssessmentId)
        {
            try
            {

                if (DBEntities.mstAssessmentSets.Any(x => x.AssessmentId == AssessmentId))
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

        /// <summary>
        /// For Generating Assessment LINK ,Initial data of Candidate save in DB 
        /// </summary>
        /// <param name="Title"></param>
        /// <param name="FirstName"></param>
        /// <param name="LastName"></param>
        /// <param name="email"></param>
        /// <param name="PhoneNumber"></param>
        /// <param name="ProfileId"></param>
        /// <param name="CompanyId"></param>
        /// <param name="ModuleId"></param>
        /// <returns></returns>
        public int SaveCandidateData( int ProfileId, int CompanyId, int AssessmentId)
        {
            try
            {

                DateTime DateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);


                txnCandidate objCandidate = new txnCandidate();
               // objCandidate.Title = Title;
              //  objCandidate.FirstName = FirstName;
              //  objCandidate.LastName = LastName;
              //  objCandidate.UserEmail = email;
              //  objCandidate.PhoneNumber = PhoneNumber;
                objCandidate.ProfileSelected = ProfileId;
                objCandidate.CreatedAt = DateTime;
                objCandidate.IsOTPRequire = false;
                objCandidate.IsActive = true;
                objCandidate.LastModified = DateTime;
               // objCandidate.IsInitialMailRequire = true;
              //  objCandidate.IsAttachmentSent = ProfileId == (int)MstProfile.FreeAssessment ? false : true;
              //  objCandidate.IsFinalMailRequire = false;
                objCandidate.AssessmentId = AssessmentId;
                objCandidate.CompanyId = CompanyId;
                DBEntities.txnCandidates.Add(objCandidate);
                DBEntities.SaveChanges();
                int UserId = objCandidate.UserId;



                //mstCompany objCompany = DBEntities.mstCompanies.Where(x => x.CompanyId == CompanyId).FirstOrDefault();

                //if(objCompany != null)
                //{
                //    txnMailSentToCandidate objMailSentToCandidate = new txnMailSentToCandidate();

                //    objMailSentToCandidate.UserId = UserId;
                // //   objMailSentToCandidate.IsInitialMailToCandidate = objCompany.IsMailRequireToCandidate.Value ? true : false;
                // //   objMailSentToCandidate.IsFinalMailToCandidate = objCompany.IsMailRequireToCandidate.Value ? true : false;
                //  //  objMailSentToCandidate.IsInitialMailToHr = objCompany.IsMailRequireToHr.Value ? true : false;
                // //   objMailSentToCandidate.IsFinalMailToHr = objCompany.IsMailRequireToHr.Value ? true : false;
                //    objMailSentToCandidate.IsAttachmentReqToCandidate = objCompany.IsMailRequireToCandidate.Value ? true : false;
                //    objMailSentToCandidate.IsAttachmentReqToHr = objCompany.IsMailRequireToHr.Value ? true : false;
                //    objMailSentToCandidate.HrName = objCompany.HrName;
                //    objMailSentToCandidate.HrEmailId = objCompany.HrEmailId;
                //    DBEntities.txnMailSentToCandidates.Add(objMailSentToCandidate);
                //    DBEntities.SaveChanges();
                //}


                string hostName = Dns.GetHostName();
                int ExamId = 0;
                ExamId = GenerateRandomQuestionNumber(UserId, null, null, hostName, AssessmentId,null);

                DBEntities.SaveChanges();

                return ExamId;
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        public int SaveCandidateRecord(string Title, string FirstName, string LastName, string email, string PhoneNumber, int ProfileId, int CompanyId, int AssessmentId)
        {
            try
            {

                DateTime DateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);


                txnCandidate objCandidate = new txnCandidate();
                objCandidate.Title = Title;
                objCandidate.FirstName = FirstName;
                objCandidate.LastName = LastName;
                objCandidate.UserEmail = email;
                objCandidate.PhoneNumber = PhoneNumber;
                objCandidate.ProfileSelected = ProfileId;
                objCandidate.CreatedAt = DateTime;
                objCandidate.IsOTPRequire = true;
                objCandidate.IsActive = true;
                objCandidate.LastModified = DateTime;
                objCandidate.AssessmentId = AssessmentId;
                objCandidate.CompanyId = CompanyId;
                DBEntities.txnCandidates.Add(objCandidate);
                DBEntities.SaveChanges();
                int UserId = objCandidate.UserId;

                string hostName = Dns.GetHostName();
                int ExamId = 0;

                ExamId = GenerateRandomQuestionNumber(UserId, null, null, hostName, AssessmentId, null);

                DBEntities.SaveChanges();

                return ExamId;
            }
            catch (Exception ex)
            {
                throw;
            }

        }


        public HumanResourceRepo GetHumanResource(int CompanyId)
        {

            mstHumanResourceRepo objHr = new mstHumanResourceRepo();//DBEntities.mstHumanResourceRepoes.Where(x => x.CompanyId == CompanyId).FirstOrDefault();

            AutoMapper.Mapper.CreateMap<mstHumanResourceRepo, HumanResourceRepo>();
            return AutoMapper.Mapper.Map<mstHumanResourceRepo, HumanResourceRepo>(objHr);
        }

        //public Tuple<bool,bool> MailRequireToSent(int TestId)
        //{
        //    int CompanyId = DBEntities.txnUserTestDetails.Join(DBEntities.txnCandidates, x => x.UserId, y => y.UserId, (x, y) => new { UTest = x, Candidate = y }).Select(i => i.Candidate.CompanyId.Value).FirstOrDefault();

        //    mstCompany objCompany = DBEntities.mstCompanies.Where(x => x.CompanyId == CompanyId).FirstOrDefault();

        //    Tuple<bool, bool> t = Tuple.Create(objCompany.IsMailRequireToHr.Value, objCompany.IsMailRequireToCandidate.Value);

        //    return t;
        //}
        /// <summary>
        /// Generate Module Wise Question 
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="TestId"></param>
        /// <param name="TypeId"></param>
        /// <param name="hostName"></param>
        /// <param name="AssessmentId"></param>
        /// <param name="ModuleId"></param>
        /// <returns></returns>
        public int GenerateRandomQuestionNumber(int UserId, int? TestId, int? TypeId, string hostName, int? AssessmentId,int? ModuleId)
        {
            try
            {
                ObjectParameter SystemTestId = new ObjectParameter("TestUniqId", typeof(int)); //Create Object parameter to receive a output value.It will behave like output parameter  

                var value = DBEntities.Sp_InsertRandomQuestion(UserId, ModuleId, TestId, TypeId, AssessmentId, hostName, SystemTestId).ToList(); //calling our entity imported function "Bangalore" is our input parameter, returnId is a output parameter, it will receive the output value   

                if (ModuleId == (int)AssessmentModule.H1PartAAptitude)
                {
                    int k = 0;
                    var Question = DBEntities.txnQuestions.Where(x => x.TestId == TestId && x.ModuleId == ModuleId).OrderBy(x => Guid.NewGuid()).ToList();

                    foreach (var Ques in Question)
                    {
                        Ques.QuesOrder = k++;
                    }

                    DBEntities.SaveChanges();

                }

                return Convert.ToInt32(SystemTestId.Value);
            }
            catch (Exception ex)
            {
                throw;
            }

        }


        /// <summary>
        /// Get Candidate Details Using TestId
        /// </summary>
        /// <param name="TestId"></param>
        /// <returns></returns>
        public CandidateBM GetCandidateData(int TestId)
        {
            try
            {

                var _ = DBEntities.txnCandidates.Where(x => x.UserId == 501).FirstOrDefault();

                CandidateBM CandidateData = DBEntities.txnCandidates.Join(DBEntities.txnUserTestDetails, i => i.UserId, j => j.UserId, (i, j) => new { i, j })
                    .Where(x => x.j.TestId == TestId)
                    .Select(x => new CandidateBM
                    {
                        UserId = x.i.UserId,
                        Title = x.i.Title,
                        FirstName = x.i.FirstName,
                        LastName = x.i.LastName,
                        UserEmail = x.i.UserEmail,
                        PhoneNumber = x.i.PhoneNumber,
                        UserDateOfBirth = x.i.UserDateOfBirth.HasValue ? x.i.UserDateOfBirth.Value :(DateTime?)null,
                        UserGender = x.i.UserGender,
                        UserAge = x.i.UserAge,
                        UserState = x.i.UserState,
                        UserCountry = x.i.UserCountry,
                        UserQualification = x.i.UserQualification,
                        UserProfessional = x.i.UserProfessional,
                        UserMaritalStatus = x.i.UserMaritalStatus,
                        UserEmployeeStatus = x.i.UserEmployeeStatus,
                        CreatedAt = x.i.CreatedAt,
                        ProfileId = x.i.ProfileSelected,
                        IsOTPRequire = x.i.IsOTPRequire,
                        LastModified = x.i.LastModified,
                        IsActive = x.i.IsActive,
                        IsMobileDevice = x.i.IsMobileDevice,
                        IsDesktopDevice = x.i.IsDesktopDevice,
                        IsTabDevice = x.i.IsTabDevice,
                        BrowserName = x.i.BrowserName,
                        IsLogin = x.i.IsLogin,
                        UserTestId = x.j.TestId,
                        status = x.j.status,
                        MainType = x.i.MainType.HasValue ? x.i.MainType.Value : 0,
                        ActionCenter = x.i.ActionCenter.HasValue ? x.i.ActionCenter.Value : 0,
                        FellingCenter = x.i.FellingCenter.HasValue ? x.i.FellingCenter.Value : 0,
                        ThinkingCenter = x.i.ThinkingCenter.HasValue ? x.i.ThinkingCenter.Value : 0,
                        CompanyId = x.i.CompanyId.HasValue ? x.i.CompanyId.Value : 0,
                        ModuleId = x.i.AssessmentId.HasValue ? x.i.AssessmentId.Value : 0,
                        UserExperience = x.i.UserExperience,
                        AssessmentCompleteDate = x.j.LastModifiedAt.HasValue ? x.j.LastModifiedAt.Value : (DateTime?)null
                    }).FirstOrDefault();

                // AutoMapper.Mapper.DynamicMap(CandidateData, CandidateBM);
                //  AutoMapper.Mapper.CreateMap<CandidateBM>();

                return CandidateData;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        
        public CandidateModel GetCandidateDetails(CandidateBM ObjUserData, int TestId)
        {
            try
            {
                DateTime? nullDateTime = null;

                CandidateModel objCandidate = new CandidateModel();

                if (ObjUserData.UserGender > 4)
                {
                    objCandidate.UserGender = 3;

                    objCandidate.GenderTxt = DBEntities.mstGenders.Where(x => x.GenderId == ObjUserData.UserGender).Select(x => x.GenderName).FirstOrDefault();
                }
                else
                {
                    objCandidate.UserGender = ObjUserData.UserGender.HasValue ? ObjUserData.UserGender.Value : 0;
                }

                if (ObjUserData.UserQualification != null && ObjUserData.UserQualification > 8)
                {
                    objCandidate.Qualification = 8;

                    objCandidate.QualificationTxt = DBEntities.mstQualifications.Where(x => x.QualificationId == ObjUserData.UserQualification).Select(x => x.QualificationName).FirstOrDefault();
                }
                else
                {
                    objCandidate.Qualification = ObjUserData.UserQualification.HasValue ? ObjUserData.UserQualification.Value : 0;
                }

                objCandidate.Industry = (from i in DBEntities.mstIndustries
                                         join j in DBEntities.txnIndustries
                                         on i.IndustryId equals j.IndustryId
                                         where j.UserId == ObjUserData.UserId
                                         select i.IndustryName).ToArray();


                objCandidate.TestId = TestId;
                objCandidate.UserId = ObjUserData.UserId;
                objCandidate.Title = ObjUserData.Title;
                objCandidate.FirstName = ObjUserData.FirstName;
                objCandidate.LastName = ObjUserData.LastName;
                objCandidate.UserEmail = ObjUserData.UserEmail;
                objCandidate.PhoneNumber = ObjUserData.PhoneNumber;
                objCandidate.UserAge = ObjUserData.UserAge.HasValue ? ObjUserData.UserAge.Value : 0;
                objCandidate.State = ObjUserData.UserState.HasValue ? ObjUserData.UserState.Value : 0;
                objCandidate.Country = ObjUserData.UserCountry.HasValue ? ObjUserData.UserCountry.Value : 0;
                objCandidate.Professional = ObjUserData.UserProfessional.HasValue ? ObjUserData.UserProfessional.Value : 0;
                objCandidate.MaritalStatus = ObjUserData.UserMaritalStatus.HasValue ? ObjUserData.UserMaritalStatus.Value : 0;
                objCandidate.EmployeeStatus = ObjUserData.UserEmployeeStatus.HasValue ? ObjUserData.UserEmployeeStatus.Value : 0;
                objCandidate.IsOTPRequire = ObjUserData.IsOTPRequire.HasValue ? ObjUserData.IsOTPRequire.Value : false;
                objCandidate.IsActive = ObjUserData.IsActive.HasValue ? ObjUserData.IsActive.Value : false;
                objCandidate.IsMobileDevice = ObjUserData.IsMobileDevice.HasValue ? ObjUserData.IsMobileDevice.Value : false;
                objCandidate.IsDesktopDevice = ObjUserData.IsDesktopDevice.HasValue ? ObjUserData.IsMobileDevice.Value : false;
                objCandidate.IsTabDevice = ObjUserData.IsTabDevice.HasValue ? ObjUserData.IsMobileDevice.Value : false;
                objCandidate.BrowserName = ObjUserData.BrowserName;
                objCandidate.IsLogin = ObjUserData.IsLogin.HasValue ? ObjUserData.IsLogin.Value : false;
                objCandidate.MainType = ObjUserData.MainType;
                objCandidate.ActionCenter = ObjUserData.ActionCenter;
                objCandidate.FellingCenter = ObjUserData.FellingCenter;
                objCandidate.ThinkingCenter = ObjUserData.ThinkingCenter;
                objCandidate.ModuleId = ObjUserData.ModuleId;
                objCandidate.DateOfBirth = ObjUserData.UserDateOfBirth.HasValue ? ObjUserData.UserDateOfBirth.Value : nullDateTime;
                objCandidate.Experience = ObjUserData.UserExperience.HasValue ? ObjUserData.UserExperience.Value : 0;
                return objCandidate;
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        /// <summary>
        /// Candidate access Link Till 15 days
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>

        public bool Check15DaysValidation(int UserId)
        {
            try
            {
                DateTime DateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

                DateTime StartDateTime = DBEntities.txnCandidates.Where(x => x.UserId == UserId).Select(x => x.CreatedAt.Value).FirstOrDefault();

                DateTime SixMonthLater = StartDateTime.AddMonths(12);

                string StartDate = SixMonthLater.ToString("dd/MM/yyyy");

                string TodayDate = DateTime.Now.ToString("dd/MM/yyyy");

                //DateTime dtNow = DateTime;

                //TimeSpan difference = dtNow - dt;

                //int Days = difference.Days;

                if (StartDate.Equals(TodayDate))
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


        /// <summary>
        /// Save Candidate Record in DB
        /// </summary>
        /// <param name="objUserModel"></param>
        /// <returns></returns>

        public bool SaveCandidateData(CandidateModel objUserModel)
        {
            bool IsSuccess;
            try
            {
                txnCandidate DbCandidate = DBEntities.txnCandidates.Where(x => x.UserId == objUserModel.UserId).FirstOrDefault();

                DateTime DateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

                if (objUserModel.UserGender == 3)
                {
                    mstGender objGender = new mstGender();
                    objGender.GenderName = objUserModel.GenderTxt;
                    DBEntities.mstGenders.Add(objGender);
                    DBEntities.SaveChanges();
                    DbCandidate.UserGender = objGender.GenderId;
                }
                else
                {
                    DbCandidate.UserGender = objUserModel.UserGender;
                }

                DbCandidate.UserAge = objUserModel.UserAge;

                if (objUserModel.Country == 1)
                {
                    DbCandidate.UserCountry = objUserModel.Country;
                    DbCandidate.UserState = objUserModel.State;
                }
                else
                {
                    DbCandidate.UserCountry = objUserModel.Country;
                }

                if (objUserModel.Qualification == 8)
                {
                    mstQualification objQulaification = new mstQualification();
                    objQulaification.QualificationName = objUserModel.QualificationTxt;
                    DBEntities.mstQualifications.Add(objQulaification);
                    DBEntities.SaveChanges();
                    DbCandidate.UserQualification = objQulaification.QualificationId;
                }
                else
                {
                    DbCandidate.UserQualification = objUserModel.Qualification;
                }

                DBEntities.txnIndustries.RemoveRange(DBEntities.txnIndustries.Where(x => x.UserId == objUserModel.UserId));
                DBEntities.SaveChanges();

                foreach (var IndustryName in objUserModel.Industry.Distinct<string>())
                {
                    int IndustryId = DBEntities.mstIndustries.Where(x => x.IndustryName == IndustryName).Select(x => x.IndustryId).FirstOrDefault();

                    txnIndustry objIndustry = new txnIndustry();
                    objIndustry.IndustryId = IndustryId;
                    objIndustry.UserId = objUserModel.UserId;
                    DBEntities.txnIndustries.Add(objIndustry);
                    DBEntities.SaveChanges();
                }
              //  DbCandidate.IsFinalMailRequire = false;
                DbCandidate.FirstName = objUserModel.FirstName;
                DbCandidate.LastName = objUserModel.LastName;
                DbCandidate.UserEmail = objUserModel.UserEmail;
                DbCandidate.PhoneNumber = objUserModel.PhoneNumber;
                DbCandidate.UserMaritalStatus = objUserModel.MaritalStatus;
                DbCandidate.UserEmployeeStatus = objUserModel.EmployeeStatus;
                DbCandidate.LastModified = DateTime;
                DbCandidate.IsMobileDevice = objUserModel.IsMobileDevice;
                DbCandidate.IsDesktopDevice = objUserModel.IsDesktopDevice;
                DbCandidate.IsTabDevice = objUserModel.IsTabDevice;
                DbCandidate.BrowserName = objUserModel.BrowserName;
                DbCandidate.UserDateOfBirth = objUserModel.DateOfBirth;
                DbCandidate.UserExperience = objUserModel.Experience;
                DbCandidate.IsLogin = true;

                // DbCandidate.CreatedAt = DateTime;
                DBEntities.Entry(DbCandidate).State = System.Data.Entity.EntityState.Modified;

                //txnMailSentToCandidate objMailSentToCandidate = DBEntities.txnMailSentToCandidates.Where(x => x.UserId == objUserModel.UserId).FirstOrDefault();
                
                //objMailSentToCandidate.CandidateName = objUserModel.FirstName + " " + objUserModel.LastName;
                //objMailSentToCandidate.CandidateEmailId = objUserModel.UserEmail;

                //DBEntities.Entry(objMailSentToCandidate).State = System.Data.Entity.EntityState.Modified;

                DBEntities.SaveChanges();

                List<txnExamSetStatu> lstExamStatus = DBEntities.txnExamSetStatus.Where(x => x.TestId == objUserModel.TestId).ToList();

                lstExamStatus.ForEach(a => a.IsDisplayInstruction = null);

                DBEntities.SaveChanges();


                IsSuccess = true;
            }
            catch (Exception ex)
            {
                IsSuccess = false;
                throw;
            }
            return IsSuccess;
        }

        /// <summary>
        /// Get website domain name from DB according to current api domain
        /// </summary>
        /// <param name="configName"></param>
        /// <returns></returns>
        public string GetUrlValue(string configName)
        {
            string Value = DBEntities.MstConfigs.Where(x => x.ConfigName == configName).Select(x => x.ConfigValue).FirstOrDefault();

            return Value;
        }
       
        /// <summary>
        /// Get latest set id base on test id
        /// </summary>
        /// <param name="TestId"></param>
        /// <returns></returns>
        public int GetLatestSetId(int TestId)
        {
            try
            {
                int SetId = 0;
                var ExamSetStatus = DBEntities.txnExamSetStatus.Where(x => x.TestId == TestId && (x.Status == "P" || x.Status == "C")).
                            OrderByDescending(x => x.LastModifiedAt).Select(x => x.ModuleId).FirstOrDefault();

                if (ExamSetStatus != null)
                {
                    SetId = ExamSetStatus.HasValue ? ExamSetStatus.Value : 0;
                }

                return SetId;
            }
            catch (Exception ex)
            {
                throw;
            }

        }
        
        

        public int GetProgressExamSet(int TestId)
        {
            try
            {
                int SetId = 0;
                var ExamSetStatus = DBEntities.txnExamSetStatus.Where(x => x.TestId == TestId && x.Status == "P").
                            OrderByDescending(x => x.LastModifiedAt).Select(x => x.ModuleId).FirstOrDefault();

                if (ExamSetStatus != null)
                {
                    SetId = ExamSetStatus.HasValue ? ExamSetStatus.Value : 0;
                }

                return SetId;
            }
            catch (Exception ex)
            {
                throw;
            }

        }


        

        /// <summary>
        /// Deactived Current User If User Lgin
        /// </summary>
        /// <param name="TestId"></param>
        /// <param name="IsLogin"></param>
        public void UpdateIdLogin(int TestId, bool IsLogin)
        {
            try
            {
                DateTime DateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

                txnCandidate obj = DBEntities.txnCandidates.Join(DBEntities.txnUserTestDetails, x => x.UserId, y => y.UserId, (x, y) => new { x, y }).
                                    Where(i => i.y.TestId == TestId).Select(j => j.x).FirstOrDefault();

                obj.IsLogin = IsLogin;
                obj.LastModified = DateTime;
                DBEntities.Entry(obj).State = System.Data.Entity.EntityState.Modified;
                DBEntities.SaveChanges();

            }
            catch (Exception ex)
            {
                throw;
            }
        }


        public async Task SentOTPViaMail(string EmailId,int OTP)
        {
            try
            {

                string FROM = "assessment@questa.in";
                string FROMNAME = "Questa Enneagram OTP Verification";
                
                string TO = EmailId;
               
                // Replace smtp_username with your Amazon SES SMTP user name.
                string SMTP_USERNAME = "AKIAYNH52N4VNGNPG774";

                // Replace smtp_password with your Amazon SES SMTP user name.
                string SMTP_PASSWORD = "BLhNdOkLRjvzh2VQO3v5dgu5AK1LnBpYFVUWQljxEP5e";

                // (Optional) the name of a configuration set to use for this message.
                // If you comment out this line, you also need to remove or comment out
                // the "X-SES-CONFIGURATION-SET" header below.
                string CONFIGSET = "QuestaEmailServer";

                // If you're using Amazon SES in a region other than US West (Oregon), 
                // replace email-smtp.us-west-2.amazonaws.com with the Amazon SES SMTP  
                // endpoint in the appropriate AWS Region.
                string HOST = "email-smtp.ap-south-1.amazonaws.com";

                // The port you will connect to on the Amazon SES SMTP endpoint. We
                // are choosing port 587 because we will use STARTTLS to encrypt
                // the connection.
                int PORT = 587;

                // The subject line of the email
                string SUBJECT =
                    "Questa Enneagram OTP Verification ";

                // The body of the email
                string HTMLContent = "<html><head></head><body><p>";
                HTMLContent = HTMLContent + OTP;
                HTMLContent = HTMLContent + "  is your One Time Verification (OTP) code to attempt the Self-Discovery Assessment at Questa Enneagram.This is valid for a period of 10 minutes.You are one step away from discovering your Enneagram Personality Type!</p></body></html> ";
                string BODY = HTMLContent;
               
                // Create and build a new MailMessage object
                MailMessage message = new MailMessage();
                message.IsBodyHtml = true;
                //create Alrternative HTML view
                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(BODY, null, "text/html");

               
                //Add view to the Email Message
                message.AlternateViews.Add(htmlView);

                message.From = new MailAddress(FROM, FROMNAME);
                message.To.Add(new MailAddress(TO));
               
                message.Subject = SUBJECT;
                message.Body = BODY;
               
                // Comment or delete the next line if you are not using a configuration set
                message.Headers.Add("X-SES-CONFIGURATION-SET", CONFIGSET);

                using (var client = new System.Net.Mail.SmtpClient(HOST, PORT))
                {
                    // Pass SMTP credentials
                    client.Credentials =
                        new NetworkCredential(SMTP_USERNAME, SMTP_PASSWORD);

                    // Enable SSL encryption
                    client.EnableSsl = true;

                    client.Send(message);

                    client.Dispose();
                }
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        //public int GetAssesmentIdBaseOnCompanyId(int CompanyId)
        //{
        //    try
        //    {
        //        return DBEntities.mstCompanies.Where(x => x.CompanyId == CompanyId).Select(x => x.AssessmentId.Value).FirstOrDefault();
        //    }
        //    catch(Exception ex)
        //    {
        //        throw;
        //    }
        //}

        public dynamic GetDummyCandidateRecord()
        {
            dynamic dummyCandidateRecord = DBEntities.DummyCandidateRecords.Select(x => 
            new { CandidateName =  x.CandidateName, CandidateEmail = x.CandidateEmail }).ToList();

            return dummyCandidateRecord;
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
        #endregion
    }
}