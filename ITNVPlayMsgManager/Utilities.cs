using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Net;

namespace ITNVPlayMsgManager
{
    public static class Utilities
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static string GetPhoneNumber(string source)
        {
            // source may be a phone number or a computer name or empty string

            string from = "";
            if (source.Length <= 0)
            {
                from = GetFromExtension();
            }
            else
            {
                // Regex checks if it is comp name or phoone 
                string pattern = @"^\d+$";  // the sourse is numeric
                Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
                Match match = regex.Match(source);

                if (!match.Success)
                {
                    from = GetFromExtension(source);

                }
                else
                    from = source;
            }
            log.Info("from: " + from);
            return from;
        }

        private static string GetFromExtension()
        {
            string compname = GetCompName();
            log.Info("compname:" + compname);

            string from = GetFromExtension(compname);

            return from;
        }

        private static string GetFromExtension(string compname)
        {
            string from = GetPhone(compname);
            log.Info("from:" + from);

            return from;
        }

        private static string GetPhone(string compname)
        {
            log.Info("compname: " + compname);
            ACSDatabase acsdb = new ACSDatabase(Configuration.Connstring);

            string phone = acsdb.GetPhoneNumber(compname);
            log.Info("phone: " + phone);
            return phone.Trim();
        }

        private static string GetCompName()
        {
            //string hostName = Dns.GetHostByAddress(GetIPAddress()).HostName;
            string hostName = Dns.GetHostEntry(GetIPAddress()).HostName;
            log.Info("hostName:" + hostName);
            return hostName;
        }

        private static string GetIPAddress()
        {
            System.Web.HttpContext context = System.Web.HttpContext.Current;
            string ipAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (!string.IsNullOrEmpty(ipAddress))
            {
                string[] addresses = ipAddress.Split(',');
                if (addresses.Length != 0)
                {
                    return addresses[0];
                }
            }
            ipAddress = context.Request.ServerVariables["REMOTE_ADDR"];
            log.Info("ipaddress:" + ipAddress);
            return ipAddress;
        }

    }
}