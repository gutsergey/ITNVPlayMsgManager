using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace ITNVPlayMsgManager
{
    public class Global : System.Web.HttpApplication
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static long HeartbeatCounter = 0;

        public static Dictionary<string, string> ControlDic = null;

        public static Dictionary<string, Element> HuntsDic = null;
        public static Dictionary<string, int> BusyLines = null;

        protected void Application_Start(object sender, EventArgs e)
        {
            log.Info("-->");
            HeartbeatCounter = 0;

            if (ControlDic == null)
            {
                ControlDic = new Dictionary<string, string>();
                log.Info("ControlDic created");
            }

            if (HuntsDic == null)
            {
                HuntsDic = new Dictionary<string, Element>();
                log.Info("HuntsDic created");
            }

            if (BusyLines == null)
            {
                BusyLines = new Dictionary<string, int>();
                log.Info("BusyLines created");
            }

            Configuration.Loader();
            log.Info("<--");
        }

        protected void Session_Start(object sender, EventArgs e)
        {
            Configuration.Loader();
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {
            log.Info("-->");
            if (ControlDic != null)
            {
                ControlDic.Clear();
                ControlDic = null;

                HuntsDic.Clear();
                HuntsDic = null;

                BusyLines.Clear();
                BusyLines = null;

                log.Info("All global objects are null now");
            }
            log.Info("<--");
        }
    }
}