using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Services;
using Newtonsoft.Json;

namespace ITNVPlayMsgManager
{
    /// <summary>
    /// Summary description for ITNVPlayMsgManager
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    public class ITNVPlayMsgManager : System.Web.Services.WebService
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [WebMethod]
        public string Version()
        {
            Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            //Context.Response.Output.Write(version.ToString());
            log.Info(version.ToString());
            return version.ToString();
        }

        [WebMethod]
        public long Heartbeat()
        {
            if (Global.HeartbeatCounter > 100000000) Global.HeartbeatCounter = 1;

            return Global.HeartbeatCounter++;
        }

        [WebMethod]
        public bool OpenSession(string source, string ivrnumber, string message, string language)
        {
            // source may be phonenumber, computer name or empty string
            // if computer name then phone number selects from ACS db if it is empty then computer name 
            // gets from the request and then from ACS db
            
            log.Info("--> source: " + source + "; ivrnumber: " + ivrnumber + "; message: " + message + "; language: " + language);
            string from = Utilities.GetPhoneNumber(source);
            language = language.ToUpper();

            if (Global.ControlDic.ContainsKey(from))
            {
                Global.ControlDic.Remove(from);
            }
            log.Info(message + ";" + language);
            Global.ControlDic.Add(from, message + ";" + language);

            bool rc = false;
            TelephonyService t = new TelephonyService();
            rc = t.conference(from, ivrnumber);
            log.Info("rs: " + rc);
            return rc;
        }

        [WebMethod]
        public bool OpenSessionWOConference(string source, string message, string language)
        {
            // source may be phonenumber, computer name or empty string
            // if computer name then phone number selects from ACS db if it is empty then computer name 
            // gets from the request and then from ACS db

            log.Info("--> source: " + source +  "; message: " + message + "; language: " + language);
            string from = Utilities.GetPhoneNumber(source);
            language = language.ToUpper();

            if (Global.ControlDic.ContainsKey(from))
            {
                Global.ControlDic.Remove(from);
            }
            log.Info(message + ";" + language);
            Global.ControlDic.Add(from, message + ";" + language);

            bool rc = true;


            return rc;
        }

        [WebMethod]
        public bool PlayMessage(string source, string message, string language)
        {
            bool brc = false;
            log.Info("--> source: " + source + "; message: " + message + "; language: " + language);
            language = language.ToUpper();

            string from = Utilities.GetPhoneNumber(source);
            log.Info("from:" + from);

            if (Global.ControlDic.ContainsKey(from))
            {
                log.Info(message + ";" + language);
                Global.ControlDic[from] = message + ";" + language;
                brc = true;
            }
            else
                brc = false;

            log.Info("brc:" + brc);
            return brc;
        }

        [WebMethod]
        public bool CloseSession(string source)
        {
            log.Info("--> source: " + source);

            string from = Utilities.GetPhoneNumber(source);
            log.Info("from:" + from);

            if (Global.ControlDic.ContainsKey(from))
            {
                Global.ControlDic.Remove(from);
            }

            return true;
        }

        [WebMethod]
        public string GetMessage(string source)
        {
            log.Info("--> source: " + source + ";");
            string from = Utilities.GetPhoneNumber(source);
            log.Info("from:" + from);

            string control = "";
            if (Global.ControlDic.ContainsKey(from))
            {
                control = Global.ControlDic[from];
                //Global.ControlDic[from] = "0;HEB";      // if ivr gets 0, it continues loop without playing
            }
            else
                control = "9;HEB";                      // if ivr gets 9, it get out from session
            log.Info("<-- control: " + control + ";");
            return control;
        }

        [WebMethod]
        public bool ChangeMessage(string source, string message)
        {
            // this func is for testing only
            log.Info("--> source: " + source + "; message: " + message);
            if (message.Trim().Length <= 0) message = "0";
            if (Global.ControlDic.ContainsKey(source))
            {
                Global.ControlDic[source] = message;
                return true;
            }
            else
                return false;
        }

        [WebMethod]
        public string GetControlDictionary()
        {
            log.Info("-->");
            string json = JsonConvert.SerializeObject(Global.ControlDic, Formatting.Indented);
            log.Info(json);
            log.Info("<--");
            return json;
        }

        [WebMethod]
        public bool SendToHuntsDic(string hd)
        {
            log.Info("-->");

            Global.HuntsDic = JsonConvert.DeserializeObject<Dictionary<string, Element>>(hd);

            var e = Global.HuntsDic.Select(x => x.Value.Extensions);

            List<string> mergedList = new List<string>();
            foreach (List<string> el in e)
            {
                mergedList = mergedList.Union(el).ToList();
            }

            foreach(string k in mergedList)
            {
                if (!Global.BusyLines.Keys.Contains(k))
                {
                    Global.BusyLines.Add(k, 0);
                }
            }

            log.Info("<--");

            return true;
        }

        [WebMethod]
        public string GetLinesStatus()
        {
            string s = JsonConvert.SerializeObject(Global.BusyLines, Formatting.None);
            return s;
        }

        [WebMethod]
        public string GetHunts()
        {
            string s = JsonConvert.SerializeObject(Global.HuntsDic, Formatting.None);
            return s;
        }

        [WebMethod]
        public bool ChangeLineStatus(string line, int status)
        {
            // status = 1 // busy
            // status = 0 // free
            bool rc;
            log.Info("-->");

            if (Global.BusyLines.Keys.Contains(line))
            {
                Global.BusyLines[line] = status;
                rc = true;
            }
            else
                rc = false;

            log.Info("<--");
            return rc;
        }

        [WebMethod]
        public bool ClearControlDictionary()
        {
            log.Info("-->");
            Global.ControlDic.Clear();
            log.Info("<--");

            return true;
        }

        [WebMethod]
        public HuntInfo GetHuntInfo(string hunt)
        {
            int total = 0;
            int free = 0;
            log.Info("--> hunt: " + hunt);
            if (Global.HuntsDic.Keys.Contains(hunt))
            {
                total = Global.HuntsDic[hunt].Extensions.Count;
                foreach(string e in Global.HuntsDic[hunt].Extensions)
                {
                    if (Global.BusyLines.Keys.Contains(e) && Global.BusyLines[e] == 0)
                    {
                        free++;
                    }
                }
            }

            HuntInfo hi = new HuntInfo(total, free);
            log.Info("<-- total: " + total + "; free: " + free);

            return hi;
        }

        [WebMethod]
        public long GetHeartbeatCounter()
        {
            log.Info("Global.HeartbeatCounter: " + Global.HeartbeatCounter);
            return Global.HeartbeatCounter;
        }

        [WebMethod]
        public void SetHeartbeatCounter(long counter)
        {
            Global.HeartbeatCounter = counter;
            log.Info("Global.HeartbeatCounter: " + Global.HeartbeatCounter);

        }

    }
}