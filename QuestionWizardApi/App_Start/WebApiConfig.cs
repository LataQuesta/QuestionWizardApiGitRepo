using QuestionWizardApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.ExceptionHandling;

namespace QuestionWizardApi
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
          //   config.EnableCors(new EnableCorsAttribute("*", headers: "*", methods: "*"));
            // Web API routes
            config.MapHttpAttributeRoutes();
            //Registering GlobalExceptionHandler
          config.Services.Replace(typeof(IExceptionHandler), new GlobalExceptionHandler());
            //Registering UnhandledExceptionLogger
            config.Services.Replace(typeof(IExceptionLogger), new UnhandledExceptionLogger());
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
         
            //   config.Formatters.Remove(config.Formatters.JsonFormatter);
        }
    }
}
