using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuestionWizardApi.CorporateData;
using QuestionWizardApi.CorporateIBusinessLayer;
using QuestionWizardApi.CorporateModel.Model;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Core.Objects;
using QuestionWizardApi.CorporateIBusinessLayer.Interface;

namespace QuestionWizardApi.CorporateBusinessLayer.Service
{
    public class MasterDataService : IDisposable, IMasterData
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        CorporateAssessmentEntities DBEntities = new CorporateAssessmentEntities();
        ~MasterDataService()
        {
            Dispose(false);
        }

        public async Task<MasterBM> GetMasterData()
        {
            try
            {
                MasterDataModel objMaster = new MasterDataModel();
                MasterBM objBModel = new MasterBM();
                await Task.Run(() => {
                    using (CorporateAssessmentEntities db = new CorporateAssessmentEntities())
                    {
                        // Create a SQL command and add parameter
                        var cmd1 = db.Database.Connection.CreateCommand();
                        cmd1.CommandText = "[dbo].[SP_GetMasterData]";
                        cmd1.CommandType = CommandType.StoredProcedure;

                        // execute your command
                        db.Database.Connection.Open();
                        var reader = cmd1.ExecuteReader();

                        // Read second model --> Bar
                        objMaster.Countries = ((IObjectContextAdapter)db)
                            .ObjectContext
                            .Translate<mstCountry>(reader, "mstCountries", MergeOption.AppendOnly).ToList();

                        AutoMapper.Mapper.CreateMap<mstCountry, CountryBM>();
                        objBModel.Countries = AutoMapper.Mapper.Map<List<mstCountry>, List<CountryBM>>(objMaster.Countries);

                        reader.NextResult();


                        // Read second model --> Bar
                        objMaster.Qualification = ((IObjectContextAdapter)db)
                            .ObjectContext
                            .Translate<mstQualification>(reader, "mstQualifications", MergeOption.AppendOnly).ToList();

                        AutoMapper.Mapper.CreateMap<mstQualification, QualificationBM>();
                        objBModel.Qualification = AutoMapper.Mapper.Map<List<mstQualification>, List<QualificationBM>>(objMaster.Qualification);

                        reader.NextResult();

                        objMaster.Profession = ((IObjectContextAdapter)db)
                          .ObjectContext
                          .Translate<mstProfession>(reader, "mstProfessions", MergeOption.AppendOnly).ToList();

                        AutoMapper.Mapper.CreateMap<mstProfession, ProfessionBM>();
                        objBModel.Profession = AutoMapper.Mapper.Map<List<mstProfession>, List<ProfessionBM>>(objMaster.Profession);

                        reader.NextResult();

                        objMaster.Ages = ((IObjectContextAdapter)db)
                            .ObjectContext
                            .Translate<mstAge>(reader, "mstAges", MergeOption.AppendOnly).ToList();

                        AutoMapper.Mapper.CreateMap<mstAge, AgeBM>();
                        objBModel.Ages = AutoMapper.Mapper.Map<List<mstAge>, List<AgeBM>>(objMaster.Ages);
                        // move to next result set
                        reader.NextResult();

                        objMaster.Gender = ((IObjectContextAdapter)db)
                           .ObjectContext
                           .Translate<mstGender>(reader, "mstGenders", MergeOption.AppendOnly).ToList();

                        AutoMapper.Mapper.CreateMap<mstGender, GenderBM>();
                        objBModel.Gender = AutoMapper.Mapper.Map<List<mstGender>, List<GenderBM>>(objMaster.Gender);

                        reader.NextResult();

                        objMaster.MaritalStatus = ((IObjectContextAdapter)db)
                          .ObjectContext
                          .Translate<mstMaritalStatu>(reader, "mstMaritalStatus", MergeOption.AppendOnly).ToList();

                        AutoMapper.Mapper.CreateMap<mstMaritalStatu, MaritalStatusBM>();
                        objBModel.MaritalStatus = AutoMapper.Mapper.Map<List<mstMaritalStatu>, List<MaritalStatusBM>>(objMaster.MaritalStatus);

                        reader.NextResult();


                        objMaster.EmployeeStatus = ((IObjectContextAdapter)db)
                            .ObjectContext
                            .Translate<mstEmployeeStatu>(reader, "mstEmployeeStatus", MergeOption.AppendOnly).ToList();

                        AutoMapper.Mapper.CreateMap<mstEmployeeStatu, EmployeeStatusBM>();
                        objBModel.EmployeeStatus = AutoMapper.Mapper.Map<List<mstEmployeeStatu>, List<EmployeeStatusBM>>(objMaster.EmployeeStatus);

                        reader.NextResult();

                        

                        objMaster.Industry = ((IObjectContextAdapter)db)
                          .ObjectContext
                          .Translate<mstIndustry>(reader, "mstIndustries", MergeOption.AppendOnly).ToList();

                        AutoMapper.Mapper.CreateMap<mstIndustry, IndustryBM>();
                        objBModel.Industry = AutoMapper.Mapper.Map<List<mstIndustry>, List<IndustryBM>>(objMaster.Industry);
                        reader.NextResult();


                        objMaster.Experience = ((IObjectContextAdapter)db)
                        .ObjectContext
                        .Translate<mstExperence>(reader, "mstExperences", MergeOption.AppendOnly).ToList();

                        AutoMapper.Mapper.CreateMap<mstExperence, ExperienceBM>();
                        objBModel.Experience = AutoMapper.Mapper.Map<List<mstExperence>, List<ExperienceBM>>(objMaster.Experience);


                    }
                });

                return objBModel;
            }
            catch
            {
                throw;
            }
        }


        public async Task<List<StateBM>> GetState(int CountryId)
        {
            try
            {
                List<mstState> States = new List<mstState>();
                await Task.Run(() => {
                    States = DBEntities.mstStates.Where(x => x.CountryId == CountryId && x.IsActive == true).ToList();
                    AutoMapper.Mapper.CreateMap<mstState, StateBM>();
                });

                return AutoMapper.Mapper.Map<List<mstState>, List<StateBM>>(States);
            }
            catch (Exception ex)
            {
                throw;
            }
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
