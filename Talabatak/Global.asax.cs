using Sentry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Talabatak
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        private IDisposable _sentry;

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            Jobs.OrdersJobs.RunProgram().GetAwaiter().GetResult();
            Jobs.StoresJob.RunProgram().GetAwaiter().GetResult();
            HttpConfiguration config = GlobalConfiguration.Configuration;

            config.Formatters.JsonFormatter
                        .SerializerSettings
                        .ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            //_sentry = SentrySdk.Init(o =>
            //{
            //    o.Dsn = new Dsn("");
            //});
        }

        protected void Application_Error()
        {
            var exception = Server.GetLastError();
            SentrySdk.CaptureException(exception);
        }

        protected void Application_End()
        {
            _sentry?.Dispose();
        }
    }
}
