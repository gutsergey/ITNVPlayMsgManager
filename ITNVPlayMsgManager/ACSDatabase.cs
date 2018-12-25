using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;

namespace ITNVPlayMsgManager
{
    public class ACSDatabase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private SqlConnection dbConn = null;
        private SqlDataReader reader = null;
        private string connStr = "";
        protected SqlCommand cmnd = null;
        public ACSDatabase(string connStr)
        {
            this.connStr = connStr;
            dbConn = new SqlConnection(connStr);

        }

        private void closeConnection()
        {
            if (reader != null && !reader.IsClosed)
                reader.Close();
            reader = null;
            if (dbConn != null && dbConn.State == System.Data.ConnectionState.Open)
                dbConn.Close();
            cmnd = null;
        }

        public string GetPhoneNumber(string compname)
        {
            log.Info("compname: " + compname);
            string phone = "";
            string sqlCmnd = "";
            sqlCmnd += "SELECT CP.ConfigData Phone, USF.FilterValue Compname ";
            sqlCmnd += "FROM [ACS].[dbo].[tblConfigPrimary] CP ";
            sqlCmnd += "inner join [ACS].[dbo].[tblTemplatePrimary] TP on TP.TemplateID= CP.TemplateID ";
            sqlCmnd += "inner join [ACS].[dbo].[tblApplication] AP on AP.AppID = TP.AppID and AP.AppName = 'CC Elite Multichannel Desktop' ";
            sqlCmnd += "inner join [ACS].[dbo].[tblTemplateSection] TS on TS.SectionID = TP.SectionID and SectionName='Telephony' ";
            sqlCmnd += "inner join [ACS].[dbo].[tblUserFilter] USF on USF.UserID = CP.UserID ";
            sqlCmnd += "inner join [ACS].[dbo].[tblFilter] FL on FL.FilterID=USF.FilterID and FL.ShortName='M' ";
            sqlCmnd += "where ConfigName='Station DN' and FilterValue='" + compname + "' ";
            log.Info("sqlCmnd: " + sqlCmnd);
            try
            {
                if (dbConn == null) dbConn = new SqlConnection(connStr);
                dbConn.Open();
                cmnd = new SqlCommand(sqlCmnd, dbConn);

                reader = cmnd.ExecuteReader();
                while (reader.Read())
                {
                    phone = reader["Phone"].ToString();
                }
                log.Info("phone: " + phone);
                return phone;
            }
            catch (Exception exc)
            {
                return "";
            }
            finally
            {
                closeConnection();
            }

        }
    }

}