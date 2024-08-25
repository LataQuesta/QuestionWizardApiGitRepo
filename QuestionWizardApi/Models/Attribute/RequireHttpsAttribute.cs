using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Net;

namespace QuestionWizardApi.Models
{
    public class RequireHttpsAttribute : AuthorizationFilterAttribute
    {
        public override void OnAuthorization(HttpActionContext actioncontext)
        {
            if(actioncontext.Request.RequestUri.Scheme !=  Uri.UriSchemeHttps)
            {
                actioncontext.Response = actioncontext.Request.CreateResponse(System.Net.HttpStatusCode.Found);
                actioncontext.Response.Content = new StringContent("<p>Use HTTPS instead of HTTP",Encoding.UTF8,"text/html");

                UriBuilder uriBuilder = new UriBuilder(actioncontext.Request.RequestUri);
                uriBuilder.Scheme = Uri.UriSchemeHttps;
                uriBuilder.Port = 44316;

                actioncontext.Response.Headers.Location = uriBuilder.Uri;
            }
            else
            {
                base.OnAuthorization(actioncontext);
            }

        }


        public class CustomAuthorizeAttribute : AuthorizationFilterAttribute
        {

            public override Task OnAuthorizationAsync(HttpActionContext actionContext, System.Threading.CancellationToken cancellationToken)
            {
               // var identity = (ClaimsIdentity)User.Identity;

                var principal = actionContext.RequestContext.Principal as ClaimsPrincipal;
                var Parameter = actionContext.ControllerContext.RouteData.Values;


                if (!principal.Identity.IsAuthenticated)
                {
                    return Task.FromResult<object>(null);
                }

                int CurrentTestIdObj = !string.IsNullOrEmpty(principal.FindFirst("username").Value) ? Convert.ToInt32(principal.FindFirst("username").Value) : 0; //UserSrv.GetLatestTestId(emailId);


              //  var userName = string.IsNullOrEmpty(principal.FindFirst("username").Value);// principal.FindFirst(ClaimTypes.Name).Value;
                var userAllowedTime = principal.FindFirst("LoggedOn").Value;

                var CurrentTestId = Parameter.Where(x => x.Key == "TestId").Select(x => x.Value).FirstOrDefault();

                if(CurrentTestId != null)
                {
                    if (CurrentTestId.ToString() != CurrentTestIdObj.ToString())
                    {
                        actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized, "Not allowed to access...bla bla");
                        return Task.FromResult<object>(null);
                    }
                }
                
                //if (DateTime.Now.ToString() != userAllowedTime)
                //{
                //    actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized, "Not allowed to access...bla bla");
                //    return Task.FromResult<object>(null);
                //}

                //User is Authorized, complete execution
                return Task.FromResult<object>(null);

            }
        }



    }
}