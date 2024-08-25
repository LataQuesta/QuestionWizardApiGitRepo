using QuestionWizardApi.CorporateBusinessLayer.Service;
using QuestionWizardApi.CorporateIBusinessLayer.Interface;
using QuestionWizardApi.CorporateModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace QuestionWizardApi.Areas.CorporateAssessment.Controllers
{
    [RoutePrefix("api/CorporateErrorHandler")]
    public class CorporateErrorHandlerController : ApiController
    {
        //private IError ErrorSrv = null;
        //public CorporateErrorHandlerController(IError ErrorSrv)
        //{
        //    this.ErrorSrv = ErrorSrv;
        //}

        [HttpPost]
        [Route("SaveCorporateErrorLog")]
        public IHttpActionResult SaveCorporateErrorLog(ErrorLogBM ErrorLog)
        {
            try
            {
                ErrorService obj = new ErrorService();
                obj.SaveErrorLog(ErrorLog);

                return Ok();
            }
            catch (Exception ex)
            {
                throw new NotImplementedException(ex.Message);
                // return BadRequest(ex.Message.ToString());
            }
        }
    }
}
