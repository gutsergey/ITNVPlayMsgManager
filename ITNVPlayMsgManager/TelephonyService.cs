using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ITNVPlayMsgManager.TelService;

using System.ServiceModel;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using System.Net.Security;
using System.Web.Services.Protocols;

namespace ITNVPlayMsgManager
{
    public class TelephonyService
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private bool useSSL = true;
        private string serviceIP;
        private Uri serviceURL = null;
        private TelephonyServiceService service = null;
        private @string SID;
        private bool faultRaised = false;

        public TelephonyService()
        {
            useSSL = Configuration.UseSSL;
            ////Trust all certificates
            System.Net.ServicePointManager.ServerCertificateValidationCallback = ((sender, certificate, chain, sslPolicyErrors) => true);

            // trust sender
            //ServicePointManager.ServerCertificateValidationCallback = ((sender, cert, chain, errors) => cert.Subject.Contains(aesServerIP));

            // validate cert by calling a function
            ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(ValidateRemoteCertificate);
            setup(Configuration.Aesserverip, Configuration.Aesusername, Configuration.Aespassword, Configuration.Cmswitchname);
            attachSesion();
        }

        private static bool ValidateRemoteCertificate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors policyErrors)
        {
            bool result = false;
            //if (cert.Subject.Contains("AesServerCert"))
            //{
            //    result = true;
            //}
            result = true;
            return result;
        }

        private void setup(string aesServer, string user, string pw, string switchName)
        {
            log.Info("--->");
            serviceIP = aesServer;

            // Locate the Telephony Service 
            service = new TelephonyServiceService();

            // define soap protocol... 
            service.SoapVersion = System.Web.Services.Protocols.SoapProtocolVersion.Soap11;

            // define the encoding... 
            service.RequestEncoding = System.Text.Encoding.UTF8;

            try
            {
                if (useSSL)
                {
                    serviceURL = new Uri("https://" + serviceIP + "/axis/services/TelephonyService");
                }
                else
                {
                    serviceURL = new Uri("http://" + serviceIP + "/axis/services/TelephonyService");
                }

                log.Info(serviceURL.ToString());
                // Bind to a port on the AES server 
                service.Url = serviceURL.AbsoluteUri;

                // Set a timeout for the web service response. Here we allow 10000ms(10s) 
                service.Timeout = 1000000;

            }
            catch (UriFormatException e)
            {
                log.Error("Bad URL (" + serviceIP + "/axis/services/TelephonyService)");
                log.Error(e.Message);
                log.Error(e.StackTrace);
            }
            catch (Exception e)
            {
                log.Error("Error accessing URL");
                log.Error(e.Message);
                log.Error(e.StackTrace);
            }

            // Set the login and credentials in the HTTP authorization header (these will also be written to all subsequent requests) 
            // If Authentication is failing are we using a proxy.. 
            service.Credentials = new System.Net.NetworkCredential(user + "@" + switchName, pw);
            service.PreAuthenticate = true;

            // set initial soap header 
            // define Session ID to be null initially 
            SID = new @string();
            SID.Text = new string[] { "0" };

            // Set some properties expected by the server 
            SID.Actor = "http://schemas.xmlsoap.org/soap/actor/next";

            // Set the sessionID element in the SOAP header (this causes it to be written to all subsequent requests) 
            service.sessionID = SID;
            log.Info("<---");
        }

        /** 
        * Send an attach request. This is used to initiate the session. It's an 
        * optional step, but if you don't do it the first actual request will take 
        * longer, and you won't be able to authenticate the user until they 
        * actually invoke an operation. 
        */
        private void attachSesion()
        {
            System.Console.WriteLine("--->");
            try
            {
                service.attach(null);
                faultRaised = false;
            }
            catch (SoapException soapE)
            {
                // A fault was raised. The fault message will contain the explanation 
                System.Console.WriteLine("A SOAP fault was raised: ");

                if (soapE.Message != null)
                { log.Error("Code: " + soapE.Code.Name); }
                if (soapE.Message != null)
                { log.Error("Message: " + soapE.Message); }
                if (soapE.Detail != null)
                { log.Error("Detail: " + soapE.Detail.InnerXml); }


                SID = service.sessionID;
                faultRaised = true;
            }
            catch (Exception re)
            {
                // A fault was raised. The fault message will contain the explanation 
                log.Error("An unexpected Exception was raised: " + re);
                faultRaised = true;
            }

            // Set the session ID for the next request. 
            manageSession();
            log.Info("<---");
        }

        public bool makeCall(string from, string to)
        {
            log.Info("---> from:" + from + "; to:" + to);
            bool rc = phoneOperation("makecall", from, to);
            log.Info("<--- ");
            return rc;

        }

        public bool conference(string from, string to)
        {
            log.Info("---> from:" + from + "; to:" + to);
            bool rc = phoneOperation("conference", from, to);
            log.Info("<--- ");
            return rc;

        }

        public bool transfer(string from, string to)
        {
            log.Info("---> from:" + from + "; to:" + to);
            bool rc = phoneOperation("transfer", from, to);
            log.Info("<--- ");
            return rc;

        }

        public bool answer(string ext)
        {
            log.Info("---> ext:" + ext);
            bool rc = phoneOperation("answer", ext, "");
            log.Info("<--- ");
            return rc;

        }

        public bool disconnect(string ext)
        {
            log.Info("---> ext:" + ext);
            bool rc = phoneOperation("disconnect", ext, "");
            log.Info("<--- ");
            return rc;

        }

        private bool phoneOperation(string operation, string ext1, string ext2)
        {
            log.Info("---> operation:" + "; ext1:" + ext1 + "; ext2:" + ext2);
            try
            {
                endpoints ep = new endpoints();
                ep.originatingExtension = ext1;
                ep.destinationNumber = ext2;

                extension ex = new extension();
                ex.extension1 = ext1;

                switch (operation)
                {
                    case "makecall":
                        service.makeCall(ep);
                        break;
                    case "answer":
                        service.answerAlertingCall(ex);
                        break;
                    case "disconnect":
                        service.disconnectActiveCall(ex);
                        break;
                    case "conference":
                        service.singleStepConferenceCall(ep);
                        break;
                    case "transfer":
                        service.singleStepTransferCall(ep);
                        break;
                    default:
                        faultRaised = true;
                        break;
                }

            }
            catch (Exception e)
            {
                log.Error("Error invoking phone operation to the Telephony Web service " + e);
                log.Error(e.StackTrace);
                faultRaised = true;
            }

            // Set the session ID for the next request. 
            manageSession();

            releaseSession();
            log.Info("<--- ");
            return !faultRaised;
        }

        private void manageSession()
        {
            // If the last request raised a fault, then there is no session response header, and no change 
            // to session management information 
            if (!faultRaised)
            {
                try
                {
                    /** 
                    * Set the sessionID in the header for the next request. This should be 
                    * done after every operation, as the session may have timed out between 
                    * requests, resulting in a new session ID. 
                    */

                    // retrieve resulting Session ID 
                    SID = service.sessionID;

                    // Copy the sessionID from the response header back into our request header to be supplied 
                    // with the next request. This fulfills the client obligation with respect to session management 
                    service.sessionID = SID;

                    log.Info("setHeader(): sessionID=" + SID.Text[0]);

                }
                catch (Exception se)
                {
                    log.Error(" Error setting the session ID" + se);
                }
            }
        }

        private void releaseSession()
        {
            log.Info("Release session");
            service.release(null);
        }
    }

}