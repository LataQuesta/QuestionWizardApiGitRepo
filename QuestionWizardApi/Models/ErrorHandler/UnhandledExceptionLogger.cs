using QuestionWizardApi.CorporateBusinessLayer.Service;
using QuestionWizardApi.CorporateIBusinessLayer.Interface;
using QuestionWizardApi.CorporateModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http.ExceptionHandling;

namespace QuestionWizardApi.Models
{
    public class UnhandledExceptionLogger : ExceptionLogger
    {
        ErrorService objErrorSvc = new ErrorService();
        public override void Log(ExceptionLoggerContext context)
        {
            var ex = context.Exception;

            string strLogText = "";
            strLogText += Environment.NewLine + "Source ---\n{0}" + ex.Source;
            strLogText += Environment.NewLine + "StackTrace ---\n{0}" + ex.StackTrace;
            strLogText += Environment.NewLine + "TargetSite ---\n{0}" + ex.TargetSite;

            if (ex.InnerException != null)
            {
                strLogText += Environment.NewLine + "Inner Exception is {0}" + ex.InnerException;//error prone
            }
            if (ex.HelpLink != null)
            {
                strLogText += Environment.NewLine + "HelpLink ---\n{0}" + ex.HelpLink;//error prone
            }

            var requestedURi = (string)context.Request.RequestUri.AbsoluteUri;
            var requestMethod = context.Request.Method.ToString();
            var timeUtc = DateTime.Now;

            var routeData = context.RequestContext.RouteData;
            if (routeData != null)
            {
                if(routeData.Values.ContainsKey("AreaName"))
                {
                    string areaName = routeData.Values["AreaName"].ToString();
                }
                
            }

            //ErrorLogBM apiError = new ErrorLogBM()
            //{
            //    Message = strLogText,
            //    ErrorMsg = ex.Message,
            //    RequestUri = requestedURi,
            //    RequestMethod = requestMethod,
            //    TimeUtc = DateTime.Now
            //};

            //// DbErrorLogSv = new DbErrorLog(ErrorSvc);
            //objErrorSvc.SaveErrorLog(apiError);
        }

        

    }
}