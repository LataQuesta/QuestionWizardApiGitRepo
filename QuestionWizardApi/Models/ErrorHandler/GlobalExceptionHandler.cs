using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using System.Web;
using System.Net.Http;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace QuestionWizardApi.Models
{
    public class GlobalExceptionHandler : ExceptionHandler
    {
        public override void Handle(ExceptionHandlerContext context)
        {
            dynamic result ;
            if(context.Exception.Message.Contains("Timeout expired.") || context.Exception.Message.Contains("provider: Named Pipes Provider, error: 40 - Could not open a connection to SQL Server"))
            {
                result = new HttpResponseMessage(HttpStatusCode.GatewayTimeout)
                {
                    Content = new StringContent(context.Exception.Message),
                    ReasonPhrase = "Exception"
                };
            }
            else
            {
                 result = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(context.Exception.Message),
                    ReasonPhrase = "Exception"
                };
            }
            

            context.Result = new ErrorMessageResult(context.Request, result);
        }

        public class ErrorMessageResult : IHttpActionResult
        {
            private HttpRequestMessage _request;
            private readonly HttpResponseMessage _httpResponseMessage;

            public ErrorMessageResult(HttpRequestMessage request, HttpResponseMessage httpResponseMessage)
            {
                _request = request;
                _httpResponseMessage = httpResponseMessage;
            }

            public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
            {
                return Task.FromResult(_httpResponseMessage);
            }
        }
    }
}