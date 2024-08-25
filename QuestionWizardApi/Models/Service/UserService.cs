using QuestionWizardApi.Models.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using QuestionWizardApi.Models.Model;
using System.Data.Entity.Core.Objects;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Net;

namespace QuestionWizardApi.Models.Service
{
    public class UserService : Repository<txnCandidate>
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        public UserService(QuestionDemoEntities context): base(context) { }
        public QuestionDemoEntities DBEntities
        {
            get
            {
                return db as QuestionDemoEntities;
            }
        }
        public async Task<List<ClsCountry>> GetCounties()
        {
            try
            {
               // List<ClsCountry> lstCountry = new List<ClsCountry>();
                //await Task.Run(() =>
                //{
                //    lstCountry = DBEntities.mstCountries.Select(x => new ClsCountry
                //    {
                //        countryId = x.CountryId,
                //        countryname = x.CountryName
                //    }).ToList();
                //});

                return await(DBEntities.mstCountries.Where(x=>x.IsActive == true).Select(x => new ClsCountry
                {
                    countryId = x.CountryId,
                    countryname = x.CountryName
                })).ToListAsync();
            }
            catch(Exception ex)
            {
                throw;
            }
            
        }

        public bool IsProfileIdExists(int ProfileId)
        {
            try
            {

                if(DBEntities.mstProfileSelecteds.Any(x=>x.ProfileId == ProfileId && x.IsActive ==true))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch(Exception ex)
            {
                throw;
            }
        }
        public bool IsUserIdExists(int UserId)
        {
            try
            {

                if (DBEntities.txnCandidates.Any(x => x.UserId == UserId))
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
        public async Task<List<ClsState>> GetState(int CountryId)
        {
            try
            {
                //List<ClsState> lstState = new List<ClsState>();
                //await Task.Run(() =>
                //{
                //    lstState = DBEntities.mstStates.Where(x => x.CountryId == CountryId).Select(x => new ClsState
                //    {
                //        stateId = x.StateId,
                //        countryId = x.CountryId.HasValue ? x.CountryId.Value : 0,
                //        statename = x.StateName
                //    }).OrderBy(x=>x.statename).ToList();
                //});

                return await(DBEntities.mstStates.Where(x => x.CountryId == CountryId && x.IsActive == true).Select(x => new ClsState
                {
                    stateId = x.StateId,
                    countryId = x.CountryId.HasValue ? x.CountryId.Value : 0,
                    statename = x.StateName
                }).OrderBy(x => x.statename)).ToListAsync();
            }
            catch(Exception ex)
            {
                throw;
            }
            
        }

        public async Task<List<ClsQualification>> GetQualification()
        {
            try
            {
                List<int> QualificationIs = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8 };
                //List<ClsQualification> lstQualification = new List<ClsQualification>();
                //await Task.Run(() => {

                //    List<int> QualificationIs = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8 };

                //    lstQualification = DBEntities.mstQualifications.Where(x=> QualificationIs.Contains(x.QualificationId)).Select(x => new ClsQualification
                //    {
                //        QualificationId = x.QualificationId,
                //        QualificationName = x.QualificationName
                //    }).ToList();
                //});

                return await(DBEntities.mstQualifications.Where(x => x.IsActive == true).Where(x => QualificationIs.Contains(x.QualificationId)).Select(x => new ClsQualification
                {
                    QualificationId = x.QualificationId,
                    QualificationName = x.QualificationName
                })).ToListAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
            
        }

        public async Task<List<ClsProfession>> GetProfession()
        {
            try
            {
                //List<ClsProfession> lstProfession = new List<ClsProfession>();
                //await Task.Run(() => {
                //    lstProfession = DBEntities.mstProfessions.Select(x => new ClsProfession
                //    {
                //        ProfessionId = x.ProfessionId,
                //        ProfessionName = x.ProfessionName
                //    }).ToList();
                //});


                return await(DBEntities.mstProfessions.Where(x => x.IsActive == true).Select(x => new ClsProfession
                {
                    ProfessionId = x.ProfessionId,
                    ProfessionName = x.ProfessionName
                }).ToListAsync());
            }
            catch (Exception ex)
            {
                throw;
            }
            
        }

        public int GetUserId(string TranId, string TranStatus)
        {
            try
            {
                DateTime DateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
                txnCandidate objCandidate = new txnCandidate();
                objCandidate.CreatedAt = DateTime;
                objCandidate.TransId = TranId;
                objCandidate.TransStatus = TranStatus;
                objCandidate.LastModified = DateTime;
                DBEntities.txnCandidates.Add(objCandidate);
                DBEntities.SaveChanges();
                int UserId = objCandidate.UserId;

                return UserId;
            }
            catch(Exception ex)
            {
                throw;
            }
        }

        public int SaveInitialCandidateData(int UserId,string Title, string FirstName, string LastName, string email, string PhoneNumber,int ProfileId)
        {
            try
            {
                txnCandidate DbCandidate = DBEntities.txnCandidates.Where(x => x.UserId == UserId).FirstOrDefault();
                DateTime DateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
                DbCandidate.Title = Title;
                DbCandidate.FirstName = FirstName;
                DbCandidate.LastName = LastName;
                DbCandidate.UserEmail = email;
                DbCandidate.PhoneNumber = PhoneNumber;
                DbCandidate.ProfileSelected = ProfileId;
                DbCandidate.IsOTPRequire = true;
                DbCandidate.IsActive = true;
                DbCandidate.LastModified = DateTime;
                DbCandidate.IsInitialMail = true;
                DbCandidate.IsAttachmentSent = (ProfileId == 1 || ProfileId == 4) ? true : false;

                DBEntities.Entry(DbCandidate).State = System.Data.Entity.EntityState.Modified;
                DBEntities.SaveChanges();
                
                string hostName = Dns.GetHostName();

                int ExamId = GenerateRandomQuestionNumber(UserId, (int)enumQuestionSet.Set1, null, null, hostName);

                DBEntities.SaveChanges();

                return ExamId;
            }
            catch (Exception ex)
            {
                throw;
            }

        }


        public int SaveCandidateData(string Title, string FirstName, string LastName, string email, string PhoneNumber, int ProfileId)
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
                objCandidate.IsInitialMail = true;
                objCandidate.IsAttachmentSent = (ProfileId == 1 || ProfileId == 4) ? true : false;

                DBEntities.txnCandidates.Add(objCandidate);
                DBEntities.SaveChanges();
                int UserId = objCandidate.UserId;

                string hostName = Dns.GetHostName();

                int ExamId = GenerateRandomQuestionNumber(UserId, (int)enumQuestionSet.Set1, null, null, hostName);

                DBEntities.SaveChanges();

                return ExamId;
            }
            catch (Exception ex)
            {
                throw;
            }

        }




        public string GetUrlValue(string configName)
        {
           string Value =  DBEntities.MstConfigs.Where(x => x.ConfigName == configName).Select(x => x.ConfigValue).FirstOrDefault();

            return Value;
        }
        public bool SaveCandidateData(CandidateModel objUserModel)
        {
            bool IsSuccess;
            try
            {
                
                txnCandidate DbCandidate = DBEntities.txnCandidates.Where(x => x.UserId == objUserModel.UserId).FirstOrDefault();

                DateTime DateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

                if(objUserModel.UserGender == 3)
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

                if(objUserModel.Country == 1)
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

                DbCandidate.UserMaritalStatus = objUserModel.MaritalStatus;
                DbCandidate.UserEmployeeStatus = objUserModel.EmployeeStatus;
                DbCandidate.LastModified = DateTime;
               // DbCandidate.CreatedAt = DateTime;
                DBEntities.Entry(DbCandidate).State = System.Data.Entity.EntityState.Modified;
                DBEntities.SaveChanges();


                IsSuccess = true;

               // txnCandidate objCandidate = new txnCandidate();
               //// objCandidate.UserName = objUserModel.UserName;
               // objCandidate.UserEmail = objUserModel.UserEmail;
               // objCandidate.UserGender = objUserModel.UserGender;
               // objCandidate.UserAge = objUserModel.UserAge;
               // objCandidate.UserState = objUserModel.State;
               // objCandidate.UserCountry = objUserModel.Country;
               // objCandidate.UserProfessional = objUserModel.Professional;
               // objCandidate.CreatedAt = DateTime;
               // DBEntities.txnCandidates.Add(objCandidate);
               // DBEntities.SaveChanges();
               // int UserId = objCandidate.UserId;

                // //txnQualification objQualification = new txnQualification();

                // //foreach (var QualificationId in objUserModel.Qualification)
                // //{
                // //    objQualification.UserId = UserId;
                // //    objQualification.QualificationId = QualificationId;
                // //    DBEntities.txnQualifications.Add(objQualification);
                // //}

                // int ExamId = GenerateRandomQuestionNumber(UserId, (int)enumQuestionSet.Set1, null, null);

                // DBEntities.SaveChanges();

                // return ExamId;
            }
            catch (Exception ex)
            {
                IsSuccess = false;
                throw;
            }
            return IsSuccess;
        }

        public int GenerateRandomQuestionNumber(int UserId,int setId,int ? TestId,int? TypeId,string hostName)
        {
            try
            {
                ObjectParameter SystemTestId = new ObjectParameter("TestUniqId", typeof(int)); //Create Object parameter to receive a output value.It will behave like output parameter  

                var value = DBEntities.Sp_InsertRandomQuestion(UserId, setId, TestId, TypeId, hostName, SystemTestId).ToList(); //calling our entity imported function "Bangalore" is our input parameter, returnId is a output parameter, it will receive the output value   
                return Convert.ToInt32(SystemTestId.Value);
            }
            catch (Exception ex)
            {
                throw;
            }
           
        }

        public int GetLatestTestId(string useremail)
        {
            try
            {
                int TestId = (from i in DBEntities.txnCandidates
                              join j in DBEntities.txnUserTestDetails
                              on i.UserId equals j.UserId
                              where i.UserEmail == useremail
                              orderby i.CreatedAt descending
                              select j.TestId).FirstOrDefault();

                return TestId;
            }
            catch (Exception ex)
            {
                throw;
            }
            
        }
        public string GetUserNamebyTestId(int TestId)
        {
            try
            {
                string UserName = (from i in DBEntities.txnUserTestDetails
                                   join j in DBEntities.txnCandidates
                                   on i.UserId equals j.UserId
                                   where i.TestId == TestId
                                   select j.FirstName).FirstOrDefault();

                return UserName;
            }
            catch (Exception ex)
            {
                throw;
            }
            
        }
        public int GetLatestSetId(int TestId)
        {
            try
            {
                int SetId = 0;
                var ExamSetStatus = DBEntities.txnExamSetStatus.Where(x => x.TestId == TestId && (x.Status == "P" || x.Status == "C")).
                            OrderByDescending(x => x.LastModifiedAt).Select(x => x.SetId).FirstOrDefault();

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

        public bool IsUserExits(string emailId, int TestId)
        {
            try
            {
                int UserId = DBEntities.txnCandidates.Where(x => x.UserEmail == emailId).OrderByDescending(x => x.CreatedAt).Select(x => x.UserId).FirstOrDefault();

                return DBEntities.txnUserTestDetails.Where(x => x.UserId == UserId && x.TestId == TestId).Any();
            }
            catch (Exception ex)
            {
                throw;
            }

        }


        public async Task<List<ClsAge>> GetAge()
        {
            try
            {
                //List<ClsAge> lstAge = new List<ClsAge>();
                //await Task.Run(() => {
                //    lstAge = DBEntities.mstAges.Select(x => new ClsAge
                //    {
                //        AgeId = x.AgeId,
                //        AgeName = x.AgeName
                //    }).ToList();
                //});


                return await(DBEntities.mstAges.Where(x => x.IsActive == true).Select(x => new ClsAge
                {
                    AgeId = x.AgeId,
                    AgeName = x.AgeName
                }).ToListAsync());
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        public async Task<List<ClsGender>> GetGender()
        {
            try
            {
                List<int> GenderIds = new List<int>() { 1, 2, 3, 4 };
                //List<ClsGender> lstGender = new List<ClsGender>();
                //await Task.Run(() => {
                //    List<int> GenderIds = new List<int>() { 1, 2, 3, 4 };
                //    lstGender = DBEntities.mstGenders.Where(x=> GenderIds.Contains(x.GenderId)).Select(x => new ClsGender
                //    {
                //        GenderId = x.GenderId,
                //        GenderName = x.GenderName
                //    }).ToList();
                //});


                return await(DBEntities.mstGenders.Where(x => x.IsActive == true).Where(x => GenderIds.Contains(x.GenderId)).Select(x => new ClsGender
                {
                    GenderId = x.GenderId,
                    GenderName = x.GenderName
                }).ToListAsync());
            }
            catch (Exception ex)
            {
                throw;
            }

        }


        public async Task<List<ClsMaritalStatus>> GetMaritalStatus()
        {
            try
            {
                //List<ClsMaritalStatus> lstMaritalStatus = new List<ClsMaritalStatus>();
                //await Task.Run(() => {
                //    lstMaritalStatus = DBEntities.mstMaritalStatus.Select(x => new ClsMaritalStatus
                //    {
                //        MaritalId = x.MaritalId,
                //        MaritalName = x.MaritalName
                //    }).ToList();
                //});


                return await(DBEntities.mstMaritalStatus.Where(x => x.IsActive == true).Select(x => new ClsMaritalStatus
                {
                    MaritalId = x.MaritalId,
                    MaritalName = x.MaritalName
                }).ToListAsync());
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        public async Task<List<ClsEmployeeStatus>> GetEmployeeStatus()
        {
            try
            {
                //List<ClsEmployeeStatus> lstEmployeeStatus = new List<ClsEmployeeStatus>();
                //await Task.Run(() => {
                //    lstEmployeeStatus = DBEntities.mstEmployeeStatus.Select(x => new ClsEmployeeStatus
                //    {
                //        EmploymentId = x.EmploymentId,
                //        EmploymentName = x.EmploymentName
                //    }).ToList();
                //});


                return await(DBEntities.mstEmployeeStatus.Where(x => x.IsActive == true).Select(x => new ClsEmployeeStatus
                {
                    EmploymentId = x.EmploymentId,
                    EmploymentName = x.EmploymentName
                }).ToListAsync());
            }
            catch (Exception ex)
            {
                throw;
            }

        }


        public async Task<List<string>> GetIndustry()
        {
            try
            {
                //List<string> lstIndustry = new List<string>();
                //await Task.Run(() => {
                //    lstIndustry = DBEntities.mstIndustries.Select(x=>x.IndustryName).ToList();
                //});


                return await(DBEntities.mstIndustries.Where(x => x.IsActive == true).Select(x => x.IndustryName).ToListAsync());
            }
            catch (Exception ex)
            {
                throw;
            }

        }


        public async Task<CandidateModel> GetCandidateDetails(ClsUserModel ObjUserData,int TestId)
        {
            try
            {
                CandidateModel objCandidate = new CandidateModel();
               
                await Task.Run(() => {

                    

                    if(ObjUserData.UserGender > 4)
                    {
                        objCandidate.UserGender = 3;

                        objCandidate.GenderTxt = DBEntities.mstGenders.Where(x => x.GenderId == ObjUserData.UserGender).Select(x => x.GenderName).FirstOrDefault();
                    }
                    else
                    {
                        objCandidate.UserGender = ObjUserData.UserGender;
                    }

                    if (ObjUserData.Qualification > 8)
                    {
                        objCandidate.Qualification = 8;

                        objCandidate.QualificationTxt = DBEntities.mstQualifications.Where(x => x.QualificationId == ObjUserData.Qualification).Select(x => x.QualificationName).FirstOrDefault();
                    }
                    else
                    {
                        objCandidate.Qualification = ObjUserData.Qualification;
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
                    objCandidate.UserAge = ObjUserData.UserAge;
                    objCandidate.State = ObjUserData.State;
                    objCandidate.Country = ObjUserData.Country;
                    objCandidate.Professional = ObjUserData.Professional;
                    objCandidate.MaritalStatus = ObjUserData.MaritalStatus;
                    objCandidate.EmployeeStatus = ObjUserData.EmployeeStatus;
                    objCandidate.IsOTPRequire = ObjUserData.IsOTPRequire;
                    objCandidate.IsActive = ObjUserData.IsActive;
                });


                return objCandidate;
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
                            OrderByDescending(x => x.LastModifiedAt).Select(x => x.SetId).FirstOrDefault();

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



        public bool Check15DaysValidation(int UserId)
        {
            try
            {
                DateTime DateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

                DateTime dt = DBEntities.txnCandidates.Where(x => x.UserId == UserId).Select(x => x.CreatedAt.Value).FirstOrDefault();

                DateTime dtNow = DateTime;

                TimeSpan difference = dtNow - dt;

                int Days = difference.Days;

                if(Days > 15 )
                {
                    return true;
                }
                else if(Days == 15 && difference.Hours > 00)
                {
                    return true;
                }
                else if (Days == 15 && difference.Hours == 00 && difference.Minutes > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch(Exception ex)
            {
                throw;
            }
        }


        public string GetEmailId(int  TestId)
        {
            try
            {
                string EmailId = (from i in DBEntities.txnCandidates
                              join j in DBEntities.txnUserTestDetails
                              on i.UserId equals j.UserId
                              where j.TestId == TestId
                              orderby i.CreatedAt descending
                              select i.UserEmail).FirstOrDefault();

                return EmailId;
            }
            catch (Exception ex)
            {
                throw;
            }

        }


    }
}