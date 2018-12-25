using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ITNVPlayMsgManager
{
    public class AutostartProvider : System.Web.Hosting.IProcessHostPreloadClient
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public void Preload(string[] parameters)
        {
            log.Info("-->");
            Global.HeartbeatCounter = 0;

            if (Global.ControlDic == null)
            {
                Global.ControlDic = new Dictionary<string, string>();
                log.Info("ControlDic created");
            }

            if (Global.HuntsDic == null)
            {
                Global.HuntsDic = new Dictionary<string, Element>();
                log.Info("HuntsDic created");
            }

            if (Global.BusyLines == null)
            {
                Global.BusyLines = new Dictionary<string, int>();
                log.Info("BusyLines created");
            }

            Configuration.Loader();
            log.Info("<--");
        }
    }
}