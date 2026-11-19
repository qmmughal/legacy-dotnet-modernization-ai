using System;
using System.Web;
using log4net;

namespace MeridianPortal.Web
{
    public class Global : HttpApplication
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Global));

        protected void Application_Start(object sender, EventArgs e)
        {
            log4net.Config.XmlConfigurator.Configure();
            Log.Info("Meridian portal starting");
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var ex = Server.GetLastError();
            Log.Error("Unhandled error on " + Request.RawUrl, ex);
        }
    }
}
