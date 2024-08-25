
using QuestionWizardApi.CorporateIBusinessLayer.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace QuestionWizardApi.Controllers
{
   // [RoutePrefix("api/CorporateExportScore")]
    public class ExportScoreController : ApiController
    {
        private IExport ExportSrv = null;

        public ExportScoreController(IExport ExportSrv)
        {
            this.ExportSrv = ExportSrv;
        }

        [HttpGet]
         [Route("api/ExportScore/ExportToExcelAllData/{TestId}")]
       // [Route("api/CorporateExportScore/ExportExcelFile/{TestId}")]
        public HttpResponseMessage CorporateExportExcelFile(int TestId)
        {
            HttpResponseMessage response = null;

            MemoryStream stream = new MemoryStream();
            stream = ExportSrv.GetScoreCard(TestId);

            if (stream != null)
            {
                string name = ExportSrv.GetName(TestId);



                stream.Seek(0, SeekOrigin.Begin);

                response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StreamContent(stream);
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                response.Content.Headers.ContentDisposition.FileName = name + ".xlsx";
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

                response.Content.Headers.ContentLength = stream.Length;
                stream.Seek(0, SeekOrigin.Begin);


                return response;
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Data Not Found For Respective Candidate");
            }
        }

        [HttpGet]
          [Route("api/CorporateExportScore/ExportQsserCandidateDetails")]
       // [Route("api/CorporateExportScore/GetQsserCandidateRc")]
        public HttpResponseMessage GetQsserCandidateRecord()
        {
            HttpResponseMessage response = null;

            MemoryStream stream = new MemoryStream();
            stream = null;//ExportSrv.GetScoreCardDataForQSSER();

            if (stream != null)
            {

                stream.Seek(0, SeekOrigin.Begin);

                response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StreamContent(stream);
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                response.Content.Headers.ContentDisposition.FileName = "QsserCandidateRecords.xlsx";
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

                response.Content.Headers.ContentLength = stream.Length;
                stream.Seek(0, SeekOrigin.Begin);


                return response;
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Data Not Found");
            }
        }


        [HttpGet]
        [Route("api/CorporateExportScore/ExportQleapCandidateDetails")]
        public HttpResponseMessage GetQLeapCandidateRecord()
        {
            HttpResponseMessage response = null;

            MemoryStream stream = new MemoryStream();
            stream = null;//ExportSrv.GetScoreCardDataForQLeap();

            if (stream != null)
            {

                stream.Seek(0, SeekOrigin.Begin);

                response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StreamContent(stream);
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                response.Content.Headers.ContentDisposition.FileName = "QLeapCandidateRecords.xlsx";
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

                response.Content.Headers.ContentLength = stream.Length;
                stream.Seek(0, SeekOrigin.Begin);


                return response;
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Data Not Found");
            }
        }

        [HttpGet]
        [Route("api/CorporateExportScore/ExportQLeadCandidateDetails")]
        public HttpResponseMessage GetQLeadCandidateRecord()
        {
            HttpResponseMessage response = null;

            MemoryStream stream = new MemoryStream();
            stream = null;//ExportSrv.GetScoreCardDataForQLead();

            if (stream != null)
            {

                stream.Seek(0, SeekOrigin.Begin);

                response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StreamContent(stream);
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                response.Content.Headers.ContentDisposition.FileName = "QLeadCandidateRecords.xlsx";
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

                response.Content.Headers.ContentLength = stream.Length;
                stream.Seek(0, SeekOrigin.Begin);


                return response;
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Data Not Found");
            }
        }
    }

    


}
