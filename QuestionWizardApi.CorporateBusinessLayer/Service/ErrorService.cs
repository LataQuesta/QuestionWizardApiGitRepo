
using QuestionWizardApi.CorporateData;
using QuestionWizardApi.CorporateIBusinessLayer;
using QuestionWizardApi.CorporateIBusinessLayer.Interface;
using QuestionWizardApi.CorporateModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestionWizardApi.CorporateBusinessLayer.Service
{
    public class ErrorService 
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        

        public void SaveErrorLog(ErrorLogBM ErrorLog)
        {
            try
            {
                using (CorporateAssessmentEntities DBEntities = new CorporateAssessmentEntities())
                {
                    DateTime DateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

                    if (ErrorLog != null)
                    {

                        ErrorLog objErrorLog = new ErrorLog();
                        objErrorLog.Message = ErrorLog.Message;
                        objErrorLog.RequestMethod = ErrorLog.RequestMethod;
                        objErrorLog.RequestUri = ErrorLog.RequestUri;
                        objErrorLog.DateTime = DateTime;
                        objErrorLog.ErrorMessage = ErrorLog.ErrorMsg;
                        DBEntities.ErrorLogs.Add(objErrorLog);

                        DBEntities.SaveChanges();

                    }
                }
                   
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        

    }
}
