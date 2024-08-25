using QuestionWizardApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace QuestionWizardApi.Controllers
{
    public class AwsMailController : ApiController
    {
        //private MailService MailSrv = null;
        //public AwsMailController(MailService MailSrv)
        //{
        //    this.MailSrv = MailSrv;
        //}
        //[HttpGet]
        //[Route("api/AwsMail/SentPdfFileToCandidate")]
        //public IHttpActionResult SentPdfFileToCandidate(ClsQuestionModel objQuestionModel)
        //{
        //    try
        //    {

        //        this.MailSrv.CreateDirectory();
        //        string FileName = (objQuestionModel.TestId == 0 || objQuestionModel.TestId == null) ? string.Empty : Convert.ToString(objQuestionModel.TestId);
        //      //  this.MailSrv.DownloadFileFromS3Bucket(FileName);
        //        MailBody objMailBody = this.MailSrv.GetMailConfig(objQuestionModel.TestId,1);
        //        this.MailSrv.SentMail(objMailBody, objQuestionModel.TestId.ToString(),"");
        //        return Ok(new { isSucess = true });
        //    }
        //    catch (Exception ex)
        //    {

        //        throw new NotImplementedException(ex.Message);
        //        //  return BadRequest(ex.Message.ToString());
        //    }
        //}
    }
}
