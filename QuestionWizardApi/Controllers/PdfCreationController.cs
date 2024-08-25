
using QuestionWizardApi.CorporateIBusinessLayer.Interface;
using QuestionWizardApi.CorporateModel.Model;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace QuestionWizardApi.Controllers
{
    public class PdfCreationController : ApiController
    {
        private IUser UserSrv = null;

        private IPremiumRpt MailSrv1 = null;
        private IMailH1 MailH1 = null;
        private IMail MailSrv = null;
        // private IMail MailSrv = null;

        public PdfCreationController(IPremiumRpt MailSrv1, IMailH1 MailH1, IUser UserSrv, IMail MailSrv)
        {
            this.MailSrv1 = MailSrv1;
            this.MailSrv = MailSrv;
            this.MailH1 = MailH1;
            this.UserSrv = UserSrv;
        }
        [HttpGet]
        [Route("api/PDF/QmapReport/{TestId}")]
        public IHttpActionResult CreatePdf(int TestId)
        {
            string hostname = System.Web.HttpContext.Current.Request.Url.Host;

            MailH1.SendIndividualQMapReport(TestId, hostname);

            //MailH1.GeneratePDF(TestId);
            // try
            // {
            //   //  MailBM ObjMailBody = new MailBM();

            ////     CandidateBM UserModel = UserSrv.GetCandidateData(10001);
            ////     MailBM objMailBody = new MailBM();
            //   //  objMailBody = MailSrv.GetMailConfig(10001, UserModel.ProfileId.Value, false, true);
            ////     MailH1.FinalSentMail(objMailBody, "10001", "");

            /////     PDFCreationSvc obj = new PDFCreationSvc();
            //    // obj.CreatePDF("10001");
            // //    obj.DemoPDFUsingBoot();
            //   //  obj.GenerateHTMLToPdf();
            //   //  byte[] abc = MailSrv.GeneratePremiumRpt("Divya Kaul", 10001, 9, 2, 3);

            // }
            return Ok("Report has been send to following email id support@questa.in");
        }

        [HttpGet]
        [Route("api/PDF/QLeapReport/{TestId}")]
        public IHttpActionResult GenerateQLeapReport(int TestId)
        {
            string hostname = System.Web.HttpContext.Current.Request.Url.Host;

            MailH1.SendIndividualQLeapReport(TestId, hostname);

            return Ok("Report has been send to following email id support@questa.in");
        }

        [HttpGet]
        [Route("api/PDF/QssrReport/{TestId}")]
        public IHttpActionResult GenerateQssrReport(int TestId)
        {
            string hostname = System.Web.HttpContext.Current.Request.Url.Host;

            MailH1.SendIndividualQSSSRReport(TestId, hostname);
            
            return Ok("Report has been send to following email id support@questa.in");
        }

        [HttpGet]
        [Route("api/PDF/StandardReport/{TestId}")]
        public IHttpActionResult GenerateStandardReport(int TestId)
        {
            string hostname = System.Web.HttpContext.Current.Request.Url.Host;

            MailH1.SendIndividualStandardReport(TestId, hostname);

            return Ok("Report has been send to following email id support@questa.in");
        }
    }
}
