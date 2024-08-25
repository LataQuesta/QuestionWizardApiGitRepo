[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(QuestionWizardApi.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(QuestionWizardApi.App_Start.NinjectWebCommon), "Stop")]

namespace QuestionWizardApi.App_Start
{
    using System;
    using System.Web;
    using Microsoft.Web.Infrastructure.DynamicModuleHelper;

    using Ninject;
    using Ninject.Web.Common;
    
    public static class NinjectWebCommon 
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start() 
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            bootstrapper.Initialize(CreateKernel);
        }
        
        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            bootstrapper.ShutDown();
        }
        
        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            try
            {
                kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
                kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();
                System.Web.Http.GlobalConfiguration.Configuration.DependencyResolver = new Ninject.WebApi.DependencyResolver.NinjectDependencyResolver(kernel);

                RegisterServices(kernel);
                return kernel;
            }
            catch
            {
                kernel.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
          //  kernel.Bind<Repository<txnQuestion>>().To<QuestionService>();
            //kernel.Bind<StudentIBusinessLayer.IError>().To<StudentBusinessLayer.Service.ErrorService>().InRequestScope();
            //kernel.Bind<StudentIBusinessLayer.IMail>().To<StudentBusinessLayer.Service.MailService>().InRequestScope();
            //kernel.Bind<StudentIBusinessLayer.IMasterData>().To<StudentBusinessLayer.Service.MasterDataService>().InRequestScope();
            //kernel.Bind<StudentIBusinessLayer.IQuestion>().To<StudentBusinessLayer.Service.QuestionService>().InRequestScope();
            //kernel.Bind<StudentIBusinessLayer.IUser>().To<StudentBusinessLayer.Service.UserService>().InRequestScope();


          //  kernel.Bind<CorporateIBusinessLayer.Interface.IError>().To<CorporateBusinessLayer.Service.ErrorService>().InRequestScope();
            kernel.Bind<CorporateIBusinessLayer.Interface.IMail>().To<CorporateBusinessLayer.Service.MailService>().InRequestScope();
            kernel.Bind<CorporateIBusinessLayer.Interface.IMasterData>().To<CorporateBusinessLayer.Service.MasterDataService>().InRequestScope();
            kernel.Bind<CorporateIBusinessLayer.Interface.IQuestion>().To<CorporateBusinessLayer.Service.QuestionService>().InRequestScope();
            kernel.Bind<CorporateIBusinessLayer.Interface.IUser>().To<CorporateBusinessLayer.Service.UserService>().InRequestScope();
            kernel.Bind<CorporateIBusinessLayer.Interface.IPremiumRpt>().To<CorporateBusinessLayer.Service.PremiumRpt>().InRequestScope();
            kernel.Bind<CorporateIBusinessLayer.Interface.IMailH1>().To<CorporateBusinessLayer.Service.MailServiceForH1>().InRequestScope();
            kernel.Bind<CorporateIBusinessLayer.Interface.IExport>().To<CorporateBusinessLayer.Service.ExportScoreData>().InRequestScope();
            kernel.Bind<CorporateIBusinessLayer.Interface.IReportGeneration>().To<CorporateBusinessLayer.Service.ReportGeneration>().InRequestScope();
            kernel.Bind<CorporateIBusinessLayer.Interface.IAwsConsole>().To<CorporateBusinessLayer.Service.AwsConsole>().InRequestScope();




        }
    }
}
