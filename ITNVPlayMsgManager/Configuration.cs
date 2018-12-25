using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ITNVPlayMsgManager
{
    public class Configuration
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static string connstring;
        private static string aesserverip;
        private static string aesusername;
        private static string aespassword;
        private static string cmswitchname;
        private static bool useSSL;

        public static string Connstring { get { return connstring; } }

        public static string Aesserverip { get { return aesserverip; } }

        public static string Aesusername { get { return aesusername; } }

        public static string Aespassword { get { return aespassword; } }

        public static string Cmswitchname { get { return cmswitchname; } }

        public static bool UseSSL { get { return useSSL; } }

        public static void Loader()
        {
            connstring = Cfg.GetConnectionString("ACS");
            log.Info("connstring: " + connstring);

            aesserverip = Cfg.GetValue("aesserverip");
            log.Info("aesserverip: " + aesserverip);

            aesusername = Cfg.GetValue("aesusername");
            log.Info("aesusername: " + aesusername);

            aespassword = Cfg.GetValue("aespassword");
            log.Info("aespassword: " + aespassword);

            cmswitchname = Cfg.GetValue("cmswitchname");
            log.Info("cmswitchname: " + cmswitchname);

            if (!bool.TryParse(Cfg.GetValue("useSSL"), out useSSL))
                useSSL = true;

            log.Info("useSSL: " + useSSL);
        }
    }

}