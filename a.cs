using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Data;
using System.Data.SqlClient;
//using Excel = Microsoft.Office.Interop.Excel;
//using Microsoft.Office.Core;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Xml;
using System.Net.Sockets;
using System.Diagnostics;
using BarcodeLib;
using ZXing;
//using ThoughtWorks.QRCode.Codec;
using System.Drawing;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Configuration;
using System.Collections;
using System.Web.Security;
using System.Drawing.Imaging;
using MessagingToolkit.QRCode.Codec;
using MessagingToolkit.QRCode.Codec.Data;
using System.DirectoryServices;
using System.Runtime.InteropServices;
using PrintReceiptClient;
using System.Web.Configuration;
using System.Text.RegularExpressions;
//using PrintReceiptClient;
using TellerWebservice.Model;
using System.Globalization;

namespace TellerWebservice
{
    /// <summary>
    /// Summary description for Service1
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class Service1 : System.Web.Services.WebService
    {

        private SqlConnection getConnection()
        {
            //hardcode connection  sau nho bo caont
            //SqlConnection con = new SqlConnection("Server=10.1.14.240;Database=eCounter;User Id=eCounter;Password=ec@#123;Max Pool Size=500");
            SqlConnection con = new SqlConnection(GetConnStr());
            return con;
        }

        //  private static eCounterWebReference1.ECounterWebserviceVer5 mWS = new eCounterWebReference1.ECounterWebserviceVer5();
        // private string xml = mWS.requestGetInforFromServer("SERVERLOCAL", "1.0.0");
        // public  string g_ConnectionString = "Server=@IP;Database=eCounter;User Id=eCounter;Password=ec@#123;Max Pool Size=500";
        public static string g_ConnectionString = "";
        private static string PATH_VERSION = @"C:\ecounter\EC_Teller Webservice\version.txt";
        private static string BRANCH_CODE = "";
        private static string BRANCH_NAME = "";
        private static string DATE_TIME_FORMAT = "yyyy-MM-dd HH:mm:ss.fff";


        private string GetConnStr()
        {
            //caont add new
            eCounterWebReference1.ECounterWebserviceVer5 mWS = new eCounterWebReference1.ECounterWebserviceVer5();
            Utils.Log.WriteLog("URL CALL SERVER PUBLIC :  " + mWS.Url);
            string VersionClient = "";

            if (Utils.Log.checkFileExit_New(PATH_VERSION))
            {
                VersionClient = System.IO.File.ReadAllText(PATH_VERSION);
            }
            else
                VersionClient = "1.0.0";


            if (!g_ConnectionString.Equals("") && g_ConnectionString.Length > 0 && g_ConnectionString.ToLower().Contains("Server=".ToLower()) && g_ConnectionString.ToLower().Contains(";Database=".ToLower()))
            {

                //  Utils.Log.WriteLog(" return ConnectionString khong request server : " + g_ConnectionString);
                return g_ConnectionString;

            }
            else
            {

                string xml = mWS.requestGetInforFromServer("BOOKING", VersionClient);
                string l_ConnectionString = "Server=@IP;Database=eCounter;User Id=eCounter;Password=ec@#123;Max Pool Size=500";

                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(xml);
                string ip = xDoc.GetElementsByTagName("IP_SERVER")[0].InnerText;
                BRANCH_CODE = xDoc.GetElementsByTagName("BRN_CODE")[0].InnerText;
                BRANCH_NAME = xDoc.GetElementsByTagName("BRANCH_NAME")[0].InnerText;
                l_ConnectionString = l_ConnectionString.Replace("@IP", ip);

                g_ConnectionString = l_ConnectionString;

                Utils.Log.WriteLog(" return ConnectionString : " + g_ConnectionString);




                return g_ConnectionString;

                //hard code test
                //  return "Server=10.1.14.240;Database=eCounter;User Id=eCounter;Password=ec@#123;Max Pool Size=500";

            }


            //return System.Configuration.ConfigurationManager.ConnectionStrings["SQLConnection"].ConnectionString;
        }






        [WebMethod]
        public string GetJson_Currency()
        {
            string xml = "";

            try
            {

                GoldWebservice.Service goldWS = new GoldWebservice.Service();
                xml = goldWS.GetJson_Currency();

            }
            catch (Exception ex)
            {
                xml = "ERROR: " + ex.Message;
            }

            return xml;
        }

        [WebMethod]
        public string GetJson_Gold(string channel)
        {
            string xml = "";

            try
            {

                GoldWebservice.Service goldWS = new GoldWebservice.Service();
                xml = goldWS.GetJson_Gold(channel);

            }
            catch (Exception ex)
            {
                xml = "ERROR: " + ex.Message;
            }

            return xml;
        }

        [WebMethod]
        public List<string> ReceiveSmartCardInfo(string RFIDReceiverId, string RFIDChanelId, string smartCardId)
        {
            //argumentsCheck
            List<string> argumentsCheck = new List<string>();
            List<string> responseArgs = new List<string>();
            List<string> response = new List<string>();


            eCounterWebReference1.ECounterWebserviceVer5 eCounterWR = new eCounterWebReference1.ECounterWebserviceVer5();
            string xml = "";
            string refCode = "";
            DataSet ds = new DataSet();
            SqlConnection con = getConnection();
            SqlCommand cmd;
            SqlDataAdapter _da;
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            try
            {
                string xmlINFO = eCounterWR.LookupFCCCustomerInfoBySmartCardId(smartCardId);
                cmd = new SqlCommand("sp_SaveDetectedCustomerInfo", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SMARTCARD_ID", smartCardId);
                cmd.Parameters.AddWithValue("@IS_DETECTED_BY_GREETER", "N");
                cmd.Parameters.AddWithValue("@DEVICE_COM_PORT", RFIDReceiverId);
                cmd.Parameters.AddWithValue("@RFID_CHANEL", RFIDChanelId);
                cmd.Parameters.AddWithValue("@CUSTOMER_IDENTIFY_NO", "000");
                cmd.Parameters.AddWithValue("@xmlINFO", xmlINFO);
                _da = new SqlDataAdapter(cmd);
                _da.Fill(ds);
                DataTable dt = ds.Tables[0];
                DataTable dt1 = ds.Tables[1];
                DataTable dt2 = ds.Tables[2];

                refCode = dt.Rows[0]["REF_CODE"].ToString();
                bool isHasCustomerInfor = false;
                string cusNo = "";
                string type = "NORMAL";
                int customerTrackingId = 0;

                if (dt1.Rows.Count > 0)
                {
                    isHasCustomerInfor = true;
                    cusNo = dt1.Rows[0]["CUSTOMER_CIF"].ToString();
                    string cusIdentifyNo = dt1.Rows[0]["CUSTOMER_IDENTIFIER"].ToString();
                    string cusSmartCardId = dt1.Rows[0]["CUSTOMER_SMARTCARD_ID"].ToString();
                    string cusName = dt1.Rows[0]["CUSTOMER_NAME"].ToString();
                    string sex = dt1.Rows[0]["CUSTOMER_SEX"].ToString();
                    type = dt1.Rows[0]["CUSTOMER_TYPE"].ToString();
                    string commingTime = dt1.Rows[0]["DETECTED_TIME"].ToString();
                    string detectedBy = dt1.Rows[0]["IS_DETECTED_BY_GREETER"].ToString() == "Y" ? "GREETER" : "AUTO SYSTEM";
                    string lastLocation = dt1.Rows[0]["DETECT_POINT_ID"].ToString();
                    string isServing = dt1.Rows[0]["IS_SERVING"].ToString();
                    customerTrackingId = Convert.ToInt32(dt1.Rows[0]["CUSTOMER_TRACKING_ID"].ToString());
                    string bookedServiceCount = dt1.Rows[0]["BOOKED_SERVICES_COUNT"].ToString();
                    if (cusNo != null && cusNo.Trim().Length > 0)
                    {
                        responseArgs.Add(cusNo + "#" + cusName + "#" + sex
                                + "#" + type + "#" + commingTime + "#"
                                + detectedBy + "#" + lastLocation + "#"
                                + isServing + "#"
                                + "CI#"
                                + bookedServiceCount);

                    }
                    else if (cusIdentifyNo != null && cusIdentifyNo.Trim().Length > 0)
                    {
                        responseArgs.Add(cusIdentifyNo + "#" + cusName + "#"
                                + sex + "#" + type + "#" + commingTime
                                + "#" + detectedBy + "#" + lastLocation
                                + "#" + isServing + "#"
                                + "CM#"
                                + bookedServiceCount);
                    }
                    else
                    {
                        responseArgs.Add(cusSmartCardId + "#" + cusName + "#"
                                + sex + "#" + type + "#" + commingTime
                                + "#" + detectedBy + "#" + lastLocation
                                + "#" + isServing + "#"
                                + "SC#" + bookedServiceCount);
                    }
                }

                if (isHasCustomerInfor)
                {
                    if (dt2.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt2.Rows.Count; i++)
                        {
                            responseArgs.Add(dt2.Rows[i]["BOOKED_SERVICE_INFO"].ToString());
                        }
                    }
                }

                // importance	
                string[] refCodeArr = refCode.Split('#');
                if (responseArgs.Count > 0 && isHasCustomerInfor && refCodeArr.Length == 2)
                {
                    response = responseArgs;
                }
                if (responseArgs.Count > 0 && isHasCustomerInfor && refCodeArr.Length == 3 && refCodeArr[2] == "IS_ON_QUEUE")
                {
                    response.Add("IS_ON_QUEUE");
                }


                if (isHasCustomerInfor && responseArgs.Count > 0)
                {
                    //Send mail
                    //if (type.equals("VIP") && cusNo.length() > 0 && customerTrackingId != 0)
                    //{
                    //    SendEmailVipComing.Send(cusNo, customerTrackingId, conn, false);
                    //}
                    if (cusNo.Length > 0)
                    {
                        //Check online booked for customer
                        argumentsCheck = isBookedOnline(cusNo, "CI");
                        //Insert to teller table
                        if (argumentsCheck.Count > 0)
                        {
                            insertToTable(cusNo, "", "", "", 0, argumentsCheck);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
                response.Add("ERROR");
                response.Add(ex.Message.ToString());
            }
            if (con.State == ConnectionState.Open)
                con.Close();

            return response;
        }

        private void insertToTable(string customerCif,
            string customerIdentifier, string customerSmartCardId,
            string bookedServiceList, int customerSelectedTable, List<string> argumentsCheck)
        {
            eCounterWebReference1.ECounterWebserviceVer5 eCounterWR = new eCounterWebReference1.ECounterWebserviceVer5();
            XmlDocument xDoc = new XmlDocument();
            for (int i = 0; i < argumentsCheck.Count; i++)
            {
                string[] s = argumentsCheck[i].ToString().Split('#');
                DataSet ds = new DataSet();
                SqlConnection con = getConnection();
                SqlCommand cmd;
                SqlDataAdapter _da;
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }

                try
                {
                    //@CUSTOMER_IDENTIFIER
                    string xmlINFO = "";
                    if (customerIdentifier.Length > 0)
                    {
                        xmlINFO = eCounterWR.LookupFCCCustomerInfoByCMND(customerIdentifier);
                    }
                    else if (customerSmartCardId.Length > 0)
                    {
                        xmlINFO = eCounterWR.LookupFCCCustomerInfoBySmartCardId(customerSmartCardId);
                    }
                    else if (customerCif.Length > 0)
                    {
                        xmlINFO = eCounterWR.LookupFCCCustomerInfo(customerCif);
                    }
                    cmd = new SqlCommand("sp_SaveBookedEBankServices", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CUSTOMER_CIF", customerCif);
                    cmd.Parameters.AddWithValue("@CUSTOMER_IDENTIFIER", customerIdentifier);
                    cmd.Parameters.AddWithValue("@CUSTOMER_SMARTCARD_ID", customerSmartCardId);
                    cmd.Parameters.AddWithValue("@CUSTOMER_BOOKED_SERVICES", bookedServiceList);
                    cmd.Parameters.AddWithValue("@CUSTOMER_NAME", "");
                    cmd.Parameters.AddWithValue("@CUSTOMER_SEX", "");
                    cmd.Parameters.AddWithValue("@CUSTOMER_TYPE", "");
                    cmd.Parameters.AddWithValue("@SELECTED_DETECT_POINT_ID", customerSelectedTable);
                    cmd.Parameters.AddWithValue("@RESERVATION_ID", s[0]);
                    cmd.Parameters.AddWithValue("@TRANS_TYPE_ID", s[1]);
                    cmd.Parameters.AddWithValue("@TRANS_TYPE", s[2]);
                    cmd.Parameters.AddWithValue("@TOTAL_MONEY_WITHDRAW", s[3]);
                    cmd.Parameters.AddWithValue("@CUSTOMER_PHONE_NUMBER", "");
                    cmd.Parameters.AddWithValue("@xmlINFO", xmlINFO);

                    _da = new SqlDataAdapter(cmd);
                    _da.Fill(ds);
                    DataTable dt = ds.Tables[0];
                    //send mail vip comming
                }
                catch (Exception ex)
                {
                    if (con.State == ConnectionState.Open)
                        con.Close();
                }
                if (con.State == ConnectionState.Open)
                    con.Close();

            }

        }

        private List<string> isBookedOnline(string customerCif, string type)
        {
            //string t = "";
            //DataSet ds = new DataSet();
            //SqlConnection con = getConnection();
            //SqlCommand cmd;
            //SqlDataAdapter _da;
            //if (con.State == ConnectionState.Closed)
            //{
            //con.Open();
            //}

            //string brn = getBRN();
            //bool checkBrn = false;
            List<string> retVal = new List<string>();
            //TUANNM5 TAM THOI BO CHECK DAT LICH RUT TIEN ONLINE DO EBANK TAM DONG TINH NANG NAY
            //try
            //{
            //    //if (type != null && checkGold.Length == 0)
            //    //{
            //        cmd = new SqlCommand("sp_CheckReservationByCif", con);
            //        cmd.CommandType = CommandType.StoredProcedure;
            //        cmd.Parameters.AddWithValue("@icif", customerCif);
            //        cmd.Parameters.AddWithValue("@itype", type);

            //    //}

            //    _da = new SqlDataAdapter(cmd);
            //    _da.Fill(ds);
            //    DataTable dt = ds.Tables[0];
            //    //if (type != null && checkGold.Length == 0)
            //    //{
            //        for (int i = 0; i < dt.Rows.Count; i++)
            //        {
            //            if (dt.Rows[i]["TRANS_TYPE"].ToString() == "1")
            //            {
            //                t = dt.Rows[i]["RESERVATION_ID"].ToString() + "#" + dt.Rows[i]["TRANS_TYPE_ID"].ToString() + "#" + dt.Rows[i]["TRANS_TYPE"].ToString() + "#" + dt.Rows[i]["TOTAL_MONEY_WITHDRAW"].ToString();
            //            }
            //            //You will compare BRN ID code here (rs.getString("BRN").equals(brn)
            //            if (t.Length> 0 && dt.Rows[i]["STATUS"].ToString() == "I")
            //            {
            //                retVal.Add(t);
            //            }
            //            //retVal.Add(dt.Rows[i]["TRANS_TYPE"].ToString() + "#" + dt.Rows[i]["TIME"].ToString() + "#" + dt.Rows[i]["RESERVATION_DATE"].ToString() + "#" + dt.Rows[i]["CONSULTING_SERVICE"].ToString() + "#" + dt.Rows[i]["BRANCH_NAME"].ToString() + "#" + dt.Rows[i]["TRANS_TYPE_ID"].ToString() + "#" + dt.Rows[i]["STATUS"].ToString() + "#" + checkBrn);
            //        }
            //    //}

            //}
            //catch (Exception ex)
            //{
            //}
            //finally
            //{
            //    con.Close();
            //}

            return retVal;
        }

        [WebMethod]
        public string CheckCustomerOnQueue(string customerNo)
        {
            //
            string xml = "";
            DataSet ds = new DataSet();
            SqlConnection con = getConnection();
            SqlCommand cmd;
            SqlDataAdapter _da;
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            try
            {
                cmd = new SqlCommand("SP_CHECK_IF_CUSTOMER_IS_ON_QUEUE", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CUSTOMER_NO", customerNo);
                _da = new SqlDataAdapter(cmd);
                _da.Fill(ds);
                DataTable dt = ds.Tables[0];

                xml = dt.Rows[0]["QUEUE_STATUS"].ToString();


            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
                xml = "ERROR " + ex.Message.ToString();
            }
            if (con.State == ConnectionState.Open)
                con.Close();

            return xml;
        }

        [WebMethod]
        public string SendUrlToGreeter(string url, string counterUsername)
        {
            string xml = "";//[sp_GetPreServingCustomers]
            DataSet ds = new DataSet();
            SqlConnection con = getConnection();
            SqlCommand cmd;
            SqlDataAdapter _da;
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            try
            {
                //Kiem tra xem Teller co dang phuc vu khach hang nao hay khong
                cmd = new SqlCommand("sp_send_greeter_url", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@URL", url);
                cmd.Parameters.AddWithValue("@COUNTER_USERNAME", counterUsername);
                cmd.ExecuteNonQuery();

                xml += "<RESP_CODE>0</RESP_CODE>";
                xml += "<RESP_CONTENT>Giao dich thanh cong!</RESP_CONTENT>";
                xml += "<ARGS>" + url + "</ARGS>";


            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
                xml += "<RESP_CODE>2000</RESP_CODE>";
                xml += "<RESP_CONTENT>" + ex.Message + "</RESP_CONTENT>";
                xml += "<ARGS>0</ARGS>";
            }
            if (con.State == ConnectionState.Open)
                con.Close();

            return xml;
        }

        [WebMethod]
        public string GetHTMLCardHelpTable()
        {
            string xml = "";//[sp_GetPreServingCustomers]
            DataSet ds = new DataSet();
            SqlConnection con = getConnection();
            SqlCommand cmd;
            SqlDataAdapter _da;
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            try
            {
                //Kiem tra xem Teller co dang phuc vu khach hang nao hay khong
                cmd = new SqlCommand("sp_GetHTMLCardHelpTable", con);
                cmd.CommandType = CommandType.StoredProcedure;
                _da = new SqlDataAdapter(cmd);
                _da.Fill(ds);
                DataTable dt = ds.Tables[0];

                if (dt.Rows.Count > 0)
                {
                    xml += "<RESP_CODE>" + dt.Rows[0]["RESP_CODE"] + "</RESP_CODE>";
                    xml += "<RESP_CONTENT>" + dt.Rows[0]["RESP_CONTENT"] + "</RESP_CONTENT>";
                    xml += "<HTML>" + dt.Rows[0]["HTML"] + "</HTML>";
                }

            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
                xml += "<RESP_CODE>2000</RESP_CODE>";
                xml += "<RESP_CONTENT>" + ex.Message + "</RESP_CONTENT>";
                xml += "<HTML></HTML>";
            }
            if (con.State == ConnectionState.Open)
                con.Close();

            return xml;
        }

        [WebMethod]
        public string AddMoreService(string customerTrackingID, string serviceCode)
        {
            string xml = "";//[sp_GetPreServingCustomers]
            DataSet ds = new DataSet();
            SqlConnection con = getConnection();
            SqlCommand cmd;
            SqlDataAdapter _da;
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            try
            {
                //Kiem tra xem Teller co dang phuc vu khach hang nao hay khong
                cmd = new SqlCommand("sp_AddMoreService", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CUSTOMER_TRACKING_ID", customerTrackingID);
                cmd.Parameters.AddWithValue("@SERVICE_CODE", serviceCode);
                _da = new SqlDataAdapter(cmd);
                _da.Fill(ds);
                DataTable dt = ds.Tables[0];

                if (dt.Rows.Count > 0)
                {
                    xml += "<RESP_CODE>" + dt.Rows[0]["RESP_CODE"] + "</RESP_CODE>";
                    xml += "<RESP_CONTENT>" + dt.Rows[0]["RESP_CONTENT"] + "</RESP_CONTENT>";
                }

            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
                xml += "<RESP_CODE>2000</RESP_CODE>";
                xml += "<RESP_CONTENT>" + ex.Message + "</RESP_CONTENT>";
            }
            if (con.State == ConnectionState.Open)
                con.Close();

            return xml;
        }

        [WebMethod]
        public string PushCustomerInstantly_ChangeCounter(string customerTrackingID, string counterUsername)
        {
            string xml = "";//[sp_GetPreServingCustomers]
            DataSet ds = new DataSet();
            SqlConnection con = getConnection();
            SqlCommand cmd;
            SqlDataAdapter _da;
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            try
            {
                //Kiem tra xem Teller co dang phuc vu khach hang nao hay khong
                cmd = new SqlCommand("sp_PushCustomerInstantly_ChangeCounter", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CUSTOMER_TRACKING_ID", customerTrackingID);
                cmd.Parameters.AddWithValue("@COUNTER_USERNAME", counterUsername);
                _da = new SqlDataAdapter(cmd);
                _da.Fill(ds);
                DataTable dt = ds.Tables[0];

                if (dt.Rows.Count > 0)
                {
                    xml += "<RESP_CODE>" + dt.Rows[0]["RESP_CODE"] + "</RESP_CODE>";
                    xml += "<RESP_CONTENT>" + dt.Rows[0]["RESP_CONTENT"] + "</RESP_CONTENT>";
                    xml += "<CUSTOMER_TRACKING_ID>" + dt.Rows[0]["CUSTOMER_TRACKING_ID"] + "</CUSTOMER_TRACKING_ID>";
                }

            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
                xml += "<RESP_CODE>2000</RESP_CODE>";
                xml += "<RESP_CONTENT>" + ex.Message + "</RESP_CONTENT>";
                xml += "<CUSTOMER_TRACKING_ID></CUSTOMER_TRACKING_ID>";
            }
            if (con.State == ConnectionState.Open)
                con.Close();

            return xml;
        }

        [WebMethod]
        public string PushCustomerInstantly(string code, string codeType, string counterUsername)
        {
            string xml = "";//[sp_GetPreServingCustomers]
            eCounterWebReference1.ECounterWebserviceVer5 eCounterWR = new eCounterWebReference1.ECounterWebserviceVer5();
            XmlDocument xmlDoc = new XmlDocument();
            string xmlCusName = eCounterWR.LookupCusNameFromCode_new(code, codeType);

            DataSet ds = new DataSet();
            SqlConnection con = getConnection();
            SqlCommand cmd;
            SqlDataAdapter _da;
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            try
            {
                //Kiem tra xem Teller co dang phuc vu khach hang nao hay khong
                cmd = new SqlCommand("sp_PushCustomerInstantly", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CODE", code);
                cmd.Parameters.AddWithValue("@CODE_TYPE", codeType);
                cmd.Parameters.AddWithValue("@COUNTER_USERNAME", counterUsername);
                cmd.Parameters.AddWithValue("@string", xmlCusName);
                _da = new SqlDataAdapter(cmd);
                _da.Fill(ds);
                DataTable dt = ds.Tables[0];

                if (dt.Rows.Count > 0)
                {
                    xml += "<RESP_CODE>" + dt.Rows[0]["RESP_CODE"] + "</RESP_CODE>";
                    xml += "<RESP_CONTENT>" + dt.Rows[0]["RESP_CONTENT"] + "</RESP_CONTENT>";
                    xml += "<CUSTOMER_TRACKING_ID>" + dt.Rows[0]["CUSTOMER_TRACKING_ID"] + "</CUSTOMER_TRACKING_ID>";
                }

            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
                xml += "<RESP_CODE>2000</RESP_CODE>";
                xml += "<RESP_CONTENT>" + ex.Message + "</RESP_CONTENT>";
                xml += "<CUSTOMER_TRACKING_ID></CUSTOMER_TRACKING_ID>";
            }
            if (con.State == ConnectionState.Open)
                con.Close();

            return xml;
        }

        [WebMethod]
        public string GetGreeterMessage(string counterUsername)
        {
            string xml = "";//[sp_GetPreServingCustomers]
            DataSet ds = new DataSet();
            SqlConnection con = getConnection();
            SqlCommand cmd;
            SqlDataAdapter _da;
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            try
            {
                //Kiem tra xem Teller co dang phuc vu khach hang nao hay khong
                cmd = new SqlCommand("sp_common_get_greeter_message", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@COUNTER_USERNAME", counterUsername);
                _da = new SqlDataAdapter(cmd);
                _da.Fill(ds);
                DataTable dt = ds.Tables[0];

                if (dt.Rows.Count > 0)
                {
                    xml += "<RESP_CODE>0</RESP_CODE>";
                    xml += "<RESP_CONTENT>Giao dich thanh cong!</RESP_CONTENT>";
                    xml += "<GREETER_MSG>" + dt.Rows[0]["GREETER_MSG"] + "</GREETER_MSG>";
                }

            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
                xml += "<RESP_CODE>2000</RESP_CODE>";
                xml += "<RESP_CONTENT>" + ex.Message + "</RESP_CONTENT>";
                xml += "<GREETER_MSG></GREETER_MSG>";
            }
            if (con.State == ConnectionState.Open)
                con.Close();

            return xml;
        }

        [WebMethod]
        public string OpenCloseQRCodeReader(string isOpen, string counterUsername)
        {
            string xml = "";//[sp_GetPreServingCustomers]
            DataSet ds = new DataSet();
            SqlConnection con = getConnection();
            SqlCommand cmd;
            SqlDataAdapter _da;
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            try
            {
                //Kiem tra xem Teller co dang phuc vu khach hang nao hay khong
                cmd = new SqlCommand("sp_open_close_QRCode_reader", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ISOPEN", isOpen);
                cmd.Parameters.AddWithValue("@COUNTER_USERNAME", counterUsername);
                cmd.ExecuteNonQuery();

                xml += "<RESP_CODE>0</RESP_CODE>";
                xml += "<RESP_CONTENT>Giao dich thanh cong!</RESP_CONTENT>";
                xml += "<ARGS>" + isOpen + "</ARGS>";


            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
                xml += "<RESP_CODE>2000</RESP_CODE>";
                xml += "<RESP_CONTENT>" + ex.Message + "</RESP_CONTENT>";
                xml += "<ARGS>0</ARGS>";
            }
            if (con.State == ConnectionState.Open)
                con.Close();

            return xml;
        }
        [WebMethod]
        public string ExportCardHelpReceipt(string custTrackingID, string bookID)
        {
            string returnUrl = WebConfigurationManager.AppSettings["returnCardHelpLink"] + "?p=" + DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            try
            {
                string printerIP = WebConfigurationManager.AppSettings["PrintReceipt"];
                PrintHelper.CallPrinter(printerIP, 10001, "cardhelp#" + custTrackingID + "#" + bookID);
            }
            catch (Exception)
            {

                throw;
            }

            return returnUrl;
        }

        [WebMethod]
        public string GetPreServingCustomers()
        {
            string xml = "";//[sp_GetPreServingCustomers]
            //List<string> result = new List<string>();
            DataSet ds = new DataSet();
            SqlConnection con = getConnection();
            SqlCommand cmd;
            SqlDataAdapter _da;
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            try
            {
                //Kiem tra xem Teller co dang phuc vu khach hang nao hay khong
                cmd = new SqlCommand("sp_GetPreServingCustomers", con);
                cmd.CommandType = CommandType.StoredProcedure;
                _da = new SqlDataAdapter(cmd);
                _da.Fill(ds);
                DataTable dt = ds.Tables[0];

                if (dt.Rows.Count == 0)
                {
                    //xml = "NGO MANH TUAN#Quầy số 1";
                    xml = "NONE";
                    //xml += "<STRING>NGO MANH TUAN#Quầy số 1</STRING><STRING>NGUYEN MANH HUY#Quầy số 3</STRING>";
                }
                else
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        xml += "<STRING>" + dt.Rows[i]["SERVING_CUSTOMERS"].ToString() + "</STRING>";
                    }

                }


            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
                xml = "ERROR: " + ex.Message;
            }
            if (con.State == ConnectionState.Open)
                con.Close();

            return xml;
        }

        [WebMethod]//custTrackID = 20762, bookID = 27
        public string GetTransDetailInfo(string custTrackID, string bookID)
        {
            //string unblockStatus = UnblockAmountProcessor(custTrackID, bookID);

            string xml = "";
            DataSet ds = new DataSet();
            SqlConnection con = getConnection();
            SqlCommand cmd;
            SqlDataAdapter _da;
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            try
            {
                //Kiem tra xem Teller co dang phuc vu khach hang nao hay khong
                cmd = new SqlCommand("sp_unblock_amount_processor", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@custTrackID", custTrackID);
                cmd.Parameters.AddWithValue("@bookID", bookID);
                _da = new SqlDataAdapter(cmd);
                _da.Fill(ds);
                DataTable dt = ds.Tables[0];

                xml = dt.Rows[0]["RESULT"].ToString();

            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
                xml = "ERROR: " + ex.Message;
            }
            if (con.State == ConnectionState.Open)
                con.Close();

            string[] dataArr = new string[] { };
            string htmlResult = "";
            try
            {
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }
                //Kiem tra xem Teller co dang phuc vu khach hang nao hay khong
                cmd = new SqlCommand("select MESSAGE_CONTENT from dbo.BOOKED_SELF_SERVICE WHERE CUSTOMER_TRACKING_ID = '" + custTrackID + "' AND ID = '" + bookID + "'", con);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                ds = new DataSet();
                da.Fill(ds);
                DataTable dt = ds.Tables[0];

                htmlResult = dt.Rows[0]["MESSAGE_CONTENT"].ToString();
                //dataArr = xml.Split('#');

            }
            catch (Exception ex)
            {
                xml = "ERROR: " + ex.Message;
            }

            //SqlConnection con = getConnection();
            xml = "";
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            try
            {
                xml = "";
                xml += "<table id=\"tb_transinfo_track20762_book27\" class=\"table table-striped table-hover\" onClick=\"RequestSv.getAutoScriptAction('GetAutoScript', '<custTrackID>" + custTrackID + "</custTrackID><bookID>" + bookID + "</bookID>');\">";
                xml += "<tbody>";
                xml += "<tr class=\"div-title\">";
                xml += "<th scope=\"row\">Thông tin giao dịch:</th>";
                xml += "<td></td>";
                xml += "</tr>";
                xml += htmlResult;
                //xml += "<tr>";
                //xml += "<td>Loại giao dịch</td>";
                //xml += "<td>Nộp tiền</td>";
                //xml += "</tr>";
                //xml += "<tr>";
                //xml += "<td>Mã giao dịch</td>";
                //xml += "<td>ABC123456789</td>";
                //xml += "</tr>";
                //xml += "<tr>";
                //xml += "<td>Số tiền giao dịch</td>";
                //xml += "<td>10,000,000 VNĐ</td>";
                //xml += "</tr>";
                //xml += "<tr>";
                //xml += "<td>Mô tả</td>";
                //xml += "<td>Chuyển tiền mặt nhận bằng Chứng minh nhân dân tới ngân hàng khác</td>";
                //xml += "</tr>";
                xml += "</tbody>";
                xml += "</table>";

            }
            catch (Exception ex)
            {
                xml = "";
            }

            if (con.State == ConnectionState.Open)
                con.Close();
            return xml;
        }

        [WebMethod]
        public string GenQRCode(string custTrackID, string bookID, string brnCode)
        {
            string xml = "DONE";
            string path = "";
            DataSet ds = new DataSet();
            SqlConnection con = getConnection();
            SqlCommand cmd;
            SqlDataAdapter da;
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }

            try
            {
                cmd = new SqlCommand("SELECT ID FROM BOOKED_SELF_SERVICE WHERE CUSTOMER_TRACKING_ID = '" + custTrackID + "'", con);
                da = new SqlDataAdapter(cmd);
                da.Fill(ds);
                DataTable dt = ds.Tables[0];

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    path = Server.MapPath(@".\QRCodes\");
                    QRCodeEncoder encoder = new QRCodeEncoder();

                    encoder.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.H; // 30%
                    encoder.QRCodeScale = 3;

                    string data = "eCounter001#" + brnCode + "#" + custTrackID + "#" + dt.Rows[i]["ID"].ToString();
                    Bitmap img = encoder.Encode(data);
                    img.SetResolution(64, 64);
                    System.Drawing.Image logo = System.Drawing.Image.FromFile(Server.MapPath(@"eCounter_logo.png"));

                    //int left = (img.Width / 2) - (logo.Width / 2);
                    //int top = (img.Height / 2) - (logo.Height / 2);


                    //Graphics g = Graphics.FromImage(img);
                    //g.DrawImage(logo, new Point(left, top));
                    img.Save(path + custTrackID + dt.Rows[i]["ID"].ToString() + ".jpg", ImageFormat.Jpeg);
                }


            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
                xml = "ERROR " + ex.Message;
            }
            if (con.State == ConnectionState.Open)
                con.Close();
            //return path + custTrackID + bookID + ".jpg";
            return xml;
        }

        [WebMethod]
        public string GenQRCode_V2()
        {
            string xml = "DONE";
            string path = "";
            DataSet ds = new DataSet();
            SqlConnection con = getConnection();
            SqlCommand cmd;
            SqlDataAdapter da;
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }

            try
            {
                cmd = new SqlCommand("sp_get_qrcode_filename", con);
                cmd.CommandType = CommandType.StoredProcedure;
                da = new SqlDataAdapter(cmd);
                da.Fill(ds);
                DataTable dt = ds.Tables[0];

                string fileName = dt.Rows[0]["QRCODE_FILENAME"].ToString() + ".jpg";

                path = Server.MapPath(@".\QRCodes\");
                QRCodeEncoder encoder = new QRCodeEncoder();

                encoder.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.H; // 30%
                encoder.QRCodeScale = 3;

                string data = "eCounter001#" + dt.Rows[0]["QRCODE_CONTENT"].ToString();
                Bitmap img = encoder.Encode(data);
                img.SetResolution(64, 64);
                System.Drawing.Image logo = System.Drawing.Image.FromFile(Server.MapPath(@"eCounter_logo.png"));

                img.Save(path + fileName, ImageFormat.Jpeg);

                xml = dt.Rows[0]["QRCODE_FILENAME"].ToString();
            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
                xml = "ERROR " + ex.Message;
            }
            if (con.State == ConnectionState.Open)
                con.Close();
            //return path + custTrackID + bookID + ".jpg";
            return xml;
        }


        //[WebMethod]
        public string UnblockAmountProcessor(string custTrackID, string bookID)
        {
            string xml = "";
            DataSet ds = new DataSet();
            SqlConnection con = getConnection();
            SqlCommand cmd;
            SqlDataAdapter da;
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            try
            {
                //Kiem tra xem Teller co dang phuc vu khach hang nao hay khong
                cmd = new SqlCommand("sp_unblock_amount_processor", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@custTrackID", custTrackID);
                cmd.Parameters.AddWithValue("@bookID", bookID);
                da = new SqlDataAdapter(cmd);
                da.Fill(ds);
                DataTable dt = ds.Tables[0];

                xml = dt.Rows[0]["RESULT"].ToString();

            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
                xml = "ERROR: " + ex.Message;
            }
            if (con.State == ConnectionState.Open)
                con.Close();
            return xml;
        }

        [WebMethod]
        public string loginUserDomain(string domain, string username, string userpass, string version, string identifier)
        {
            bool status = false;
            //string err = "";
            using (DirectoryEntry entry = new DirectoryEntry())
            {
                entry.Username = username;
                entry.Password = userpass;

                DirectorySearcher searcher = new DirectorySearcher(entry);

                searcher.Filter = "(objectclass=user)";

                try
                {
                    searcher.FindOne();
                    status = true;
                }
                catch (COMException ex)
                {
                    //err = ex.Message;
                    //if (err.Length == 0) err = "Exception";
                    if (ex.ErrorCode == -2147023570)
                    {
                        // Login or password is incorrect
                    }
                }
                entry.Close();
            }
            return status ? "Y" : "N";
            //return status ? "Y" : err;
        }

        [WebMethod]
        public List<string> GetFCCCustSignatureFromCIF(string custCIF)
        {
            List<string> xml = new List<string>();

            eCounterWebReference1.ECounterWebserviceVer5 eCounterWR = new eCounterWebReference1.ECounterWebserviceVer5();
            XmlDocument xDoc = new XmlDocument();
            try
            {
                xDoc.LoadXml("<root>" + eCounterWR.GetFCCCustomerSignature(custCIF) + "</root>");
                XmlNodeList nodelist = xDoc.GetElementsByTagName("IMAGE_FILE");
                if (nodelist.Count > 0)
                {
                    for (int i = 0; i < nodelist.Count; i++)
                    {
                        xml.Add(nodelist[i].InnerText);
                    }
                }
            }
            catch (Exception ex)
            {
                //xml = "ERROR: " + ex.Message;
            }

            return xml;
        }

        [WebMethod]
        public List<string> GetFCCCustSignature(string custTrackID)
        {
            eCounterWebReference1.ECounterWebserviceVer5 eCounterWR = new eCounterWebReference1.ECounterWebserviceVer5();
            XmlDocument xDoc = new XmlDocument();

            List<string> xml = new List<string>();
            try
            {
                xDoc.LoadXml("<root>" + eCounterWR.GetFCCCustomerSignature(custTrackID) + "</root>");

                XmlNodeList nodelist = xDoc.GetElementsByTagName("IMAGE_FILE");
                if (nodelist.Count > 0)
                {
                    for (int i = 0; i < nodelist.Count; i++)
                    {
                        xml.Add(nodelist[i].InnerText);
                    }
                }
            }
            catch (Exception ex)
            {
                xml.Add("ERROR: " + ex.Message);
            }
            return xml;
        }

        [WebMethod]
        public string PrintReceiptWS(string custTrackID, string custName, string bookInfo)
        {
            string xml = "DONE";
            //string customerName = "";
            //string bookedTableMessage = "";
            DataSet ds = new DataSet();
            SqlConnection con = getConnection();
            SqlCommand cmd;
            SqlDataAdapter da;
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }

            try
            {
                //Kiem tra xem Teller co dang phuc vu khach hang nao hay khong
                cmd = new SqlCommand("sp_GetReceiptPrintDate", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CUST_TRACK_ID", custTrackID);
                da = new SqlDataAdapter(cmd);
                da.Fill(ds);
                DataTable dt = ds.Tables[0];
                string data = "";

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    data = "";
                    string bookID = dt.Rows[i]["ID"].ToString();
                    string[] content = dt.Rows[i]["MESSAGE_CONTENT"].ToString().Split('#');
                    string[] title = dt.Rows[i]["TRANS_DESCRIPTION"].ToString().Split('#');
                    string brnCode = dt.Rows[i]["BRN_CODE"].ToString();

                    for (int j = 0; j < title.Length; j++)
                    {
                        if (title[j].Length > 0)
                        {
                            data = data + title[j] + ":" + content[j] + "#";
                        }
                    }
                    data = data + bookInfo + "#" + custName;

                    PrintReceipt(custTrackID, bookID, data, brnCode);
                }
            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
                xml = "ERROR: " + ex.Message;
            }
            if (con.State == ConnectionState.Open)
                con.Close();

            return xml;
        }

        [WebMethod]
        public void PrintReceiptSocket(string custTrackID)
        {
            try
            {
                string printerIP = WebConfigurationManager.AppSettings["PrintReceipt"];
                PrintHelper.CallPrinter(printerIP, 10001, custTrackID);
            }
            catch (Exception)
            {

                throw;
            }

        }

        public void PrintReceipt(string custTrackID, string bookID, string data, string branchCode)
        {
            //string xml = "DONE";
            string customerName = "";
            string bookedTableMessage = "";
            try
            {

                string[] bookedTransArr = data.Split('#');
                customerName = bookedTransArr[bookedTransArr.Length - 1];
                bookedTableMessage = bookedTransArr[bookedTransArr.Length - 2];

                List<string> messContentArr = new List<string>();
                messContentArr.Add(customerName);
                for (int i = 0; i < bookedTransArr.Length - 2; i++)
                {
                    string[] itemArr = bookedTransArr[i].ToString().Split(':');
                    messContentArr.Add(itemArr[0].ToString());
                    messContentArr.Add(itemArr[1].ToString());
                }
                messContentArr.Add(bookedTableMessage);

                List<string> coordinateArr = new List<string>();
                coordinateArr.Add("130#420");
                for (int i = 0; i < bookedTransArr.Length - 2; i++)
                {
                    int yCoordinate = 400 - (20 * i);
                    coordinateArr.Add("50#" + yCoordinate.ToString());
                    coordinateArr.Add("150#" + yCoordinate.ToString());
                }
                coordinateArr.Add("80#220");

                //generate pdf receipt file
                InsertTextToPdfWithImage(@".\Receipt_Template\receipt.pdf", @".\Receipts\" + custTrackID + bookID + ".pdf", messContentArr, coordinateArr, GenQRCode(custTrackID, bookID, branchCode));
                //InsertImageToPdf(Server.MapPath(@".\Receipt_Template\receipt.pdf"), GenQRCode(custTrackID, bookID, data), Server.MapPath(@".\Receipts\" + custTrackID + bookID + ".pdf"));

                //change content of cmd file
                CreateCmdFile(custTrackID + bookID + ".pdf");
                //print file
                CallPrinter();


            }
            catch (Exception ex)
            {
                //xml = "ERROR " + ex.Message;
            }

            //return xml;
        }

        private void CreateCmdFile(string fileName)
        {
            string link = "";
            string receiptFilePath = "";
            string linkWS = "";
            string foxit = "";
            string printerName = "";
            XmlDocument doc = new XmlDocument();
            doc.Load(Server.MapPath(@"\PDFCoordinates.xml"));
            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                if (node.Name == "PRINTCOMMANDPATH")
                {
                    link = node.InnerText;
                }
                if (node.Name == "RECEIPTFILEPATH")
                {
                    receiptFilePath = node.InnerText;
                }
                if (node.Name == "PHYSICALLINKWS")
                {
                    linkWS = node.InnerText;
                }
                if (node.Name == "FOXIT")
                {
                    foxit = node.InnerText;
                }
                if (node.Name == "PRINTERNAME")
                {
                    printerName = node.InnerText;
                }
            }

            //check if file exists.
            if (File.Exists(link))
                File.Delete(link);
            //create new file.
            var fi = new FileInfo(link);
            var fileStream = fi.Create();
            fileStream.Close();
            //write commands to file.
            string newContent = receiptFilePath + fileName;
            using (TextWriter writer = new StreamWriter(link))
            {
                //writer.WriteLine(String.Format("cd / \r\n\"" + foxit + "\" /t \"" + newContent + "\" \"" + printerName + "\" \r\npause"));// /quiet
                writer.WriteLine(String.Format("cd / \r\nprint /" + linkWS + " \"" + newContent + "\" \r\npause"));
            }
        }

        public string CallPrinter()
        {
            string xml = "DONE";
            try
            {
                string link = "";
                XmlDocument doc = new XmlDocument();
                doc.Load(Server.MapPath(@"\PDFCoordinates.xml"));
                foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                {
                    if (node.Name == "PRINTCOMMANDPATH")
                    {
                        link = node.InnerText;
                    }
                }

                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = link;
                info.CreateNoWindow = true;
                info.WindowStyle = ProcessWindowStyle.Hidden;

                Process p = new Process();
                p.StartInfo = info;
                p.Start();

                //p.WaitForInputIdle();
                System.Threading.Thread.Sleep(3000);
                if (false == p.CloseMainWindow())
                    p.Kill();
                xml += " --> " + link;
            }
            catch (Exception ex)
            {
                xml = ex.Message;
            }
            return xml;
        }

        [WebMethod]
        public string PrintPDFFile(string counterUsername, string custTrackingID, string bookID)
        {
            string result = "";
            string[] ContentArr;
            string serviceCode = "";
            //string custTrackingID = "";
            //string bookID = "";
            string inputFilePath = "";
            string outputFilePath = "";
            List<string> coordinateArr = new List<string>();
            List<string> messContentArr = new List<string>();
            string link = "";

            DataSet ds = new DataSet();
            SqlConnection con = getConnection();
            SqlCommand cmd;
            SqlDataAdapter da;
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            try
            {
                //Kiem tra xem Teller co dang phuc vu khach hang nao hay khong
                cmd = new SqlCommand("sp_get_final_content", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@COUNTER_USER", counterUsername);
                cmd.Parameters.AddWithValue("@CUST_TRACK_ID", custTrackingID);
                cmd.Parameters.AddWithValue("@BOOK_ID", bookID);
                da = new SqlDataAdapter(cmd);
                da.Fill(ds);
                DataTable dt = ds.Tables[0];
                ContentArr = dt.Rows[0]["PRINT_DATA"].ToString().Split('#');
                for (int i = 0; i < ContentArr.Length; i++)
                {
                    messContentArr.Add(ContentArr[i].ToString());
                }
                string[] tempArr = dt.Rows[0]["REVIEW_CONTENT"].ToString().Split('#');
                serviceCode = tempArr[tempArr.Length - 3];
                //custTrackingID = ContentArr[ContentArr.Length - 2];
                //bookID = ContentArr[ContentArr.Length - 1];

                //XmlTextReader reader = new XmlTextReader(Server.MapPath(@"\PDFCoordinates.xml"));
                //while (reader.Read()) 
                //{
                //    if ((reader.NodeType == XmlNodeType.Element) && (reader.Name == "_" + serviceCode))
                //    {
                //        if (reader.HasAttributes)
                //        {
                //            //Console.WriteLine(reader.GetAttribute("currency") + ": " + reader.GetAttribute("rate"));
                //            outputFilePath = reader.GetAttribute("Path").ToString();

                //        }
                //    }

                XmlDocument doc = new XmlDocument();
                doc.Load(Server.MapPath(@"\PDFCoordinates.xml"));
                foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                {
                    if (node.Name == "LINK")
                    {
                        link = node.InnerText;
                    }
                    if (node.Name == "_" + serviceCode)
                    {
                        inputFilePath = node.ChildNodes[0].InnerText;
                        for (int i = 1; i < node.ChildNodes.Count; i++)
                        {
                            coordinateArr.Add(node.ChildNodes[i].InnerText);
                        }
                    }
                }

                outputFilePath = GetOutputFilePath(custTrackingID, bookID);
                //string imageFile = "";
                InsertTextToPdf(inputFilePath, outputFilePath, messContentArr, coordinateArr);
                result = link + outputFilePath.Replace(@"\", "/");
                //switch (reader.NodeType) 
                //{
                //    case XmlNodeType.Element: // The node is an element.
                //        Console.Write("<" + reader.Name);
                //        Console.WriteLine(">");
                //        break;
                //    case XmlNodeType.Text: //Display the text in each element.
                //        Console.WriteLine (reader.Value);
                //        break;
                //    case XmlNodeType.EndElement: //Display the end of the element.
                //        Console.Write("</" + reader.Name);
                //        Console.WriteLine(">");
                //        break;
                //}


            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
                result = "ERROR " + ex.Message;
            }
            if (con.State == ConnectionState.Open)
                con.Close();

            return result;
        }

        private string GetOutputFilePath(string custTrackingID, string bookID)
        {
            string path = "";

            DateTime date = DateTime.Now.Date;
            string dateFolderPath = date.ToString("yyyy/MM/dd").Replace("/", "");

            if (!Directory.Exists(Server.MapPath(@"\Download\" + dateFolderPath)))
            {
                Directory.CreateDirectory(Server.MapPath(@"\Download\" + dateFolderPath));
            }

            path = @"\Download\" + dateFolderPath + @"\" + custTrackingID + bookID + ".pdf";
            return path;
        }

        private static void InsertImageToPdf(string sourceFileName, string imageFileName, string newFileName)
        {
            using (Stream pdfStream = new FileStream(sourceFileName, FileMode.Open))
            using (Stream imageStream = new FileStream(imageFileName, FileMode.Open))
            using (Stream newpdfStream = new FileStream(newFileName, FileMode.Create, FileAccess.ReadWrite))
            {
                PdfReader pdfReader = new PdfReader(pdfStream);
                PdfStamper pdfStamper = new PdfStamper(pdfReader, newpdfStream);
                PdfContentByte pdfContentByte = pdfStamper.GetOverContent(1);
                iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(imageStream);
                image.SetAbsolutePosition(0, 0);
                pdfContentByte.AddImage(image);
                pdfStamper.Close();
            }
        }

        private void InsertTextToPdf(string sourceFileName, string newFileName, List<string> contentArr, List<string> coordinates)
        {
            using (Stream pdfStream = new FileStream(Server.MapPath(sourceFileName), FileMode.Open))

            using (Stream newpdfStream = new FileStream(Server.MapPath(newFileName), FileMode.Create, FileAccess.ReadWrite))
            {
                PdfReader pdfReader = new PdfReader(pdfStream);
                PdfStamper pdfStamper = new PdfStamper(pdfReader, newpdfStream);


                PdfContentByte pdfContentByte = pdfStamper.GetOverContent(1);
                BaseFont baseFont = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1250, BaseFont.NOT_EMBEDDED);
                pdfContentByte.SetColorFill(BaseColor.BLACK);
                pdfContentByte.SetFontAndSize(baseFont, 11);
                pdfContentByte.BeginText();
                for (int i = 0; i < coordinates.Count; i++)
                {
                    int x = Convert.ToInt32(coordinates[i].Split('#')[0].ToString());
                    int y = Convert.ToInt32(coordinates[i].Split('#')[1].ToString());
                    pdfContentByte.ShowTextAligned(PdfContentByte.ALIGN_CENTER, contentArr[i], x, y, 0);
                }
                pdfContentByte.EndText();
                pdfStamper.Close();
            }
        }

        private void InsertTextToPdfWithImage(string sourceFileName, string newFileName, List<string> contentArr, List<string> coordinates, string imageFileName)
        {
            using (Stream pdfStream = new FileStream(Server.MapPath(sourceFileName), FileMode.Open))
            using (Stream imageStream = new FileStream(imageFileName, FileMode.Open))
            using (Stream newpdfStream = new FileStream(Server.MapPath(newFileName), FileMode.Create, FileAccess.ReadWrite))
            {
                PdfReader pdfReader = new PdfReader(pdfStream);
                PdfStamper pdfStamper = new PdfStamper(pdfReader, newpdfStream);


                PdfContentByte pdfContentByte = pdfStamper.GetOverContent(1);
                BaseFont baseFont = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1250, BaseFont.NOT_EMBEDDED);
                pdfContentByte.SetColorFill(BaseColor.BLACK);
                pdfContentByte.SetFontAndSize(baseFont, 12);
                pdfContentByte.BeginText();
                for (int i = 0; i < coordinates.Count; i++)
                {
                    int x = Convert.ToInt32(coordinates[i].Split('#')[0].ToString());
                    int y = Convert.ToInt32(coordinates[i].Split('#')[1].ToString());
                    pdfContentByte.ShowTextAligned(PdfContentByte.ALIGN_LEFT, contentArr[i], x, y, 0);
                }
                pdfContentByte.EndText();

                iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(imageStream);
                image.SetAbsolutePosition(250, 430);
                pdfContentByte.AddImage(image);

                pdfStamper.Close();
            }
            //InsertImageToPdf(Server.MapPath(newFileName), imageFileName, Server.MapPath(newFileName));
        }

        [WebMethod]
        public void TestInsertExcel()
        {
            //Excel.Application xlApp;
            //Excel.Workbook xlWorkBook;
            //Excel.Worksheet xlWorkSheet;
            //object misValue = System.Reflection.Missing.Value;

            //xlApp = new Excel.Application();
            //xlWorkBook = xlApp.Workbooks.Add(misValue);
            //xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);

            ////add some text 
            //xlWorkSheet.Cells[1, 1] = "http://csharp.net-informations.com";
            //xlWorkSheet.Cells[2, 1] = "Adding picture in Excel File";

            //xlWorkSheet.Shapes.AddPicture(Server.MapPath("Lighthouse.jpg"), Microsoft.Office.Core.MsoTriState.msoFalse, Microsoft.Office.Core.MsoTriState.msoCTrue, 50, 50, 300, 45);


            //xlWorkBook.SaveAs(Server.MapPath("csharp.net-informations.xls"), Excel.XlFileFormat.xlWorkbookNormal, misValue, misValue, misValue, misValue, Excel.XlSaveAsAccessMode.xlExclusive, misValue, misValue, misValue, misValue, misValue);
            //xlWorkBook.Close(true, misValue, misValue);
            //xlApp.Quit();

            //releaseObject(xlApp);
            //releaseObject(xlWorkBook);
            //releaseObject(xlWorkSheet);

        }

        private void releaseObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex)
            {
                obj = null;
                //MessageBox.Show("Unable to release the Object " + ex.ToString());
            }
            finally
            {
                GC.Collect();
            }
        }

        [WebMethod]
        public List<string> IsReceiveReviewContentProcessor(string counterUsername)
        {
            List<string> xml = new List<string>();
            DataSet ds = new DataSet();
            SqlConnection con = getConnection();
            SqlCommand cmd;
            SqlDataAdapter da;
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            try
            {
                //Kiem tra xem Teller co dang phuc vu khach hang nao hay khong
                cmd = new SqlCommand("sp_is_receive_review_content", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@COUNTER_USERNAME", counterUsername);
                da = new SqlDataAdapter(cmd);
                da.Fill(ds);
                DataTable dt = ds.Tables[0];

                xml.Add(dt.Rows[0]["IS_SENDING_REVIEW_CONTENT"].ToString());
                if (dt.Rows[0]["IS_SENDING_REVIEW_CONTENT"].ToString() == "1")
                {
                    xml.Add(dt.Rows[0]["REVIEW_CONTENT"].ToString());
                }
                else
                {
                    xml.Add("NONE");
                }

                xml.Add(dt.Rows[0]["IS_BEGIN_SERVING"].ToString());

            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
            return xml;
        }

        [WebMethod]
        public List<string> GetTellerMessageProcessor(string counterUsername, string custTrackingID)
        {
            List<string> xml = new List<string>();
            //string xml = "";
            DataSet ds = new DataSet();
            SqlConnection con = getConnection();
            SqlCommand cmd;
            SqlDataAdapter da;
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            try
            {
                //Kiem tra xem Teller co dang phuc vu khach hang nao hay khong
                cmd = new SqlCommand("sp_get_teller_messages", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@COUNTER_USERNAME", counterUsername);
                cmd.Parameters.AddWithValue("@CUST_TRACK_ID", custTrackingID);
                da = new SqlDataAdapter(cmd);
                da.Fill(ds);
                DataTable dt = ds.Tables[0];

                xml.Add(dt.Rows[0]["STATUS_MESSAGE"].ToString());
                if (Convert.ToInt32(dt.Rows[0]["DAYS_COUNT"].ToString()) > 30)
                {
                    xml.Add("1");
                }
                else
                {
                    xml.Add("0");
                }
                xml.Add(dt.Rows[0]["REVIEW_CONTENT"].ToString());
                //xml += dt.Rows[0]["STATUS_MESSAGE"].ToString();
                //if (Convert.ToInt32(dt.Rows[0]["DAYS_COUNT"].ToString()) > 30)
                //{
                //    xml += "#1#";
                //}
                //else
                //{
                //    xml += "#0#";
                //}

                //xml += dt.Rows[0]["REVIEW_CONTENT"].ToString();
            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
            return xml;
        }

        public string getContentAutoScriptEform(string eformID, string bookID, out bool check)
        {
            string xmlResult = "";
            string listServiceBooked = "000";
            //string xml = "";
            XmlDocument xDoc = new XmlDocument();
            //string CIF = "";
            string phoneNumber = "";
            //string TypeQRCode = "EF001";
            //string NameQRCode = "";
            //string ContentQRCode = "";
            string name_customer = "";
            string identifier = "";
            //string customer_type = "NORMAL";
            //for autoscript value
            string descript = "";
            check = true;
            DataSet ds = new DataSet();
            SqlConnection con = getConnection();
            SqlCommand cmd;

            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }

            try
            {
                eCounterWebReference1.ECounterWebserviceVer5 webService = new eCounterWebReference1.ECounterWebserviceVer5();
                xmlResult = webService.requestGetInfoEformFormID(eformID);
                xDoc.LoadXml("<root>" + xmlResult + "</root>");

                XmlNodeList nodelist = xDoc.GetElementsByTagName("O_ERRCODE");
                string respCode = nodelist[0].InnerText;

                if (respCode != "" && respCode.Equals("0"))
                {
                    XmlNodeList nodelistP = xDoc.DocumentElement.SelectNodes("/root/O_LIST_EFORM");

                    name_customer = nodelistP[0].ChildNodes[0].SelectSingleNode("CUSTOMER_NAME").InnerText;
                    identifier = nodelistP[0].ChildNodes[0].SelectSingleNode("CUSTOMER_IDENTIFY_VALUE").InnerText;
                    phoneNumber = nodelistP[0].ChildNodes[0].SelectSingleNode("CUSTOMER_MOBILE_NO").InnerText;

                    for (int i = 0; i < nodelistP[0].ChildNodes.Count; i++)
                    {
                        string type_form = nodelistP[0].ChildNodes[i].SelectSingleNode("FORM_TYPE").InnerText;
                        string type_payment = nodelistP[0].ChildNodes[i].SelectSingleNode("PAYMENT_TYPE").InnerText;
                        string formId = nodelistP[0].ChildNodes[i].SelectSingleNode("TRANSID").InnerText;

                        string cmnd = nodelistP[0].ChildNodes[i].SelectSingleNode("CUSTOMER_IDENTIFY_VALUE").InnerText;
                        string cif = nodelistP[0].ChildNodes[i].SelectSingleNode("CUSTOMER_CIF").InnerText;
                        string acc_no = nodelistP[0].ChildNodes[i].SelectSingleNode("CUSTOMER_ACCOUNT_NO").InnerText;
                        string cif_record = "";
                        string cif_authen = "";
                        string tktt_record = "";
                        string tktt_authen = "";
                        string document_type = "";
                        CheckCIF(cmnd, "", out cif_record, out cif_authen, out tktt_record, out tktt_authen, out document_type, out cif);

                        //Có CIF
                        if (!(cif_record == "" && cif_authen == "" && tktt_record == "" && tktt_authen == "" && document_type == ""))
                        {
                            if (!new List<string>() { "CMND", "CCCD", "HO CHIEU" }.Contains(document_type))
                            {
                                check = false;
                                return "Khách hàng đăng ký bằng số ĐKKD. Không thể thực hiện tiếp giao dịch";
                            }
                        }
                        if (tktt_record == "N" && tktt_authen == "N")
                        {
                            acc_no = "";
                        }
                        else
                        {
                            acc_no = "1";
                        }

                        if (cif_record == "" && cif_authen == "" && tktt_record == "" && tktt_authen == "" && document_type == "")
                        {
                            acc_no = "";
                        }

                        switch (type_form)
                        {
                            case "EF001"://Nop tien tai khoan
                                if (cif == "" && acc_no == "" //1
                                    || (cif != "" && cif_record == "C" && cif_authen == "A" && (acc_no == "" || (acc_no != "" && tktt_record == "N"))) //2
                                    || (cif != "" && (cif_record == "C" || cif_record == "O") && cif_authen == "U") //3
                                    || (cif != "" && cif_record == "O" && cif_authen == "A") //4,5,6
                                    )
                                {
                                    #region Nop tien tai khoan

                                    string sotiennop     = nodelistP[0].ChildNodes[i].SelectSingleNode("AMOUNT").InnerText;
                                    string tknop         = nodelistP[0].ChildNodes[i].SelectSingleNode("ACCOUNT_NO_DES").InnerText;
                                    string loaitien      = nodelistP[0].ChildNodes[i].SelectSingleNode("CCY").InnerText;
                                    string tennguoinhan  = nodelistP[0].ChildNodes[i].SelectSingleNode("NAME_ACCOUNT_DES").InnerText;
                                    string noidung       = nodelistP[0].ChildNodes[i].SelectSingleNode("TRANS_CONTENT").InnerText;
                                    string linkEditInfo  = nodelistP[0].ChildNodes[i].SelectSingleNode("LINK_EDIT_INFO_EFORM").InnerText;

                                    string customerMobileNo = nodelistP[0].ChildNodes[i].SelectSingleNode("CUSTOMER_MOBILE_NO").InnerText;
                                    string customerIdentifyValue = nodelistP[0].ChildNodes[i].SelectSingleNode("CUSTOMER_IDENTIFY_VALUE").InnerText;
                                    string customerContactAddress = nodelistP[0].ChildNodes[i].SelectSingleNode("CUSTOMER_CONTACT_ADDRESS").InnerText;
                                    string identifyDateOfIssue = nodelistP[0].ChildNodes[i].SelectSingleNode("IDENTIFY_DATE_OF_ISSUE").InnerText;
                                    string identifyPlaceOfIssue = nodelistP[0].ChildNodes[i].SelectSingleNode("IDENTIFY_PLACE_OF_ISSUE").InnerText;
                                    string customerName = nodelistP[0].ChildNodes[i].SelectSingleNode("CUSTOMER_NAME").InnerText;
                                    string currency = nodelistP[0].ChildNodes[i].SelectSingleNode("CCY").InnerText;

                                    string bankDes = nodelistP[0].ChildNodes[i].SelectSingleNode("BANK_DES").InnerText;



                                    if (bankDes != null && bankDes != "" && bankDes != "Ngân hàng Thương mại Cổ phần Tiên Phong" && bankDes != "TPBank" && bankDes != "Ngan hang Thuong mai Co phan Tien Phong")
                                    {
                                        // Nộp tiền cho khách có tài khoản ở ngân hàng khác
                                        //descript = customerName + "#" + customerMobileNo + "#" + customerContactAddress + "#" + sotiennop + "#" + noidung + "#" + currency;
                                        descript = customerName + "#" + customerMobileNo + "#" + customerContactAddress + "#" + FormatCurrency(sotiennop) + "#" + noidung + "#" + GetNameWithoutAccent(currency);
                                        listServiceBooked = "064";
                                    }
                                    else
                                    {
                                        // Tự nộp tiền hoặc nộp tiền cho khách có tài khoản ở TPBank
                                        //descript = customerName + "#" + customerIdentifyValue + "#" + customerContactAddress + "#" + dateFormatToFCC(identifyDateOfIssue)
                                        //    + "#" + identifyPlaceOfIssue + "#" + customerMobileNo + "#" + tknop + "#" + sotiennop + "#" + noidung + "#" + currency;
                                        descript = customerName + "#" + customerIdentifyValue + "#" + customerContactAddress + "#" + dateFormatToFCCddMMyyyy(identifyDateOfIssue)
                                            + "#" + identifyPlaceOfIssue + "#" + customerMobileNo + "#" + tknop + "#" + FormatCurrency(sotiennop) + "#" + noidung + "#" + GetNameWithoutAccent(currency);
                                        listServiceBooked = "056";
                                    }

                                    #endregion
                                }
                                else
                                {
                                    // CIF không đủ điều kiện
                                    check = false;
                                    return "Không thể thực hiện tiếp giao dịch này do thông tin của khách hàng đã bị thay đổi";
                                }
                                break;
                            case "EF002":
                                if (cif == "" && acc_no == "" //1
                                    || (cif != "" && cif_record == "C" && cif_authen == "A" && (acc_no == "" || (acc_no != "" && tktt_record == "N"))) //2
                                    || (cif != "" && (cif_record == "C" || cif_record == "O") && cif_authen == "U") //3
                                    || (cif != "" && cif_record == "O" && cif_authen == "A") //4,5,6
                                    )
                                {
                                    #region De nghi mua ngoai te

                                    string beneficiaryName = nodelistP[0].ChildNodes[i].SelectSingleNode("CUSTOMER_NAME").InnerText;
                                    string passport = nodelistP[0].ChildNodes[i].SelectSingleNode("CUSTOMER_IDENTIFY_VALUE").InnerText;
                                    string amountBought = nodelistP[0].ChildNodes[i].SelectSingleNode("AMOUNT").InnerText;
                                    string currencyBought = nodelistP[0].ChildNodes[i].SelectSingleNode("CCY").InnerText;
                                    string account = nodelistP[0].ChildNodes[i].SelectSingleNode("ACCOUNT_NO_DES").InnerText;
                                    string currencyPaid = "VND";

                                    string purpose = nodelistP[0].ChildNodes[i].SelectSingleNode("PURPOSE_BUY_CURRENCY").InnerText;
                                    descript = beneficiaryName + "#" + passport + "#" + amountBought + "#" + currencyBought + "#" + purpose;

                                    if (type_payment.Equals("2")) // tra qua tk
                                    {
                                        string customeraccinfo = webService.GetCustomerAccInfo(account.Substring(0, 8));
                                        string branch_code = "";
                                        XmlDocument xCus = new XmlDocument();
                                        xCus.LoadXml("<root>" + customeraccinfo + "</root>");
                                        XmlNodeList list_account = xCus.DocumentElement.SelectNodes("/root");
                                        for (int j = 0; j < list_account[0].ChildNodes.Count; j++)
                                        {
                                            if (account == list_account[0].ChildNodes[i].SelectSingleNode("ACC_NO").InnerText)
                                            {
                                                branch_code = list_account[0].ChildNodes[i].SelectSingleNode("BRANCH_CODE").InnerText;
                                            }
                                        }

                                        listServiceBooked = "057";
                                        descript = descript + "#" + account + "#" + branch_code;
                                    }
                                    else if (type_payment.Equals("1")) // tra bang tien mat
                                    {
                                        descript = descript + "#" + currencyPaid;
                                        listServiceBooked = "058";
                                    }

                                    #endregion
                                }
                                else
                                {
                                    // CIF không đủ điều kiện
                                    check = false;
                                    return "Không thể thực hiện tiếp giao dịch này do thông tin của khách hàng đã bị thay đổi";
                                }
                                break;
                            case "EF003":
                                if (cif == "" && acc_no == "" //1
                                    || (cif != "" && cif_record == "C" && cif_authen == "A" && (acc_no == "" || (acc_no != "" && tktt_record == "N"))) //2
                                    || (cif != "" && (cif_record == "C" || cif_record == "O") && cif_authen == "U") //3
                                    || (cif != "" && cif_record == "O" && cif_authen == "A") //4,5,6
                                    )
                                {
                                    #region De nghi ban ngoai te

                                    //string currencySaleType = nodelistP[0].ChildNodes[i].SelectSingleNode("CURRENCY_SALE_TYPE").InnerText;
                                    string currencyReceiveType = nodelistP[0].ChildNodes[i].SelectSingleNode("CURRENCY_RECEIVE_TYPE").InnerText;
                                    string _beneficiaryName = nodelistP[0].ChildNodes[i].SelectSingleNode("CUSTOMER_NAME").InnerText;
                                    string _passport = nodelistP[0].ChildNodes[i].SelectSingleNode("CUSTOMER_IDENTIFY_VALUE").InnerText;
                                    string amountSale = nodelistP[0].ChildNodes[i].SelectSingleNode("AMOUNT").InnerText;
                                    string currencySale = nodelistP[0].ChildNodes[i].SelectSingleNode("CCY").InnerText;
                                    string currencyReceived = "VND";

                                    // Tài khoản nhận ngoại tệ
                                    string accountReceiveCurrency = nodelistP[0].ChildNodes[i].SelectSingleNode("ACCOUNT_NO_DES").InnerText;

                                    // Tài khoản thanh toán
                                    string accountPay = nodelistP[0].ChildNodes[i].SelectSingleNode("ACCOUNT_NO_SOURCE").InnerText;


                                    /* KienNT13 - 20181204 - Ban ngoai te fix
                                    // TH1 : Tiền mặt-Thanh toan bang tien mat (1-1)
                                    if ((currencyReceiveType.Equals("1") || currencyReceiveType == "") && type_payment.Equals("1"))
                                    {
                                        descript = _beneficiaryName + "#" + _passport + "#" + amountSale + "#" + currencySale + "#" + currencyReceived;
                                        listServiceBooked = "062";
                                    }

                                    // TH1 : Tiền mặt-Thanh toan bang tai khoan (2-1)
                                    if ((currencyReceiveType.Equals("1") || currencyReceiveType == "") && type_payment.Equals("2"))
                                    {
                                        listServiceBooked = "061";
                                        descript = _beneficiaryName + "#" + _passport + "#" + amountSale + "#" + currencySale + "#" + accountPay;
                                    } */

                                    // TH1 : Chuyển khoản-Thanh toan bang tai khoan (2-2)
                                    if (currencyReceiveType.Equals("2") && type_payment.Equals("2"))
                                    {
                                        descript = accountPay.Substring(0, 8) + "#" + accountPay + "#" + accountReceiveCurrency + "#" + currencySale + "#" + amountSale;
                                        listServiceBooked = "063";
                                    }

                                    // TH2 : Chuyển khoản-Thanh toan bang tiền mặt (2-1)
                                    if (currencyReceiveType.Equals("1") && type_payment.Equals("2"))
                                    {
                                        string purpose = nodelistP[0].ChildNodes[i].SelectSingleNode("PURPOSE_BUY_CURRENCY").InnerText;
                                        descript = accountPay + "#" + amountSale + "#" + currencySale + "#" + currencyReceived + "#" + purpose;
                                        listServiceBooked = "062";
                                    }
                                    /* END KienNT13 - 20181204 - Ban ngoai te fix */

                                    /*
                                    CURRENCY_SALE_TYPE : kiểu bán ngoại tệ
                                        - Bán ngoại tệ mặt 				: 0
                                        - Bán ngoại tệ chuyển khoản		: 1

                                    PAYMENT_TYPE :	kiểu thanh toán
                                        - Thanh toán qua tiền mặt 		: 0 
                                        - Thanh toán qua chuyển khoản 	: 1
                                    */
                                    // Ban ngoai te mat
                                    /*if (currencySaleType.Equals("0"))
                                    {
                                        // TH1 : Thanh toan bang tien mat
                                        if (type_payment.Equals("0"))
                                        {
                                            // BeneficiaryName # Passport # AmountSold # CurrencySold # CurrencyReceived  
                                            descript = _beneficiaryName + "#" + _passport + "#" + amountSale + "#" + currencySale + "#" + currencyReceived;
                                        }
                                        // TH2 : Thanh toan bang tai khoan
                                        else if (type_payment.Equals("1"))
                                        {
                                            // BeneficiaryName # Passport # FXAmount # FXCurrency # Account
                                            descript = _beneficiaryName + "#" + _passport + "#" + amountSale + "#" + currencySale + "#" + accountPay;
                                        }
                                    }
                                    // Ban ngoai te chuyen khoan
                                    else if (currencySaleType.Equals("1"))
                                    {
                                        // TH3 : Thanh toan bang tai khoan
                                        if (type_payment.Equals("1"))
                                        {
                                            // Account # Account # Currency # Amount
                                            descript = accountPay + "#" + accountReceiveCurrency + "#" + currencySale + "#" + amountSale;
                                        }
                                    }*/

                                    #endregion
                                }
                                else
                                {
                                    // CIF không đủ điều kiện
                                    check = false;
                                    return "Không thể thực hiện tiếp giao dịch này do thông tin của khách hàng đã bị thay đổi";
                                }
                                break;
                            case "EF004":
                                if (cif != "" && cif_record == "O" && cif_authen == "A" && acc_no != "" && tktt_authen == "Y")
                                {
                                    #region Thay doi thong tin khach hang

                                    listServiceBooked = "059";

                                    string idName = nodelistP[0].ChildNodes[i].SelectSingleNode("CHANGE_ID_TYPE_TEXT").InnerText;
                                    string idValue = nodelistP[0].ChildNodes[i].SelectSingleNode("CHANGE_ID_VALUE").InnerText;
                                    string nationality = nodelistP[0].ChildNodes[i].SelectSingleNode("CHANGE_NATIONALITY").InnerText;
                                    string perAddress = nodelistP[0].ChildNodes[i].SelectSingleNode("CHANGE_PER_ADDRESS").InnerText;
                                    string telephoneNo = nodelistP[0].ChildNodes[i].SelectSingleNode("CHANGE_TELEPHONE_NO").InnerText;
                                    string email = nodelistP[0].ChildNodes[i].SelectSingleNode("CHANGE_EMAIL").InnerText;
                                    string fullName = nodelistP[0].ChildNodes[i].SelectSingleNode("CHANGE_NAME").InnerText;
                                    string contactAddress = nodelistP[0].ChildNodes[i].SelectSingleNode("CHANGE_CONTACT_ADDRESS").InnerText;
                                    string placeIssue = nodelistP[0].ChildNodes[i].SelectSingleNode("CHANGE_PLACE_ISSUE").InnerText;
                                    string dataIssue = nodelistP[0].ChildNodes[i].SelectSingleNode("CHANGE_DATE_ISSUE").InnerText;
                                    //string linkEditInfo = nodelistP[0].ChildNodes[i].SelectSingleNode("LINK_EDIT_INFO_EFORM").InnerText;
                                    string CIF = nodelistP[0].ChildNodes[i].SelectSingleNode("CUSTOMER_CIF").InnerText;
                                    string registerSMS = nodelistP[0].ChildNodes[i].SelectSingleNode("CHANGE_REGISTER_NOTIFY_SMS").InnerText;
                                    string[] arrListName = fullName.Split(' ');
                                    string firstName = arrListName[0];
                                    string lastName = arrListName[arrListName.Length - 1];
                                    string midName = "";

                                    if (arrListName.Length > 2)
                                    {
                                        for (int j = 1; j < arrListName.Length - 1; j++)
                                        {
                                            midName = midName + " " + arrListName[j];
                                        }
                                    }
                                    string shorname = lastName + idValue;
                                    string[] arrPerAddress = perAddress.Split(',');
                                    string valuePerAddress = "";
                                    for (int j = 0; j < 4; j++)
                                    {
                                        if (j < arrPerAddress.Length)
                                        {
                                            valuePerAddress = valuePerAddress + "#" + arrPerAddress[j];
                                        }
                                        else
                                        {
                                            valuePerAddress = valuePerAddress + "#";
                                        }
                                    }
                                    string[] arrContactAddress = contactAddress.Split(',');
                                    string valueContactAddress = "";
                                    for (int j = 0; j < 4; j++)
                                    {
                                        if (j < arrContactAddress.Length)
                                        {
                                            valueContactAddress = valueContactAddress + "#" + arrContactAddress[j];
                                        }
                                        else
                                        {
                                            valueContactAddress = valueContactAddress + "#";
                                        }
                                    }
                                    descript = fullName + "#" + shorname + "#" + firstName + "#" + midName + "#" + lastName + "#" + idName + "#" +
                                                idValue + "#" + dateFormatToFCCddMMyyyy(dataIssue) + "#" + placeIssue + "#" + nationality + valuePerAddress + valueContactAddress + "#" + telephoneNo + "#" + email +
                                                "#" + idName + "#" + CIF + "#" + telephoneNo + "#" + registerSMS;

                                    #endregion
                                }
                                else
                                {
                                    // CIF không đủ điều kiện
                                    check = false;
                                    return "Không thể thực hiện tiếp giao dịch này do thông tin của khách hàng đã bị thay đổi";
                                }
                                break;
                            case "EF005":
                                if (cif == "" && acc_no == "")
                                {
                                    #region Mo TKTT chua co cif

                                    listServiceBooked = "060";


                                    //  string tennguoinhan = nodelistP[0].ChildNodes[i].SelectSingleNode("NAME_ACCOUNT_DES").InnerText;
                                    //  string noidung = nodelistP[0].ChildNodes[i].SelectSingleNode("TRANS_CONTENT").InnerText;
                                    //  string linkEditInfo = nodelistP[0].ChildNodes[i].SelectSingleNode("LINK_EDIT_INFO_EFORM").InnerText;

                                    string customerNationality   = nodelistP[0].ChildNodes[i].SelectSingleNode("COUNTRY_CODE").InnerText;
                                    string dateOfBirth           = nodelistP[0].ChildNodes[i].SelectSingleNode("CUSTOMER_DOB").InnerText;
                                    string gender                = nodelistP[0].ChildNodes[i].SelectSingleNode("CUSTOMER_GENDER").InnerText;
                                    string identifyType          = nodelistP[0].ChildNodes[i].SelectSingleNode("CUSTOMER_IDENTIFY_TYPE_TEXT").InnerText;
                                    string identifyValue         = nodelistP[0].ChildNodes[i].SelectSingleNode("CUSTOMER_IDENTIFY_VALUE").InnerText;

                                    string dateOfIssue           = nodelistP[0].ChildNodes[i].SelectSingleNode("IDENTIFY_DATE_OF_ISSUE").InnerText;
                                    string placeOfIssue          = nodelistP[0].ChildNodes[i].SelectSingleNode("IDENTIFY_PLACE_OF_ISSUE").InnerText;
                                    string mobileNo              = nodelistP[0].ChildNodes[i].SelectSingleNode("CUSTOMER_MOBILE_NO").InnerText;
                                    string telNo                 = nodelistP[0].ChildNodes[i].SelectSingleNode("CUSTOMER_TEL_NO").InnerText;
                                    string customerEmail         = nodelistP[0].ChildNodes[i].SelectSingleNode("CUSTOMER_EMAIL").InnerText;
                                    string companytel            = nodelistP[0].ChildNodes[i].SelectSingleNode("CUSTOMER_COMPANY_TEL_NO").InnerText;

                                    string permanentAddress      = nodelistP[0].ChildNodes[i].SelectSingleNode("CUSTOMER_PERMANENT_ADDRESS").InnerText;
                                    string registerSMS1          = nodelistP[0].ChildNodes[i].SelectSingleNode("REGISTER_NOTIFY_SMS").InnerText;

                                    string[] arrPermanentAddress = permanentAddress.Split(',');
                                    string valuePermanentAddress = "";
                                    for (int j = 0; j < 4; j++)
                                    {
                                        if (j < arrPermanentAddress.Length)
                                        {
                                            valuePermanentAddress = valuePermanentAddress + "#" + arrPermanentAddress[j];
                                        }
                                        else
                                        {
                                            valuePermanentAddress = valuePermanentAddress + "#";
                                        }
                                    }


                                    string cusContactAddress = nodelistP[0].ChildNodes[i].SelectSingleNode("CUSTOMER_CONTACT_ADDRESS").InnerText;
                                    string[] arrCusContactAddress = cusContactAddress.Split(',');
                                    string valueCusContactAddress = "";
                                    for (int j = 0; j < 4; j++)
                                    {
                                        if (j < arrCusContactAddress.Length)
                                        {
                                            valueCusContactAddress = valueCusContactAddress + "#" + arrCusContactAddress[j];
                                        }
                                        else
                                        {
                                            valueCusContactAddress = valueCusContactAddress + "#";
                                        }
                                    }

                                    string residentStatus = nodelistP[0].ChildNodes[i].SelectSingleNode("CUSTOMER_RESIDENT_STATUS").InnerText;
                                    string fullNameCustomer = nodelistP[0].ChildNodes[i].SelectSingleNode("CUSTOMER_NAME").InnerText;
                                    string[] arrListNames = fullNameCustomer.Split(' ');
                                    string firstNameCustomer = arrListNames[0];
                                    string lastNameCustomer = arrListNames[arrListNames.Length - 1];
                                    string midNameCustomer = "";
                                    if (arrListNames.Length > 2)
                                    {
                                        for (int j = 1; j < arrListNames.Length - 1; j++)
                                        {
                                            midNameCustomer = midNameCustomer + " " + arrListNames[j];
                                        }
                                    }

                                    string sortName = lastNameCustomer + identifyValue;
                                    /* KienNT13 - 11/29/2018 - LinkFCC - Mở TK thanh toán */
                                    string fullNameWithoutAccent = GetNameWithoutAccent(fullNameCustomer); //Personal\Address for Correspondence-Name[BLK_CUSTOMER__NAME]
                                    string customerCategory = "CA NHAN"; //Personal-Customer Category[BLK_CUSTOMER__CCATEG] - TAB_PERSONAL
                                    string language = "VIE"; //Personal-Language[BLK_CUSTPERSONAL__LANG]
                                    string chargeGroup = "RB"; //Additional\Mics Details-Charge Group[BLK_CUSTOMER__CHGGRP] - TAB_ADDITIONAL
                                    string location = "VN"; //Additional\Mics Details-Location[BLK_CUSTOMER__LOC]
                                    string media = "EMAIL"; //Additional\Mics Details-Media[BLK_CUSTOMER__MEDIA]
                                    string trackLimits = "Y"; // Y: Tích chọn //Additional\Mics Details-Track Limits[BLK_CUSTOMER__TRACK_LIMITS]
                                    string NHOM_KH_VAY = "KH5"; //Tại link Fields[BLK_UDF_DETAILS_VIEW__FLDVAL3] - fnShowUDFScreen('CSCFNUDF','','CVS_UDF')
                                    string NGANH_NGHE = "KHCN, khong ap dung"; //Tại link Fields[BLK_UDF_DETAILS_VIEW__FLDVAL17]
                                    string QUY_MO = "KHCN KHONG AP DUNG"; //Tại link Fields[BLK_UDF_DETAILS_VIEW__FLDVAL19]
                                    string overallLimit = "0.00"; //Tại link Limits[BLK_CUST_LIAB__OVERALL_LIMITI] - fnSubScreenMain('STDCIF','STDCIF','CVS_LIM')
                                    /* END KienNT13 - 11/29/2018 - LinkFCC - Mở TK thanh toán */
                                    descript = fullNameCustomer + "#" + sortName + "#" + firstNameCustomer + "#" + midNameCustomer + "#" + lastNameCustomer + "#" + dateFormatToFCCyyyyMMdd(dateOfBirth) + "#" + gender + "#" + identifyType + "#" +
                                         identifyValue + "#" + dateFormatToFCCyyyyMMdd(dateOfIssue) + "#" + placeOfIssue + "#" + mobileNo + "#" + mobileNo + "#" + customerEmail + valuePermanentAddress + valueCusContactAddress + "#" +
                                         residentStatus + "#" + customerNationality + "#" + identifyType + "#" + registerSMS1 + "#" + companytel;
                                    /* KienNT13 - 11/29/2018 - LinkFCC - Mở TK thanh toán */
                                    descript += "#" + fullNameWithoutAccent + "#" + customerCategory + "#" + language + "#" +
                                        chargeGroup + "#" + location + "#" + media + "#" + trackLimits + "#" +
                                        NHOM_KH_VAY + "#" + NGANH_NGHE + "#" + QUY_MO + "#" + overallLimit;
                                    /* END KienNT13 - 11/29/2018 - LinkFCC - Mở TK thanh toán */

                                    #endregion
                                }
                                // Chưa có CIF hoặc CIF không đủ điều kiện
                                else if (cif != "" && (cif_record == "C" || cif_record == "O") && cif_authen == "A" && (acc_no == "" || (acc_no != "" && tktt_record == "N")))
                                {
                                    listServiceBooked = "065";
                                    descript = cif;
                                }
                                else
                                {
                                    // CIF không đủ điều kiện
                                    check = false;
                                    return "Không thể thực hiện tiếp giao dịch này do thông tin của khách hàng đã bị thay đổi";
                                }

                                break;
                            case "EF006":// gửi tiền tiết kiệm
                                if (cif == "" && acc_no == "" //1
                                    || (cif != "" && cif_record == "C" && cif_authen == "A" && (acc_no == "" || (acc_no != "" && tktt_record == "N"))) //2
                                    || (cif != "" && (cif_record == "C" || cif_record == "O") && cif_authen == "U") //3
                                    || (cif != "" && cif_record == "O" && cif_authen == "A") //4,5,6
                                    )
                                {
                                    #region gui tiet kiem

                                    listServiceBooked = "066";

                                    //-Customer Id
                                    string cmndIdCustomer = nodelistP[0].ChildNodes[i].SelectSingleNode("CUSTOMER_IDENTIFY_VALUE").InnerText;//(*abcd)

                                    //-Loại tiền - Currency
                                    string loaitien = nodelistP[0].ChildNodes[i].SelectSingleNode("CCY").InnerText;//(*abcd)

                                    //-Account class - Loại hình tiết kiệm
                                    string accountClass = nodelistP[0].ChildNodes[i].SelectSingleNode("ACCOUNT_CLASS").InnerText;// CẦN XÁC MINH T3 07/04/2020 4:42CH (*abcd)

                                    //-Customer Name
                                    string beneficiaryName = nodelistP[0].ChildNodes[i].SelectSingleNode("CUSTOMER_NAME").InnerText;//(*abcd)

                                    //-Branch code
                                    //-Account number                -(FCC tự động sinh)
                                    //-Account open date             -(FCC tự động sinh)
                                    //-External Reference Number     -(FCC tự động sinh)
                                    //-Account Description           -(FCC tự động sinh)
                                    //====================================================================================================================================
                                    //-Pay in Details
                                    // --Term Deposit Pay In Option
                                    // --Percentage
                                    // --Amount = Trường Số tiền tại mục Thông tin tiền gửi của lựa chọn hình thức trích nợ tương ứng 
                                    string sotien = nodelist[0].ChildNodes[i].SelectSingleNode("AMOUNT").InnerText;//(*abcd)
                                    // --Offset branch = Số tài khoản (2 TH)
                                    string branch_code_offset = "";
                                    if (type_payment.Equals("2")) //trích nợ Từ tài khoản thanh toán tại TPBank
                                    {
                                        branch_code_offset = nodelistP[0].ChildNodes[i].SelectSingleNode("ACCOUNT_NO_DES").InnerText;
                                        listServiceBooked = "067";
                                    }
                                    else if (type_payment.Equals("1")) // trích nợ bằng tiền mặt
                                    {
                                        branch_code_offset = "101100000";//Số tài khoản
                                        listServiceBooked = "068";
                                    }
                                    // --Offet account = Trường Số tài khoản tại mục Thông tin tiền gửi của lựa chọn hình thức trích nợ tương ứng  = Offset branch
                                    string offsetacount = branch_code_offset;
                                    //====================================================================================================================================

                                    //-Term deposit amount = Trường Số tiền tại mục Thông tin tiền gửi
                                    string terndepositamount = nodelistP[0].ChildNodes[i].SelectSingleNode("SO_TIEN_TIET_KIEM").InnerText;
                                    //-Deposit tenor = Trường Kỳ hạn tại mục Thông tin tiền gửi
                                    string deposittenor = nodelistP[0].ChildNodes[i].SelectSingleNode("KY_HAN").InnerText;
                                    //-Auto rollover = FCC mặc định tích Auto rollover
                                    //-Account tenor = FCC mặc định tích Account tenor
                                    //====================================================================================================================================
                                    string taituc = nodelistP[0].ChildNodes[i].SelectSingleNode("TAI_TUC").InnerText;
                                    //-Rollover type
                                    string principal="";
                                    string interest="";
                                    // --+ Principal + Interest = '- FCC tích chọn 'Principal + Interest' nếu checkbox 'Chuyển cả gốc và lãi sang kỳ hạn mới' tại mục Thông tin tiền gửi\Tái tục trên eForm được tích chọn                                   
                                    if (taituc.Equals("Chuyển cả gốc và lãi sang kỳ hạn mới"))
                                    {
                                        principal = "1";
                                        interest = "1";
                                    }
                                    // --+ Principal = '- FCC tích chọn 'Principal' nếu checkbox 'Nhận lãi, chuyển gốc sang kỳ hạn mới' tại mục Thông tin tiền gửi\Tái tục trên eForm được tích chọn                                    
                                    if (taituc.Equals("Nhận lãi, chuyển gốc sang kỳ hạn mới"))
                                    {
                                        principal = "1";
                                        interest = "2";
                                    }
                                    //====================================================================================================================================
                                    //-Payout Details
                                    string payouttype = "";
                                    // --Payout type = FCC tích chọn ' Account' nếu trường Tái tục = 'Nhận lãi, chuyển gốc sang kỳ hạn mới' tại mục Thông tin tiền gửi                                    
                                    if(taituc.Equals("Nhận lãi, chuyển gốc sang kỳ hạn mới"))
                                    {
                                        payouttype = "Account";
                                    }
                                    // --Percentage = Mặc định = 100%
                                    string percentage = "100%";
                                    // --Account = FCC hiển thị theo trường 'Số tài khoản' của lựa chọn Trả lãi trên Teller Admin
                                    string tralaiAccount = nodelistP[0].ChildNodes[i].SelectSingleNode("ACCOUNT_TRALAI").InnerText;
                                    // --Offset branch = FCC hiển thị theo Account 
                                    string offsetbranchaccount = nodelistP[0].ChildNodes[i].SelectSingleNode("ACCOUNT_TRALAI").InnerText;
                                    // --Payout component = 'mặc định hiển thị = 'Interest'
                                    string payoutcomponent = "Interest";
                                    // --Account title = Hệ thống hiển thị theo TKTT của KH (NV check lai)

                                    descript = cmndIdCustomer + "#" + loaitien + "#" + accountClass + "#" + beneficiaryName + "#" + sotien + "#" + branch_code_offset + "#" + offsetacount
                                        + "#" + terndepositamount + "#" + deposittenor + "#" + principal + "#" + interest + "#" + payouttype + "#" + percentage + "#" + tralaiAccount + "#" + offsetbranchaccount + "#" + payoutcomponent;

                                    #endregion
                                }
                                // Chưa có CIF hoặc CIF không đủ điều kiện
                                else if (cif != "" && (cif_record == "C" || cif_record == "O") && cif_authen == "A" && (acc_no == "" || (acc_no != "" && tktt_record == "N")))
                                {
                                    listServiceBooked = "066";
                                    descript = cif;
                                }
                                else
                                {
                                    // CIF không đủ điều kiện
                                    check = false;
                                    return "Không thể thực hiện tiếp giao dịch này do thông tin của khách hàng đã bị thay đổi";
                                }

                                
                                break;
                        }
                    }
                    cmd = new SqlCommand("sp_update_list_service_book", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@SERVICE_BOOK", SqlDbType.NVarChar).Value = listServiceBooked;
                    cmd.Parameters.Add("@BOOK_ID", SqlDbType.NVarChar).Value = bookID;
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                }
                else
                {
                    descript = "";
                }
            }
            catch (Exception ex)
            {
                descript += "";
            }

            return descript;
        }

        private void CheckCIF(string identity, string type, out string cif_record, out string cif_authen, out string tktt_record, out string tktt_authen, out string document_type, out string cif_no)
        {
            cif_record = "";
            cif_authen = "";
            tktt_record = "";
            tktt_authen = "";
            document_type = "";
            cif_no = "";
            try
            {
                eCounterWebReference1.ECounterWebserviceVer5 webService = new eCounterWebReference1.ECounterWebserviceVer5();
                string chkCif = webService.checkCIFIC(identity, type);
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml("<root>" + chkCif + "</root>");
                XmlNodeList xmlNode = xmlDoc.DocumentElement.SelectNodes("/root");
                cif_record = xmlNode[0].SelectSingleNode("RecordStat").InnerText;
                cif_authen = xmlNode[0].SelectSingleNode("AuthStat").InnerText;
                tktt_record = xmlNode[0].SelectSingleNode("StatusAccCif").InnerText;
                tktt_authen = xmlNode[0].SelectSingleNode("StatusAccUath").InnerText;
                document_type = xmlNode[0].SelectSingleNode("IDType").InnerText;
                cif_no = xmlNode[0].SelectSingleNode("CIF_NO").InnerText;
            }
            catch (Exception) { }
        }

        /* KienNT13 - 11/29/2018 - LinkFCC - Mở TK thanh toán */
        private string GetNameWithoutAccent(string input)
        {
            string output = "";
            for (int i = 0; i < input.Length; i++)
            {
                string up = ("" + input[i]).ToUpper();
                if ("ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ".Contains(up))
                    output += "A";
                else if ("ÉÈẸẺẼÊẾỀỆỂỄ".Contains(up))
                    output += "E";
                else if ("ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ".Contains(up))
                    output += "O";
                else if ("ÚÙỤỦŨƯỨỪỰỬỮ".Contains(up))
                    output += "U";
                else if ("ÍÌỊỈĨ".Contains(up))
                    output += "I";
                else if ("Đ".Contains(up))
                    output += "D";
                else if ("ÝỲỴỶỸ".Contains(up))
                    output += "Y";
                else
                    output += up;
            }
            return output;
        }

        private string FormatCurrency(string input, string type = "VND")
        {
            if (type == "VND")
            {
                try
                {
                    //string.Format(new System.Globalization.CultureInfo("en-US"), "{0:N2}", input);
                    return (Convert.ToDouble(input)).ToString("N2");
                }
                catch (Exception)
                {

                }
            }
            return input;
        }
        /* END KienNT13 - 11/29/2018 - LinkFCC - Mở TK thanh toán */

        //caont edit 06/08/2018
        [WebMethod]//custTrackID = 20762, bookID = 27
        public string GetAutoScript(string custTrackID, string bookID)
        {
            //string unblockStatus = UnblockAmountProcessor(custTrackID, bookID);

            string xml = "";
            string eformID = "";
            string valueContent = "";
            DataSet ds = new DataSet();
            SqlConnection con = getConnection();
            SqlCommand cmd;
            SqlDataAdapter _da;


            xml = "";
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            try
            {
                //nếu là giao dịch eform lấy ra eformID để update lại content autoscript
                cmd = new SqlCommand("sp_get_eform_transid", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@BOOK_ID", SqlDbType.NVarChar).Value = bookID;
                SqlDataAdapter daEform = new SqlDataAdapter(cmd);
                ds = new DataSet();
                daEform.Fill(ds);
                if (ds.Tables.Count > 0)
                {
                    DataTable dt1 = ds.Tables[0];
                    eformID = dt1.Rows[0]["EFORM_ID"].ToString();

                }
                bool check = true;
                //neu la giao dich eform --> goi len db eform lai lai thong tin giao dich
                if (eformID.Length > 0 && !eformID.Equals(""))
                {
                    valueContent = getContentAutoScriptEform(eformID, bookID, out check);
                }

                if (check)
                {
                    cmd = new SqlCommand("sp_get_auto_script_for_booked_trans", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@CUST_TRACK_ID", SqlDbType.NVarChar).Value = custTrackID;
                    cmd.Parameters.Add("@BOOK_ID", SqlDbType.NVarChar).Value = bookID;
                    cmd.Parameters.Add("@BOOK_DESC", SqlDbType.NVarChar).Value = valueContent;
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);


                    string content = "";
                    xml += "[";
                    foreach (DataRow row in dt.Rows)
                    {
                        if (row["REPLACE_KEY"].ToString() != null && row["REPLACE_KEY"].ToString() != "")
                        {
                            string[] dataTempArr = row["REPLACE_KEY"].ToString().Split('#');
                            string[] dataValArr = row["MESSAGE_CONTENT"].ToString().Split('#');
                            content = row["TEMP_SCRIPT"].ToString();

                            for (int i = 0; i < dataTempArr.Length; i++)
                            {
                                content = content.Replace(dataTempArr[i], dataValArr[i]);
                            }
                        }
                        else
                        {
                            content = row["TEMP_SCRIPT"].ToString();
                        }

                        xml += "{\"script\": \"" + content + "\",";
                        xml += "\"delayStart\": \"" + row["DELAYSTART"].ToString() + "\",";
                        xml += "\"delayEnd\": \"" + row["DELAYEND"].ToString() + "\",";
                        xml += "\"logfile\": \"" + row["LOGFILE"].ToString() + "\",";
                        xml += "\"stepNo\": \"" + row["STEP_NO"].ToString() + "\",";
                        xml += "\"scriptID\": \"" + row["AUTOSCRIPT_ID"].ToString() + "\",";
                        xml += "\"description\": \"" + row["DESCRIPTION"].ToString() + "\"}";

                        xml += ",";
                    }
                    xml += "]";
                    xml = xml.Replace("},]", "}]");
                }
                else
                {
                    xml = "[{\"code\": \"001\", \"content\": \"" + valueContent + "\"}]";
                }
            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
                xml += "]";
                xml = xml.Replace("},]", "}]");
            }
            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
            return xml;
        }

        [WebMethod]
        public void SaveCustSignatureImageUnauthorized(string custTrackID, string bookID, string imgData1, string imgData2, string imgData3, string xRef)
        {
            //string[] args = xRef.Split('#');
            SqlConnection con = getConnection();
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            try
            {
                string[] args = xRef.Split('#');

                SqlCommand cmd = new SqlCommand("sp_insert_data_signature_unauthorized", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@custTrackID", SqlDbType.NVarChar).Value = custTrackID;
                cmd.Parameters.Add("@bookID", SqlDbType.NVarChar).Value = bookID;
                cmd.Parameters.Add("@imgData1", SqlDbType.NVarChar).Value = imgData1;
                cmd.Parameters.Add("@imgData2", SqlDbType.NVarChar).Value = imgData2;
                cmd.Parameters.Add("@imgData3", SqlDbType.NVarChar).Value = imgData3;
                cmd.Parameters.Add("@xRef", SqlDbType.NVarChar).Value = args[0].ToString();
                cmd.Parameters.Add("@feeCharges", SqlDbType.NVarChar).Value = args[1].ToString() + "#" + args[2].ToString();

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
        }

        [WebMethod]
        public void SaveCustSignatureImage(string custTrackID, string bookID, string imgData1, string imgData2, string imgData3, string xRef)
        {
            string brnID = "";
            brnID = getBRN();
            SqlConnection con = getConnection();
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            try
            {
                eCounterWebReference1.ECounterWebserviceVer5 eCounterWR = new eCounterWebReference1.ECounterWebserviceVer5();
                eCounterWR.InsertCustomerSignature(imgData1, imgData2, imgData3, custTrackID, bookID, xRef, brnID);

                SqlCommand cmd = new SqlCommand("sp_insert_data_signature", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@custTrackID", SqlDbType.NVarChar).Value = custTrackID;
                cmd.Parameters.Add("@bookID", SqlDbType.NVarChar).Value = bookID;
                cmd.Parameters.Add("@imgData1", SqlDbType.NVarChar).Value = imgData1;
                cmd.Parameters.Add("@imgData2", SqlDbType.NVarChar).Value = imgData2;
                cmd.Parameters.Add("@imgData3", SqlDbType.NVarChar).Value = imgData3;
                cmd.Parameters.Add("@xRef", SqlDbType.NVarChar).Value = xRef;

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
        }

        [WebMethod]
        public string GetUrlCustSignatureImage(string custTrackingID, string custBookID)
        {
            return "CustSignatureImage.aspx?custTrackingID=" + custTrackingID + "&custBookID=" + custBookID;
        }

        [WebMethod]
        public string GetCustSignatureImage(string custTrackingID, string custBookID)
        {
            string xml = "";
            //return xml;
            DataSet dsCheck = new DataSet();
            DataTable dt = new DataTable();
            SqlConnection con = getConnection();
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            try
            {
                SqlCommand cmd = new SqlCommand("sp_get_signature_image", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@cust_tracking_id", SqlDbType.NVarChar).Value = custTrackingID;
                cmd.Parameters.Add("@book_id", SqlDbType.NVarChar).Value = custBookID;
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dsCheck);
                dt = dsCheck.Tables[0];
                xml = dt.Rows[0]["IMG_DATA"].ToString();
            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }

            return xml;
        }

        [WebMethod]
        public DataSet CheckIfTellerHasCurrentServing(string counterUsername, string myIP)
        {
            DataSet dsCheck = new DataSet();
            SqlConnection con = getConnection();
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            try
            {
                SqlCommand cmd = new SqlCommand("sp_CheckIfTellerHasCurrentServing", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@COUNTER_USERNAME", counterUsername);
                cmd.Parameters.AddWithValue("@COUNTER_IP", myIP);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dsCheck);

            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }

            return dsCheck;
        }
        [WebMethod]
        public string requestGetTellerName()
        {
            string xmlResult = "";

            string ip = HttpContext.Current.Request.UserHostName;
            //hard code
            ip = "10.2.230.230";

            Utils.Log.WriteLog("requestGetTellerName : ip client = " + ip);

            DataSet dsCheck = new DataSet();
            SqlConnection con = getConnection();
            SqlCommand cmd;
            SqlDataAdapter da;
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            try
            {
                cmd = new SqlCommand("sp_getTellerName", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IP_CLIENT_TELLER", ip);
                da = new SqlDataAdapter(cmd);
                da.Fill(dsCheck);


                if (dsCheck != null && dsCheck.Tables[0].Rows[0][0].ToString() != "2000")
                {

                    Utils.Log.WriteLog("requestGetTellerName : name teller :  " + dsCheck.Tables[0].Rows[0][0].ToString());
                    xmlResult += "<RESP_CODE>0</RESP_CODE>";
                    xmlResult += "<RESP_CONTENT>Giao dich thanh cong</RESP_CONTENT>";
                    xmlResult += "<ARGS>";
                    xmlResult += "<STRING>" + dsCheck.Tables[0].Rows[0][0].ToString() + "</STRING>";
                    xmlResult += "</ARGS>";

                }
                else
                {
                    xmlResult += "<RESP_CODE>2000</RESP_CODE>";
                    xmlResult += "<RESP_CONTENT>Không tồn tại user</RESP_CONTENT>";
                    xmlResult += "<ARGS>";
                    xmlResult += "<STRING>Không tồn tại user</STRING>";
                    xmlResult += "</ARGS>";
                }
            }
            catch (Exception ex)
            {
                Utils.Log.WriteLog("requestGetTellerName ex :  = " + ex.ToString());
                xmlResult = "<RESP_CODE>2000</RESP_CODE>";
                xmlResult += "<RESP_CONTENT>" + ex.Message.ToString() + "</RESP_CONTENT>";
                xmlResult += "<ARGS><STRING></STRING></ARGS>";
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }

            }

            return xmlResult;

        }


        [WebMethod]
        public String GetCommingCustomer(string counterUsername)
        {
            string xml = "";
            DataSet dsCheck = new DataSet();
            DataSet dsCheck1 = new DataSet();
            SqlConnection con = getConnection();
            SqlCommand cmd;
            SqlDataAdapter da;
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            try
            {
                //Kiem tra xem Teller co dang phuc vu khach hang nao hay khong
                Utils.Log.WriteLog("GetCommingCustomer : " + counterUsername);
                cmd = new SqlCommand("sp_CheckIfTellerHasCurrentServing", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@COUNTER_USERNAME", counterUsername);
                da = new SqlDataAdapter(cmd);
                da.Fill(dsCheck);



                if (dsCheck != null && dsCheck.Tables[0].Rows[0][0].ToString() != "NO_DATA")
                {
                    string test = dsCheck.Tables[0].Rows[0][0].ToString();
                    Utils.Log.WriteLog("GetCommingCustomer : " + dsCheck.Tables[0].Rows[0][0].ToString());
                    //Dang phuc vu khach hang: lay thong tin khach hang hien thoi de return cho client
                    xml = MergeDataFromTableToHTML(dsCheck);
                }
                else
                {
                    Utils.Log.WriteLog("GetCommingCustomer : khong co thong tin");
                    //Dang khong phuc vu khach hang nao: query lien tuc lay thong tin khach hang moi chuan bi vao quay GD
                    cmd = new SqlCommand("sp_GetComingCustomer", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@COUNTER_USERNAME", counterUsername);
                    da = new SqlDataAdapter(cmd);
                    da.Fill(dsCheck1);

                    xml = MergeDataFromTableToHTML(dsCheck1);
                }
            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
                //xml = "Không có khách hàng mới";
                xml += "<div id='idComingCustTitle' class='div-title'>Thông tin khách hàng sắp đến quầy GD</div>";
                xml += "<table class='table table-striped'>";
                xml += "<tbody>";
                xml += "<tr>";
                xml += "<th scope='row'>Tên</th>";
                xml += "<td>-</td>";
                xml += "</tr>";
                xml += "<tr>";
                xml += "<th scope='row'>Giới tính</th>";
                xml += "<td>-</td>";
                xml += "</tr>";
                xml += "<tr>";
                xml += "<th scope='row'>Loại</th>";
                xml += "<td>-</td>";
                xml += "</tr>";
                xml += "<tr>";
                xml += "<th scope='row'>CIF</th>";
                xml += "<td>-</td>";
                xml += "</tr>";
                xml += "<tr>";
                xml += "<th scope='row'>CMND/HC</th>";
                xml += "<td>-</td>";
                xml += "</tr>";
                //caont start
                xml += "<tr>";
                xml += "<th scope='row'>Thời gian chờ </th>";
                xml += "<td>-</td>";
                xml += "</tr>";
                //caont end
                xml += "</tbody>";
                xml += "</table>";
            }
            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
            return xml;
        }

        private string MergeDataFromTableToHTML(DataSet ds)
        {
            string xml = "";
            DataTable dtCommingCustInfo = ds.Tables[0];
            DataTable dtBookedServices = ds.Tables[1];
            DataTable dtBookedSelfServices = ds.Tables[2];
            DataTable dtBookedEBankServices = ds.Tables[3];
            //DataTable dtBrnNo = ds.Tables[4];
            string sex = dtCommingCustInfo.Rows[0]["SEX"].ToString() == "M" ? "Nam" : "Nữ";
            int count = 1;

            xml += "<div id='idComingCustTitle' class='div-title'>Thông tin khách hàng sắp đến quầy GD</div>";
            xml += "<table class='table table-striped'>";
            xml += "<tbody>";
            xml += "<tr>";
            xml += "<th scope='row'>Tên</th>";
            xml += "<td>" + dtCommingCustInfo.Rows[0]["CUSTOMER_NAME"].ToString() + " - STT " + dtCommingCustInfo.Rows[0]["STT_USER"].ToString() + "</td>";
            xml += "</tr>";
            xml += "<tr>";
            xml += "<th scope='row'>Giới tính</th>";
            xml += "<td>" + sex + "</td>";
            xml += "</tr>";
            xml += "<tr>";
            xml += "<th scope='row'>Loại</th>";
            xml += "<td>" + dtCommingCustInfo.Rows[0]["TYPE_NAME"].ToString() + "</td>";
            xml += "</tr>";
            xml += "<tr>";
            xml += "<th scope='row'>CIF</th>";
            xml += "<td>" + dtCommingCustInfo.Rows[0]["CUSTOMER_NO"].ToString() + "</td>";
            xml += "</tr>";
            xml += "<tr>";
            xml += "<th scope='row'>CMND/HC</th>";
            xml += "<td>" + dtCommingCustInfo.Rows[0]["CUSTOMER_IDENTIFY_NO"].ToString() + "</td>";
            xml += "</tr>";
            //caont start
            xml += "<tr>";
            xml += "<th scope='row'>Thời gian chờ </th>";
            xml += "<td>" + dtCommingCustInfo.Rows[0]["TIME_WAIT"].ToString() + " phút : " + dtCommingCustInfo.Rows[0]["SECOND_WAIT"].ToString() + " giây " + "</td>";
            xml += "</tr>";
            //caont end

            xml += "</tbody>";
            xml += "</table>";
            // KienNT 2018/11/22
            if (int.Parse(dtCommingCustInfo.Rows[0]["TIME_WAIT"].ToString()) >= 5)
            {
                xml += "<div style='width: 100%;display: flex;'>";
                xml += "<div id='idBookingCustTitle' class='div-title' style='width: 100%;'>Các dịch vụ yêu cầu</div>";
                xml += "<div onClick='RequestSv.handleGiveGiftClick();' id='btnGiveGift' class='div-gift' style='display:none; width: 80%;'>TẶNG QUÀ</div>";
                xml += "</div>";
            }
            else
            {
                // END KienNT 2018/11/22
                xml += "<div id='idBookingCustTitle' class='div-title'>Các dịch vụ yêu cầu</div>";
            }
            xml += "<table id='tbBookingList' class='table table-striped table-hover'>";
            xml += "<thead>";
            xml += "<tr>";
            xml += "<th>No</th>";
            xml += "<th>Loại giao dịch</th>";
            xml += "</tr>";
            xml += "</thead>";
            xml += "<tbody>";

            for (int i = 0; i < dtBookedServices.Rows.Count; i++)
            {
                if (dtBookedServices.Rows[i]["SERVICE_CODE"].ToString() == "053")
                {
                    xml += "<tr id='tr_cust_track" + dtCommingCustInfo.Rows[0]["CUSTOMER_TRACKING_ID"].ToString() + "_book" + dtBookedServices.Rows[i]["BOOKED_SERVICE_ID"].ToString() + "' onClick='RequestSv.getHTMLCardNoInfo(\"" + dtBookedServices.Rows[i]["BOOKED_SERVICE_ID"].ToString() + "\");'>";
                    xml += "<th scope='row'>" + count++ + "</th>";
                    xml += "<td>" + dtBookedServices.Rows[i]["SERVICE_NAME"].ToString() + " - GD không có thông tin</td>";
                    xml += "</tr>";
                }
                else
                {
                    xml += "<tr id='tr_cust_track" + dtCommingCustInfo.Rows[0]["CUSTOMER_TRACKING_ID"].ToString() + "_book" + dtBookedServices.Rows[i]["BOOKED_SERVICE_ID"].ToString() + "' onClick='RequestSv.getAutoScriptAction(\"GetAutoScript\", \"<custTrackID>" + dtCommingCustInfo.Rows[0]["CUSTOMER_TRACKING_ID"].ToString() + "</custTrackID><bookID>" + dtBookedServices.Rows[i]["BOOKED_SERVICE_ID"].ToString() + "</bookID>\");'>";
                    xml += "<th scope='row'>" + count++ + "</th>";
                    xml += "<td>" + dtBookedServices.Rows[i]["SERVICE_NAME"].ToString() + " - GD không có thông tin</td>";
                    xml += "</tr>";
                }

            }
            int typeService = 0;
            for (int i = 0; i < dtBookedSelfServices.Rows.Count; i++)
            {
                typeService = Int32.Parse(dtBookedSelfServices.Rows[i]["SERVICE_TYPE"].ToString());
                if (dtBookedSelfServices.Rows[i]["SERVICE_TYPE"].ToString() == "046" || dtBookedSelfServices.Rows[i]["SERVICE_TYPE"].ToString() == "052")
                {
                    //RequestSv.sendFCCTransInfo
                    xml += "<tr id='tr_cust_track" + dtCommingCustInfo.Rows[0]["CUSTOMER_TRACKING_ID"].ToString() + "_book" + dtBookedSelfServices.Rows[i]["SERVICEID"].ToString() + "' onClick='RequestSv.getTransDetailInfo(\"GetTransDetailInfo\", \"<custTrackID>" + dtCommingCustInfo.Rows[0]["CUSTOMER_TRACKING_ID"].ToString() + "</custTrackID><bookID>" + dtBookedSelfServices.Rows[i]["SERVICEID"].ToString() + "</bookID>\");'>";
                    xml += "<th scope='row'>" + count++ + "</th>";
                    xml += "<td>" + dtBookedSelfServices.Rows[i]["SERVICE_NAME"].ToString() + " - GD có thông tin</td>";
                    xml += "</tr>";
                }
                else if (dtBookedSelfServices.Rows[i]["SERVICE_TYPE"].ToString() == "053")
                {
                    //RequestSv.sendFCCTransInfo
                    xml += "<tr id='tr_cust_track" + dtCommingCustInfo.Rows[0]["CUSTOMER_TRACKING_ID"].ToString() + "_book" + dtBookedSelfServices.Rows[i]["SERVICEID"].ToString() + "' onClick='RequestSv.getTransDetailInfo(\"GetTransDetailInfo\", \"<custTrackID>" + dtCommingCustInfo.Rows[0]["CUSTOMER_TRACKING_ID"].ToString() + "</custTrackID><bookID>" + dtBookedSelfServices.Rows[i]["SERVICEID"].ToString() + "</bookID>\");'>";
                    xml += "<th scope='row'>" + count++ + "</th>";
                    xml += "<td>" + dtBookedSelfServices.Rows[i]["SERVICE_NAME"].ToString() + " - GD có thông tin</td>";
                    xml += "</tr>";
                }
                else
                {
                    if (typeService > 55)
                    {//caont add
                        xml += "<tr eformid='" + dtBookedSelfServices.Rows[i]["EFORM_ID"].ToString() + "' id='tr_cust_track" + dtCommingCustInfo.Rows[0]["CUSTOMER_TRACKING_ID"].ToString() + "_book" + dtBookedSelfServices.Rows[i]["SERVICEID"].ToString() + "' onClick='RequestSv.openLinkEditInfoEForm(\"GetlinkEditEForm\",  \"<custTrackID>" + dtCommingCustInfo.Rows[0]["CUSTOMER_TRACKING_ID"].ToString() + "</custTrackID><bookID>" + dtBookedSelfServices.Rows[i]["SERVICEID"].ToString() + "</bookID><eformID>" + dtBookedSelfServices.Rows[i]["EFORM_ID"].ToString() + "</eformID><linkEditEform>" + dtBookedSelfServices.Rows[i]["LINK_EDIT_EFORM"].ToString() + "</linkEditEform>\");'>";
                        xml += "<th scope='row'>" + count++ + "</th>";
                        xml += "<td>" + dtBookedSelfServices.Rows[i]["SERVICE_NAME"].ToString() + " - GD có thông tin</td>";
                        xml += "</tr>";

                    }
                    else
                    {
                        xml += "<tr id='tr_cust_track" + dtCommingCustInfo.Rows[0]["CUSTOMER_TRACKING_ID"].ToString() + "_book" + dtBookedSelfServices.Rows[i]["SERVICEID"].ToString() + "' onClick='RequestSv.getAutoScriptAction(\"GetAutoScript\", \"<custTrackID>" + dtCommingCustInfo.Rows[0]["CUSTOMER_TRACKING_ID"].ToString() + "</custTrackID><bookID>" + dtBookedSelfServices.Rows[i]["SERVICEID"].ToString() + "</bookID>\");'>";
                        xml += "<th scope='row'>" + count++ + "</th>";
                        xml += "<td>" + dtBookedSelfServices.Rows[i]["SERVICE_NAME"].ToString() + " - GD có thông tin</td>";
                        xml += "</tr>";

                    }


                }
            }

            //for (int i = 0; i < dtBookedEBankServices.Rows.Count; i++)
            //{
            //    xml += "<tr id='tr_cust_track" + dtCommingCustInfo.Rows[0]["CUSTOMER_TRACKING_ID"].ToString() + "_book" + dtBookedServices.Rows[i]["RESERVATION_ID"].ToString() + "' onClick='RequestSv.getAutoScriptAction(\"GetAutoScript\", \"<custTrackID>" + dtCommingCustInfo.Rows[0]["CUSTOMER_TRACKING_ID"].ToString() + "</custTrackID><bookID>" + dtBookedSelfServices.Rows[0]["RESERVATION_ID"].ToString() + "</bookID>\");'>";
            //    xml += "<th scope='row'>" + count++ + "</th>";
            //    xml += "<td>" + dtBookedSelfServices.Rows[0]["TRANS_TYPE"].ToString() + "</td>";
            //    xml += "</tr>";
            //}

            //xml += "<tr onClick='RequestSv.getAutoScriptAction(\"GetAutoScript\", \"<custTrackID>20762</custTrackID><bookID>27</bookID>\");'>";
            //xml += "<th scope='row'>1</th>";
            //xml += "<td>Nộp tiền</td>";
            //xml += "</tr>";
            //xml += "<tr onClick='RequestSv.getAutoScriptAction(\"GetAutoScript\", \"<custTrackID>20762</custTrackID><bookID>27</bookID>\");'>"; ;
            //xml += "<th scope='row'>2</th>";
            //xml += "<td>Rút tiền</td>";
            //xml += "</tr>";
            xml += "</tbody>";
            xml += "</table>";
            xml += "<input id=\"txtParamsStartServing\" type=\"text\" style=\"display:none;\" value=\"" + "<customerCIF>" + dtCommingCustInfo.Rows[0]["CUSTOMER_NO"].ToString() + "</customerCIF><currentCustID>" + dtCommingCustInfo.Rows[0]["CUSTOMER_IDENTIFY_NO"].ToString() + "</currentCustID><currentCustSmartCardNo>" + dtCommingCustInfo.Rows[0]["CUSTOMER_SMARTCARD_NO"].ToString() + "</currentCustSmartCardNo><counterUsername>___COUNTER_USERNAME___</counterUsername><currentCustTrackingID>" + dtCommingCustInfo.Rows[0]["CUSTOMER_TRACKING_ID"].ToString() + "</currentCustTrackingID>" + "\">"; //Dong nay khong duoc thay doi, dung de tra ve cho client//COUNTER_USERNAME

            return xml;
        }





        [WebMethod]
        public void UpdateExitTellerClause(string counterUsername, string clauseExit, string myIP)
        {
            SqlConnection con = getConnection();
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            try
            {
                SqlCommand cmd = new SqlCommand("sp_update_exit_clause", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@COUNTER_USERNAME", counterUsername);
                cmd.Parameters.AddWithValue("@CLAUSE_EXIT", clauseExit);
                cmd.Parameters.AddWithValue("@COUNTER_IP", myIP);

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
        }



        [WebMethod]//<customerCIF></customerCIF><currentCustID></currentCustID><currentCustSmartCardNo></currentCustSmartCardNo><counterUsername></counterUsername><currentCustTrackingID></currentCustTrackingID>
        public string LockCustomerForServing(string customerCIF, string currentCustID, string currentCustSmartCardNo, string counterUsername, string currentCustTrackingID)
        {
            string xml = "";
            DataSet ds = new DataSet();
            SqlConnection con = getConnection();
            SqlCommand cmd;
            SqlDataAdapter _da;
            DataTable dtListTrans = new DataTable();
            eCounterWebReference1.ECounterWebserviceVer5 eCounterWR = new eCounterWebReference1.ECounterWebserviceVer5();

            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            try
            {
                //Kiem tra xem Teller co dang phuc vu khach hang nao hay khong
                SqlDataAdapter _da1;
                DataSet _ds = new DataSet();
                cmd = new SqlCommand("SELECT ID, CUSTOMER_TRACKING_ID, AMT_BLOCK_ID FROM dbo.BOOKED_SELF_SERVICE WHERE AMT_BLOCK_ID IS NOT NULL AND AMT_BLOCK_ID <> '' AND AMT_BLOCK_ID <> 'ERROR' AND CUSTOMER_TRACKING_ID = '" + currentCustTrackingID + "'", con);
                _da1 = new SqlDataAdapter(cmd);
                _da1.Fill(_ds);
                dtListTrans = _ds.Tables[0];

            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
                xml = "ERROR: " + ex.Message;
            }
            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }



            //try
            //{
            //    for (int i = 0; i < dtListTrans.Rows.Count; i++)
            //    {
            //        if (con.State == ConnectionState.Closed)
            //        {
            //            con.Open();
            //        }

            //        cmd = new SqlCommand("sp_unblock_amount_processor", con);
            //        cmd.CommandType = CommandType.StoredProcedure;
            //        cmd.Parameters.AddWithValue("@custTrackID", currentCustTrackingID);
            //        cmd.Parameters.AddWithValue("@bookID", dtListTrans.Rows[i]["ID"].ToString());
            //        _da = new SqlDataAdapter(cmd);
            //        _da.Fill(ds);
            //        DataTable dt = ds.Tables[0];

            //        xml = dt.Rows[0]["RESULT"].ToString();

            //        con.Close();
            //    }
            //}
            //catch (Exception ex)
            //{
            //    xml = "ERROR: " + ex.Message;
            //}


            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            try
            {
                cmd = new SqlCommand("sp_LockCustomerForServing", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CUSTOMER_CIF", customerCIF);
                cmd.Parameters.AddWithValue("@CUSTOMER_IDENTIFY_NO", currentCustID);
                cmd.Parameters.AddWithValue("@CUSTOMER_SMARTCARD_NO", currentCustSmartCardNo);
                cmd.Parameters.AddWithValue("@COUNTER_USERNAME", counterUsername);
                cmd.Parameters.AddWithValue("@CUSTOMER_TRACKING_ID", currentCustTrackingID);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(ds);
                int k;
                k = ds.Tables.Count;
                DataTable dt = ds.Tables[0];
                if (dt.Rows.Count > 0)
                {
                    string str_bookFrom = dt.Rows[0]["BOOK_FROM"].ToString();
                    string str_brn = dt.Rows[0]["this_branch"].ToString();
                    xml = dt.Rows[0]["CUSTOMER_NAME"].ToString() + "#" + dt.Rows[0]["CUSTOMER_CODE"].ToString() + "#" + dt.Rows[0]["CUSTOMER_CODE_TYPE"].ToString();// +"#" + str_bookFrom + "#" + customerCIF;

                    if (str_bookFrom.Equals("EBANK") && customerCIF.Length == 8)
                    {
                        eCounterWR.UpdateEcounterState(customerCIF + "#" + str_brn);
                    }
                }


            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }

            return xml;
        }
        [WebMethod]
        public DataSet MarkCompleteServing(string counterUsername, string currentCustTrackID)
        {
            DataSet ds = new DataSet();
            SqlConnection con = getConnection();
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            try
            {
                SqlCommand cmd = new SqlCommand("sp_MarkCompleteServing", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@COUNTER_USERNAME", counterUsername);
                cmd.Parameters.AddWithValue("@CUSTOMER_TRACKING_ID", currentCustTrackID);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(ds);

            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }

            return ds;
        }

        [WebMethod]
        public DataSet MarkCompleteServingPre(string counterUsername, string currentCustTrackID)
        {
            DataSet ds = new DataSet();
            SqlConnection con = getConnection();
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            try
            {
                SqlCommand cmd = new SqlCommand("sp_MarkCompleteServingPre", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@COUNTER_USERNAME", counterUsername);
                cmd.Parameters.AddWithValue("@CUSTOMER_TRACKING_ID", currentCustTrackID);
                cmd.ExecuteNonQuery();
                /*SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(ds);
                DataTable dt = new DataTable();
                dt = ds.Tables[0];
                if (dt.Rows.Count > 0)
                {
                    eCounterWebReference1.ECounterWebserviceVer5 ws = new eCounterWebReference1.ECounterWebserviceVer5();
                    string temp = dt.Rows[0][0].ToString();
                    ws.UpdateEcounterState(temp);
                }*/

            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }

            return ds;
        }

        [WebMethod]
        public String GetFCCInfoTemplate(string custTrackingID, string custBookID)
        {
            String xml = "";
            SqlConnection con = getConnection();
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            try
            {
                SqlCommand cmd = new SqlCommand("SP_GET_FCC_INFO_TEMPLATE", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@CUST_TRACKING_ID", SqlDbType.NVarChar).Value = custTrackingID;
                cmd.Parameters.Add("@CUST_BOOK_ID", SqlDbType.NVarChar).Value = custBookID;

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                foreach (DataRow row in dt.Rows)
                {
                    xml = row["TEMP_CONTENT"].ToString();
                    break;
                }

            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
            return xml;
        }

        [WebMethod]
        public string SendReviewTransContent(string counterUsername, string reviewContent, string custTrackingID, string custBookID)
        {
            string xml = "DONE";
            SqlConnection con = getConnection();
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            try
            {
                string test1 = GetTemplateFCCMessage(custTrackingID, custBookID, reviewContent) + custTrackingID + "#" + custBookID;
                string test2 = GetComplatePrintContent(custTrackingID, custBookID, reviewContent);
                SqlCommand cmd = new SqlCommand("sp_send_review_content", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@CUST_TRACK_ID", SqlDbType.NVarChar).Value = custTrackingID;
                cmd.Parameters.Add("@BOOK_ID", SqlDbType.NVarChar).Value = custBookID;
                cmd.Parameters.Add("@COUNTER_USERNAME", SqlDbType.NVarChar).Value = counterUsername;
                cmd.Parameters.Add("@REVIEW_CONTENT", SqlDbType.NVarChar).Value = GetTemplateFCCMessage(custTrackingID, custBookID, reviewContent) + custTrackingID + "#" + custBookID;
                cmd.Parameters.Add("@PRINT_CONTENT", SqlDbType.NVarChar).Value = GetComplatePrintContent(custTrackingID, custBookID, reviewContent);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
            return xml;
        }

        private string GetComplatePrintContent(string custTrackingID, string custBookingID, string content)
        {
            string xml = "";
            SqlConnection con = getConnection();
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            try
            {
                SqlCommand cmd = new SqlCommand("sp_get_fcc_template_message", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@CUST_TRACKING_ID", SqlDbType.NVarChar).Value = custTrackingID;
                cmd.Parameters.Add("@CUST_BOOK_ID", SqlDbType.NVarChar).Value = custBookingID;
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);
                DataTable dt = new DataTable();
                dt = ds.Tables[0];

                Utilities utilities = new Utilities();

                xml += utilities.GeneratePrintContent(dt.Rows[0]["ECOUNTER_FUNC"].ToString().Trim(), content, dt.Rows[0]["PRINT_CONTENT"].ToString(), dt.Rows[0]["USER_CONTENT"].ToString());
                //if (dt.Rows[0]["ECOUNTER_FUNC"].ToString().Trim() == "013")
                //{

                //}
                //else if (dt.Rows[0]["ECOUNTER_FUNC"].ToString().Trim() == "008")
                //{

                //    //string firstContent = dt.Rows[0]["USER_CONTENT"].ToString();
                //   // xml = xml + firstContent + contentArr[6].ToString() + "#" + contentArr[8].ToString() + "#" + contentArr[3].ToString() + " " + contentArr[1].ToString() + "#" + contentArr[4].ToString() + "#" + ConvertFromNumToWord(Convert.ToDouble(contentArr[3].ToString())) + " " + ConvertCurrencyToVietnamWord(contentArr[1].ToString()) + "#" + contentArr[5].ToString() + " đồng#" + ConvertFromNumToWord(Convert.ToDouble(contentArr[5].ToString()));
                //}

                //else
                //{

                //}
            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
            return xml;
        }

        private string GetTemplateFCCMessage(string custTrackingID, string custBookingID, string content)
        {
            string xml = "";
            int count = 0;
            SqlConnection con = getConnection();
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            try
            {
                Utilities utilities = new Utilities();

                SqlCommand cmd = new SqlCommand("sp_get_fcc_template_message", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@CUST_TRACKING_ID", SqlDbType.NVarChar).Value = custTrackingID;
                cmd.Parameters.Add("@CUST_BOOK_ID", SqlDbType.NVarChar).Value = custBookingID;
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);
                DataTable dt = new DataTable();
                dt = ds.Tables[0];

                xml += utilities.GenerateDataUsingFCCTemplate(dt.Rows[0]["ECOUNTER_FUNC"].ToString().Trim(), content, dt.Rows[0]["TITLE"].ToString(), dt.Rows[0]["TEMP_NAME"].ToString(), dt.Rows[0]["PRINT_CONTENT"].ToString(), custTrackingID, custBookingID);

                //if (dt.Rows[0]["ECOUNTER_FUNC"].ToString().Trim() == "013")
                //{


                //}
                //else if (dt.Rows[0]["ECOUNTER_FUNC"].ToString().Trim() == "008")
                //{

                //}
                //else if (dt.Rows[0]["ECOUNTER_FUNC"].ToString().Trim() == "007")
                //{


                //}
                //else if (dt.Rows[0]["ECOUNTER_FUNC"].ToString().Trim() == "045")
                //{

                //}

                xml += dt.Rows[0]["ECOUNTER_FUNC"].ToString() + "#";
            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
                xml = "ERROR index " + count.ToString();
            }
            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
            return xml;
        }

        [WebMethod]
        public string MarkCurrentTransComplete(string counterUserName, string custTrackingID, string bookID)
        {
            string xml = "DONE";
            SqlConnection con = getConnection();
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            try
            {
                SqlCommand cmd = new SqlCommand("sp_finish_current_trans", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@COUNTER_USERNAME", SqlDbType.NVarChar).Value = counterUserName;
                cmd.Parameters.Add("@CUST_TRACK_ID", SqlDbType.NVarChar).Value = custTrackingID;
                cmd.Parameters.Add("@BOOK_ID", SqlDbType.NVarChar).Value = bookID;
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
                xml = "ERROR";
            }
            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
            return xml;
        }

        [WebMethod]
        public string RequireResetCustSignature(string counterUsername, string custTrackingID, string bookID)
        {
            string xml = "DONE";
            SqlConnection con = getConnection();
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            try
            {
                SqlCommand cmd = new SqlCommand("sp_require_reset_signature", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@COUNTER_USERNAME", SqlDbType.NVarChar).Value = counterUsername;
                cmd.Parameters.Add("@CUST_TRACK_ID", SqlDbType.NVarChar).Value = custTrackingID;
                cmd.Parameters.Add("@BOOK_ID", SqlDbType.NVarChar).Value = bookID;
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
                xml = "ERROR";
            }
            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
            return xml;
        }

        [WebMethod]
        public List<string> CashWithdrawBookProcessor(string CIF, string bookType, string sourceAccountNo, string sourceAccountName, string desAccountNo, string desAccountName, string amount, string citadCode, string feeChargesStyle, string description, string selectedTable)
        {
            List<string> xml = new List<string>();
            int customerSelectedTable = -1;
            if (selectedTable.Length > 0)
            {
                customerSelectedTable = Convert.ToInt32(selectedTable);
            }

            SqlConnection con = getConnection();
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            try
            {
                SqlCommand cmd = new SqlCommand("sp_SaveBookedSelfService", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@CIF", SqlDbType.NVarChar).Value = CIF;
                cmd.Parameters.Add("@SELF_SERVICE_TYPE", SqlDbType.NVarChar).Value = bookType;
                cmd.Parameters.Add("@SOURCE_ACCOUNT_NO", SqlDbType.NVarChar).Value = sourceAccountNo;
                cmd.Parameters.Add("@SOURCE_ACCOUNT_NAME", SqlDbType.NVarChar).Value = sourceAccountName;
                cmd.Parameters.Add("@DES_ACCOUNT_NO", SqlDbType.NVarChar).Value = desAccountNo;
                cmd.Parameters.Add("@DES_ACCOUNT_NAME", SqlDbType.NVarChar).Value = desAccountName;
                cmd.Parameters.Add("@AMOUNT", SqlDbType.NVarChar).Value = amount;
                cmd.Parameters.Add("@CITAD_CODE", SqlDbType.NVarChar).Value = citadCode;
                cmd.Parameters.Add("@FEE_CHARGE_STYLE", SqlDbType.NVarChar).Value = feeChargesStyle;
                cmd.Parameters.Add("@DESCRIPTION", SqlDbType.NVarChar).Value = description;
                cmd.Parameters.Add("@SELECTED_DETECT_POINT_ID", SqlDbType.NVarChar).Value = customerSelectedTable;
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);
                DataTable dtDetectPointDes = ds.Tables[0];
                DataTable dtCustBookInfo = ds.Tables[1];

                string counterTable = dtDetectPointDes.Rows[0][0].ToString();
                if (counterTable == null)
                {
                    counterTable = "";
                }
                xml.Add(counterTable);
                xml.Add(dtCustBookInfo.Rows[0]["BRN"].ToString());
                xml.Add(dtCustBookInfo.Rows[0]["CUSTOMER_TRACKING_ID"].ToString());
                xml.Add(dtCustBookInfo.Rows[0]["BOOK_ID"].ToString());
            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
                xml.Add("ERROR");
                xml.Add(ex.Message);
            }
            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
            return xml;
        }

        [WebMethod]
        public string GetCommingCustomerProcessor_Greeter(string counterUsername)
        {
            string xml = "";
            SqlConnection con = getConnection();
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            try
            {
                SqlCommand cmd = new SqlCommand("sp_GetComingCustomer_Greeter", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@COUNTER_USERNAME", SqlDbType.NVarChar).Value = counterUsername;
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);
                DataTable dt = new DataTable();
                dt = ds.Tables[0];

                xml = dt.Rows[0]["CUSTOMER_NAME"].ToString();
                string stt = dt.Rows[0]["STT_USER"].ToString();
                xml = xml + " Số thứ tự " + stt;
            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
                xml = "ERROR";
            }
            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
            return xml;
        }

        [WebMethod]
        public string IsBeginServingCustomerProcessor(string counterUsername)
        {
            string xml = "";
            SqlConnection con = getConnection();
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            try
            {
                SqlCommand cmd = new SqlCommand("sp_is_begin_serving_cust", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@COUNTER_USERNAME", SqlDbType.NVarChar).Value = counterUsername;
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);
                DataTable dt = new DataTable();
                dt = ds.Tables[0];

                xml = dt.Rows[0]["IS_BEGIN_SERVING"].ToString() + "#" + dt.Rows[0]["FULL_NAME"].ToString();
            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
                xml = "ERROR";
            }
            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
            return xml;
        }


        private string getBRN()
        {
            string brn = "";
            DataSet ds = new DataSet();
            SqlConnection con = getConnection();
            SqlCommand cmd;
            SqlDataAdapter _da;
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            try
            {
                cmd = new SqlCommand("SELECT * FROM BRN", con);
                cmd.CommandType = CommandType.Text;
                _da = new SqlDataAdapter(cmd);
                _da.Fill(ds);
                DataTable dt = ds.Tables[0];

                brn = dt.Rows[0][0].ToString();
            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }

            return brn;
        }


        //SangNT1 - Update card support

        [WebMethod]//custTrackID = 20762, bookID = 27
        public string UpdateCardSupportInfo(string custTrackingID, string bookID, string parameters)
        {
            //string unblockStatus = UnblockAmountProcessor(custTrackID, bookID);

            string[] listInfo = parameters.Split('#');

            string xml = "";
            DataSet ds = new DataSet();
            SqlConnection con = getConnection();
            SqlCommand cmd;
            SqlDataAdapter _da;
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            try
            {
                //Kiem tra xem Teller co dang phuc vu khach hang nao hay khong
                cmd = new SqlCommand("SP_SAVE_CARD_SUPPORT_INFO", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CUST_TRACKING_ID", custTrackingID);
                cmd.Parameters.AddWithValue("@BOOK_ID", bookID);
                cmd.Parameters.AddWithValue("@CIF", listInfo[2]);
                cmd.Parameters.AddWithValue("@TEN", listInfo[3]);
                cmd.Parameters.AddWithValue("@SOTK", listInfo[4]);
                cmd.Parameters.AddWithValue("@SOTHE", listInfo[5]);
                cmd.Parameters.AddWithValue("@CMND", listInfo[6]);
                cmd.Parameters.AddWithValue("@NGAYCAP", listInfo[7]);
                cmd.Parameters.AddWithValue("@NOICAP", listInfo[8]);
                cmd.Parameters.AddWithValue("@KHOATHE", listInfo[9]);
                cmd.Parameters.AddWithValue("@KHPIN", listInfo[10]);
                cmd.Parameters.AddWithValue("@HUYTKLK", listInfo[11]);
                cmd.Parameters.AddWithValue("@TKHUY", listInfo[12]);
                cmd.Parameters.AddWithValue("@DOITKMD", listInfo[13]);
                cmd.Parameters.AddWithValue("@NHAPDOITKMD", listInfo[14]);
                cmd.Parameters.AddWithValue("@MOKHOATHE", listInfo[15]);
                cmd.Parameters.AddWithValue("@NEWPIN", listInfo[16]);
                cmd.Parameters.AddWithValue("@THEMTKLKTHE", listInfo[17]);
                cmd.Parameters.AddWithValue("@TKMOI", listInfo[18]);
                cmd.Parameters.AddWithValue("@NGUNGSDTHE", listInfo[19]);
                cmd.Parameters.AddWithValue("@YCKHAC", listInfo[20]);
                cmd.Parameters.AddWithValue("@NOIDUNGYC", listInfo[21]);
                cmd.Parameters.AddWithValue("@THECHINH", listInfo[22]);
                cmd.Parameters.AddWithValue("@THEPHU", listInfo[23]);
                cmd.Parameters.AddWithValue("@LAYTHENUOT", listInfo[24]);
                cmd.Parameters.AddWithValue("@KHIEUNAI", listInfo[25]);
                cmd.Parameters.AddWithValue("@NDLAYTHENUOT", listInfo[26]);
                cmd.Parameters.AddWithValue("@NDKHIEUNAI", listInfo[27]);
                _da = new SqlDataAdapter(cmd);
                _da.Fill(ds);
                DataTable dt = ds.Tables[0];

                xml = dt.Rows[0]["RESULT"].ToString();

            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
                xml = "ERROR: " + ex.Message;
            }
            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }


            return xml;
        }

        //minhdv1

        private string addDataToPageForEForm(DataSet ds, string listBooking, bool is_book, string xmlCustomer, string identifier)
        {
            string xml = "";
            XmlDocument xmlListBooking = new XmlDocument();
            xmlListBooking.LoadXml(listBooking);
            XmlNodeList nodelist = xmlListBooking.DocumentElement.SelectNodes("/root/O_LIST_EFORM");
            XmlDocument xmlCus = new XmlDocument();
            xmlCus.LoadXml(xmlCustomer);
            int countP = 1;
            int countY = 1;
            int countD = 1;

            DataTable dtCommingCustInfo = new DataTable();
            DataTable dtBookedSelfServices = new DataTable();
            if (is_book)
            {
                dtCommingCustInfo = ds.Tables[0];
                dtBookedSelfServices = ds.Tables[1];

                string sex = dtCommingCustInfo.Rows[0]["SEX"].ToString() == "M" ? "Nam" : "Nữ";

                xml += "<div id='idComingCustTitle' class='div-title'>Thông tin khách hàng sắp đến quầy GD</div>";
                xml += "<table class='table table-striped'>";
                xml += "<tbody>";
                xml += "<tr>";
                xml += "<th scope='row'>Tên</th>";
                xml += "<td>" + dtCommingCustInfo.Rows[0]["CUSTOMER_NAME"].ToString() + " - STT " + dtCommingCustInfo.Rows[0]["STT_USER"].ToString() + "</td>";
                xml += "</tr>";
                xml += "<tr>";
                xml += "<th scope='row'>Giới tính</th>";
                xml += "<td>" + sex + "</td>";
                xml += "</tr>";
                xml += "<tr>";
                xml += "<th scope='row'>Loại</th>";
                xml += "<td>" + dtCommingCustInfo.Rows[0]["TYPE_NAME"].ToString() + "</td>";
                xml += "</tr>";
                xml += "<tr>";
                xml += "<th scope='row'>CIF</th>";
                xml += "<td>" + dtCommingCustInfo.Rows[0]["CUSTOMER_NO"].ToString() + "</td>";
                xml += "</tr>";
                xml += "<tr>";
                xml += "<th scope='row'>CMND/HC</th>";
                xml += "<td>" + dtCommingCustInfo.Rows[0]["CUSTOMER_IDENTIFY_NO"].ToString() + "</td>";
                xml += "</tr>";
                //caont start
                xml += "<tr>";
                xml += "<th scope='row'>SĐT</th>";
                xml += "<td>" + formatPhone(xmlCus.GetElementsByTagName("CUSTOMER_PHONE")[0].InnerText) + "</td>";
                xml += "</tr>";
                //caont end

                xml += "</tbody>";
                xml += "</table>";
            }
            else
            {
                string sex = xmlCus.GetElementsByTagName("CUSTOMER_SEX")[0].InnerText == "M" ? "Nam" : "Nữ";

                xml += "<div id='idComingCustTitle' class='div-title'>Thông tin khách hàng sắp đến quầy GD</div>";
                xml += "<table class='table table-striped'>";
                xml += "<tbody>";
                xml += "<tr>";
                xml += "<th scope='row'>Tên</th>";
                xml += "<td>" + xmlCus.GetElementsByTagName("CUSTOMER_NAME")[0].InnerText + " - STT 1</td>";
                xml += "</tr>";
                xml += "<tr>";
                xml += "<th scope='row'>Giới tính</th>";
                xml += "<td>" + sex + "</td>";
                xml += "</tr>";
                xml += "<tr>";
                xml += "<th scope='row'>Loại</th>";
                xml += "<td>" + xmlCus.GetElementsByTagName("CUSTOMER_TYPE")[0].InnerText + "</td>";
                xml += "</tr>";
                xml += "<tr>";
                xml += "<th scope='row'>CIF</th>";
                xml += "<td>" + xmlCus.GetElementsByTagName("CUSTOMER_NO")[0].InnerText + "</td>";
                xml += "</tr>";
                xml += "<tr>";
                xml += "<th scope='row'>CMND/HC</th>";
                xml += "<td>" + identifier + "</td>";
                xml += "</tr>";
                //caont start
                xml += "<tr>";
                xml += "<th scope='row'>SĐT</th>";
                xml += "<td>" + formatPhone(xmlCus.GetElementsByTagName("CUSTOMER_PHONE")[0].InnerText) + "</td>";
                xml += "</tr>";
                //caont end

                xml += "</tbody>";
                xml += "</table>";
            }
            //minhdv1
            xml += "<div id='idBookingCustTitle' class='div-title' style='margin-bottom:5px;'>Các dịch vụ yêu cầu</div>";
            xml += "<div id='tbBookingEForm'>";
            xml += "<div id='ListPTitle' class='div-title'>Chờ xử lý</div>";
            xml += "<table id='tbBookingList' class='table table-striped table-hover tbBooking active'>";
            xml += "<thead>";
            xml += "<tr>";
            xml += "<th>No</th>";
            xml += "<th>Loại giao dịch</th>";
            xml += "</tr>";
            xml += "</thead>";
            xml += "<tbody>";
            for (int i = 0; i < dtBookedSelfServices.Rows.Count; i++)
            {
                xml += "<tr eformid='" + dtBookedSelfServices.Rows[i]["EFORM_ID"].ToString() + "' id='tr_cust_track" + dtCommingCustInfo.Rows[0]["CUSTOMER_TRACKING_ID"].ToString() + "_book" + dtBookedSelfServices.Rows[i]["SERVICEID"].ToString() + "' onClick='RequestSv.openLinkEditInfoEForm(\"GetlinkEditEForm\",  \"<custTrackID>" + dtCommingCustInfo.Rows[0]["CUSTOMER_TRACKING_ID"].ToString() + "</custTrackID><bookID>" + dtBookedSelfServices.Rows[i]["SERVICEID"].ToString() + "</bookID><eformID>" + dtBookedSelfServices.Rows[i]["EFORM_ID"].ToString() + "</eformID><linkEditEform>" + dtBookedSelfServices.Rows[i]["LINK_EDIT_EFORM"].ToString() + "</linkEditEform>\");'>";
                xml += "<th scope='row'>" + countP++ + "</th>";
                xml += "<td>" + dtBookedSelfServices.Rows[i]["SERVICE_NAME"].ToString() + " - GD có thông tin</td>";
                xml += "</tr>";
            }
            xml += "</tbody>";
            xml += "</table>";
            xml += "<div id='ListPTitle' class='div-title'>Đã xử lý</div>";
            xml += "<table id='tbBookingListP' class='table table-striped table-hover tbBooking active'>";
            xml += "<thead>";
            xml += "<tr>";
            xml += "<th>No</th>";
            xml += "<th>Loại giao dịch</th>";
            xml += "</tr>";
            xml += "</thead>";
            xml += "<tbody>";
            for (int i = 0; i < nodelist[0].ChildNodes.Count; i++)
            {
                string form_status = nodelist[0].ChildNodes[i].SelectSingleNode("formStatus").InnerText;
                if (form_status == "Y")
                {
                    xml += "<tr id='tr_cust_track" + nodelist[0].ChildNodes[i].SelectSingleNode("eformId").InnerText + "' onClick='RequestSv.openLinkInfoEForm(\"GetlinkEditEForm\",  \"<eformID>" + nodelist[0].ChildNodes[i].SelectSingleNode("eformId").InnerText + "</eformID><linkEditEform>" + nodelist[0].ChildNodes[i].SelectSingleNode("linkEditInfo").InnerText + "</linkEditEform>\");'>";
                    xml += "<th scope='row'>" + countY++ + "</th>";
                    xml += "<td>" + nodelist[0].ChildNodes[i].SelectSingleNode("serviceName").InnerText + " - GD có thông tin<br>";
                    xml += "User nhập: " + nodelist[0].ChildNodes[i].SelectSingleNode("updateBy").InnerText + " - Ngày nhập: " + formatDateTime(nodelist[0].ChildNodes[i].SelectSingleNode("updateDate").InnerText) + "</td>";
                    xml += "</tr>";
                }
            }
            xml += "</tbody>";
            xml += "</table>";
            xml += "<div id='ListPTitle' class='div-title'>Hết hiệu lực</div>";
            xml += "<table id='tbBookingListP' class='table table-striped table-hover tbBooking active'>";
            xml += "<thead>";
            xml += "<tr>";
            xml += "<th>No</th>";
            xml += "<th>Loại giao dịch</th>";
            xml += "</tr>";
            xml += "</thead>";
            xml += "<tbody>";
            for (int i = 0; i < nodelist[0].ChildNodes.Count; i++)
            {
                string form_status = nodelist[0].ChildNodes[i].SelectSingleNode("formStatus").InnerText;
                if (form_status == "D")
                {
                    xml += "<tr id='tr_cust_track" + nodelist[0].ChildNodes[i].SelectSingleNode("eformId").InnerText + "' onClick='RequestSv.openLinkInfoEForm(\"GetlinkEditEForm\",  \"<eformID>" + nodelist[0].ChildNodes[i].SelectSingleNode("eformId").InnerText + "</eformID><linkEditEform>" + nodelist[0].ChildNodes[i].SelectSingleNode("linkEditInfo").InnerText + "</linkEditEform>\");'>";
                    xml += "<th scope='row'>" + countD++ + "</th>";
                    xml += "<td>" + nodelist[0].ChildNodes[i].SelectSingleNode("serviceName").InnerText + " - GD có thông tin</td>";
                    xml += "</tr>";
                }
            }
            xml += "</tbody>";
            xml += "</table>";
            if (is_book)
            {
                xml += "<input id=\"txtParamsStartServing\" type=\"text\" style=\"display:none;\" value=\"" + "<customerCIF>" + dtCommingCustInfo.Rows[0]["CUSTOMER_NO"].ToString() + "</customerCIF><currentCustID>" + dtCommingCustInfo.Rows[0]["CUSTOMER_IDENTIFY_NO"].ToString() + "</currentCustID><currentCustSmartCardNo>" + dtCommingCustInfo.Rows[0]["CUSTOMER_SMARTCARD_NO"].ToString() + "</currentCustSmartCardNo><counterUsername>___COUNTER_USERNAME___</counterUsername><currentCustTrackingID>" + dtCommingCustInfo.Rows[0]["CUSTOMER_TRACKING_ID"].ToString() + "</currentCustTrackingID>" + "\">"; //Dong nay khong duoc thay doi, dung de tra ve cho client//COUNTER_USERNAME
            }
            else
            {
                xml += "<input id=\"txtParamsStartServing\" type=\"text\" style=\"display:none;\" value=\"" + "<customerCIF>" + xmlCus.GetElementsByTagName("CUSTOMER_NO")[0].InnerText + "</customerCIF><currentCustID>" + identifier + "</currentCustID><currentCustSmartCardNo></currentCustSmartCardNo><counterUsername>___COUNTER_USERNAME___</counterUsername><currentCustTrackingID></currentCustTrackingID>" + "\">"; //Dong nay khong duoc thay doi, dung de tra ve cho client//COUNTER_USERNAME
            }
            return xml;
        }

        //minh add for EForm
        public string BookInstantlyForEForm(string code, string xmlCusName, string xmlBookInfo, string counterUsername)
        {
            string xml = "";//[sp_GetPreServingCustomers]

            DataSet ds = new DataSet();
            SqlConnection con = getConnection();
            SqlCommand cmd;
            SqlDataAdapter _da;
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            try
            {
                //Kiem tra xem Teller co dang phuc vu khach hang nao hay khong
                cmd = new SqlCommand("sp_BookInstantlyForEForm", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CODE", code);
                cmd.Parameters.AddWithValue("@COUNTER_USERNAME", counterUsername);
                cmd.Parameters.AddWithValue("@xmlCusName", xmlCusName);
                cmd.Parameters.AddWithValue("@xmlBookInfo", xmlBookInfo);
                _da = new SqlDataAdapter(cmd);
                _da.Fill(ds);
                DataTable dt = ds.Tables[0];

                if (dt.Rows.Count > 0)
                {
                    xml += "<RESP_CODE>" + dt.Rows[0]["RESP_CODE"] + "</RESP_CODE>";
                    xml += "<RESP_CONTENT>" + dt.Rows[0]["RESP_CONTENT"] + "</RESP_CONTENT>";
                    xml += "<CUSTOMER_TRACKING_ID>" + dt.Rows[0]["CUSTOMER_TRACKING_ID"] + "</CUSTOMER_TRACKING_ID>";
                }

            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
                xml += "<RESP_CODE>2000</RESP_CODE>";
                xml += "<RESP_CONTENT>" + ex.Message + "</RESP_CONTENT>";
                xml += "<CUSTOMER_TRACKING_ID></CUSTOMER_TRACKING_ID>";
            }
            if (con.State == ConnectionState.Open)
                con.Close();

            return xml;
        }

        [WebMethod]
        public string SearchDataFromEForm(string formID, string formType, string formStatus, string customerIndentify, string startdate, string enddate, string phoneNbr, string userRequest, string counterUsername)
        {
            string xmlResult = "";
            string transList = "";
            string name_customer = "";
            string identifier = "";
            string phoneNumber = "";
            string gender = "";
            string cif = "";
            string code = "";
            bool is_book = false;
            XmlDocument xDoc = new XmlDocument();
            try
            {
                eCounterWebReference1.ECounterWebserviceVer5 webService = new eCounterWebReference1.ECounterWebserviceVer5();
                transList = webService.SearchDataFromEForm(formID, formType, formStatus, customerIndentify, startdate, enddate, phoneNbr, userRequest);
                xDoc.LoadXml("<root>" + transList + "</root>");

                string respCode = xDoc.GetElementsByTagName("O_ERRCODE")[0].InnerText;

                if (respCode != "" && respCode.Equals("0"))
                {

                    XmlNodeList nodelist = xDoc.DocumentElement.SelectNodes("/root/O_LIST_RESULT");

                    name_customer = nodelist[0].ChildNodes[0].SelectSingleNode("CUSTOMER_NAME").InnerText;
                    identifier = nodelist[0].ChildNodes[0].SelectSingleNode("CUSTOMER_IDENTIFY_VALUE").InnerText;
                    phoneNumber = nodelist[0].ChildNodes[0].SelectSingleNode("CUSTOMER_MOBILE_NO").InnerText;
                    gender = nodelist[0].ChildNodes[0].SelectSingleNode("CUSTOMER_GENDER").InnerText == "1" ? "M" : "F";
                    cif = nodelist[0].ChildNodes[0].SelectSingleNode("CUSTOMER_CIF").InnerText;
                    string xmlCusName = "";


                    if (cif == "" || cif.Length == 0)
                    {
                        xmlCusName += "<row>";
                        xmlCusName += "<CUSTOMER_NAME>" + name_customer + "</CUSTOMER_NAME>";
                        xmlCusName += "<CUSTOMER_TYPE>NORMAL</CUSTOMER_TYPE>";
                        xmlCusName += "<CUSTOMER_SEX>" + gender + "</CUSTOMER_SEX>";
                        xmlCusName += "<CUSTOMER_PHONE>" + phoneNumber + "</CUSTOMER_PHONE>";
                        xmlCusName += "<CUSTOMER_NO>" + identifier + "</CUSTOMER_NO>";
                        xmlCusName += "<REF_CODE>00</REF_CODE>";
                        xmlCusName += "<CUS_DOI></CUS_DOI>";
                        xmlCusName += "<CUS_POI></CUS_POI>";
                        xmlCusName += "</row>";
                        code = identifier;

                    }
                    else
                    {
                        eCounterWebReference1.ECounterWebserviceVer5 eCounterWR = new eCounterWebReference1.ECounterWebserviceVer5();
                        xmlCusName = eCounterWR.LookupCusNameFromCode_EFORM(cif, "CI");
                        code = cif;
                    }
                     string customer_nameFCC = ExtractStringIncludeTag(xmlCusName, "CUSTOMER_NAME");
                     if (customer_nameFCC == "" || customer_nameFCC == null )
                    {
                        xmlCusName = "";
                        xmlCusName += "<row>";
                        xmlCusName += "<CUSTOMER_NAME>" + name_customer + "</CUSTOMER_NAME>";
                        xmlCusName += "<CUSTOMER_TYPE>NORMAL</CUSTOMER_TYPE>";
                        xmlCusName += "<CUSTOMER_SEX>" + gender + "</CUSTOMER_SEX>";
                        xmlCusName += "<CUSTOMER_PHONE>" + phoneNumber + "</CUSTOMER_PHONE>";
                        xmlCusName += "<CUSTOMER_NO>" + identifier + "</CUSTOMER_NO>";
                        xmlCusName += "<REF_CODE>00</REF_CODE>";
                        xmlCusName += "<CUS_DOI></CUS_DOI>";
                        xmlCusName += "<CUS_POI></CUS_POI>";
                        xmlCusName += "</row>";
                        code = identifier;
                    }



                    XmlDocument xmlDoc = new XmlDocument();
                    string temxml = "<root>";
                    string listBooking = "<root><O_LIST_EFORM>";
                    string xmlBookResult = "";

                    for (int i = 0; i < nodelist[0].ChildNodes.Count; i++)
                    {
                        string type_form = nodelist[0].ChildNodes[i].SelectSingleNode("FORM_TYPE").InnerText;
                        string form_status = nodelist[0].ChildNodes[i].SelectSingleNode("FORM_STATE").InnerText;
                        string formId = nodelist[0].ChildNodes[i].SelectSingleNode("TRANSID").InnerText;
                        string type_payment = nodelist[0].ChildNodes[i].SelectSingleNode("PAYMENT_TYPE").InnerText;
                        string listServiceBooked = "";
                        string linkEditInfo = nodelist[0].ChildNodes[i].SelectSingleNode("LINK_EDIT_INFO_EFORM").InnerText;
                        string formName = nodelist[0].ChildNodes[i].SelectSingleNode("FORM_NAME").InnerText;
                        string sotien = nodelist[0].ChildNodes[i].SelectSingleNode("AMOUNT").InnerText;
                        string update_date = nodelist[0].ChildNodes[i].SelectSingleNode("UPDATE_STATE_DATE").InnerText;
                        string update_by = nodelist[0].ChildNodes[i].SelectSingleNode("UPDATE_BY").InnerText;

                        listBooking += "<Item>";
                        listBooking += "<serviceName>" + formName + "</serviceName>";
                        listBooking += "<formStatus>" + form_status + "</formStatus>";
                        listBooking += "<eformId>" + formId + "</eformId>";
                        listBooking += "<linkEditInfo>" + linkEditInfo + "</linkEditInfo>";
                        listBooking += "<updateDate>" + update_date + "</updateDate>";
                        listBooking += "<updateBy>" + update_by + "</updateBy>";
                        listBooking += "</Item>";

                        if (form_status == "P")
                        {
                            is_book = true;
                            switch (type_form)
                            {
                                case "EF001":
                                    string tknop = nodelist[0].ChildNodes[i].SelectSingleNode("ACCOUNT_NO_DES").InnerText;
                                    string loaitien = nodelist[0].ChildNodes[i].SelectSingleNode("CCY").InnerText;
                                    string tennguoinhan = nodelist[0].ChildNodes[i].SelectSingleNode("NAME_ACCOUNT_DES").InnerText;
                                    string noidung = nodelist[0].ChildNodes[i].SelectSingleNode("TRANS_CONTENT").InnerText;
                                    string bankDes = nodelist[0].ChildNodes[i].SelectSingleNode("BANK_DES").InnerText;

                                    if (bankDes != null && bankDes != "")
                                    {
                                        // Nộp tiền cho khách có tài khoản ở ngân hàng khác
                                        listServiceBooked = "064";
                                    }
                                    else
                                    {
                                        // Tự nộp tiền hoặc nộp tiền cho khách có tài khoản ở TPBank
                                        listServiceBooked = "056";

                                    }
                                    temxml += "<child>";
                                    temxml += "<CIF>" + "</CIF>";
                                    temxml += "<sourceAccountNo>" + "</sourceAccountNo>";
                                    temxml += "<sourceAccountName>" + name_customer + "</sourceAccountName>";
                                    temxml += "<desAccountNo>" + tknop + "</desAccountNo>";
                                    temxml += "<desAccountName>" + tennguoinhan + "</desAccountName>";
                                    temxml += "<amount>" + sotien + "</amount>";

                                    temxml += "<desc></desc>";
                                    temxml += "<serviceCode>" + listServiceBooked + "</serviceCode>";
                                    temxml += "<custIDNo></custIDNo>";
                                    temxml += "<printingContent></printingContent>";
                                    temxml += "<currency>" + loaitien + "</currency>";
                                    temxml += "<blockAmtID></blockAmtID>";
                                    temxml += "<receiptDataTitle></receiptDataTitle>";
                                    temxml += "<receiptDataValue></receiptDataValue>";
                                    temxml += "<qrCodeName></qrCodeName>";
                                    temxml += "<bookedDevice>EBANK</bookedDevice>";
                                    temxml += "<eformId>" + formId + "</eformId>";
                                    temxml += "<linkEditEform>" + linkEditInfo + "</linkEditEform>";
                                    temxml += "</child>";

                                    break;
                                case "EF002":

                                    // Mua ngoai te

                                    listServiceBooked = "057";
                                    string loaiNgoaiTeMua = nodelist[0].ChildNodes[i].SelectSingleNode("CCY").InnerText;

                                    if (type_payment.Equals("2")) // tra qua tai khoan
                                    {
                                        listServiceBooked = "057";
                                    }
                                    else if (type_payment.Equals("1"))
                                    { // tra qua tien mat
                                        listServiceBooked = "058";
                                    }

                                    temxml += "<child>";
                                    temxml += "<CIF>" + "</CIF>";
                                    temxml += "<sourceAccountNo>" + "</sourceAccountNo>";
                                    temxml += "<sourceAccountName>" + name_customer + "</sourceAccountName>";
                                    temxml += "<desAccountNo>" + "</desAccountNo>";
                                    temxml += "<desAccountName>" + "</desAccountName>";
                                    temxml += "<amount>" + sotien + "</amount>";

                                    temxml += "<desc>" + "</desc>";
                                    temxml += "<serviceCode>" + listServiceBooked + "</serviceCode>";
                                    temxml += "<custIDNo></custIDNo>";
                                    temxml += "<printingContent></printingContent>";
                                    temxml += "<currency>" + loaiNgoaiTeMua + "</currency>";
                                    temxml += "<blockAmtID></blockAmtID>";
                                    temxml += "<receiptDataTitle>" + "</receiptDataTitle>";
                                    temxml += "<receiptDataValue>" + "</receiptDataValue>";
                                    temxml += "<qrCodeName></qrCodeName>";
                                    temxml += "<bookedDevice>EBANK</bookedDevice>";
                                    temxml += "<eformId>" + formId + "</eformId>";
                                    temxml += "<linkEditEform>" + linkEditInfo + "</linkEditEform>";
                                    temxml += "</child>";
                                    break;

                                case "EF003":

                                    listServiceBooked = "061";
                                    string loaiNgoaiTeBan = nodelist[0].ChildNodes[i].SelectSingleNode("CCY").InnerText;
                                    string currencySaleType = nodelist[0].ChildNodes[i].SelectSingleNode("CURRENCY_RECEIVE_TYPE").InnerText;
                                    /*
                                    CURRENCY_SALE_TYPE : kiểu bán ngoại tệ
                                        - Bán ngoại tệ mặt 				: 1
                                        - Bán ngoại tệ chuyển khoản		: 2

                                    PAYMENT_TYPE :	kiểu thanh toán
                                        - Thanh toán qua tiền mặt 		: 1
                                        - Thanh toán qua chuyển khoản 	: 2
                                    */
                                    // Ban ngoai te mat
                                    if (currencySaleType.Equals("1"))
                                    {
                                        // TH1 : Thanh toan bang tien mat
                                        if (type_payment.Equals("1"))
                                        {
                                            listServiceBooked = "062";
                                        }
                                        // TH2 : Thanh toan bang tai khoan
                                        else if (type_payment.Equals("2"))
                                        {
                                            listServiceBooked = "061";
                                        }
                                    }
                                    // Ban ngoai te chuyen khoan
                                    else if (currencySaleType.Equals("2"))
                                    {
                                        // TH3 : Thanh toan bang tai khoan
                                        if (type_payment.Equals("2"))
                                        {
                                            listServiceBooked = "063";
                                        }
                                    }

                                    temxml += "<child>";
                                    temxml += "<CIF>" + "</CIF>";
                                    temxml += "<sourceAccountNo>" + "</sourceAccountNo>";
                                    temxml += "<sourceAccountName>" + name_customer + "</sourceAccountName>";
                                    temxml += "<desAccountNo>" + "</desAccountNo>";
                                    temxml += "<desAccountName>" + "</desAccountName>";
                                    temxml += "<amount>" + sotien + "</amount>";

                                    temxml += "<desc>" + "</desc>";
                                    temxml += "<serviceCode>" + listServiceBooked + "</serviceCode>";
                                    temxml += "<custIDNo></custIDNo>";
                                    temxml += "<printingContent></printingContent>";
                                    temxml += "<currency>" + loaiNgoaiTeBan + "</currency>";
                                    temxml += "<blockAmtID></blockAmtID>";
                                    temxml += "<receiptDataTitle>" + "</receiptDataTitle>";
                                    temxml += "<receiptDataValue>" + "</receiptDataValue>";
                                    temxml += "<qrCodeName></qrCodeName>";
                                    temxml += "<bookedDevice>EBANK</bookedDevice>";
                                    temxml += "<eformId>" + formId + "</eformId>";
                                    temxml += "<linkEditEform>" + linkEditInfo + "</linkEditEform>";
                                    temxml += "</child>";
                                    break;

                                case "EF004": //thay doi thong tin khach hang
                                    listServiceBooked = "059";

                                    temxml += "<child>";
                                    temxml += "<CIF>" + "</CIF>";
                                    temxml += "<sourceAccountNo>" + "</sourceAccountNo>";
                                    temxml += "<sourceAccountName>" + name_customer + "</sourceAccountName>";
                                    temxml += "<desAccountNo>" + "</desAccountNo>";
                                    temxml += "<desAccountName>" + "</desAccountName>";
                                    temxml += "<amount>" + "</amount>";

                                    temxml += "<desc>" + "</desc>";
                                    temxml += "<serviceCode>" + listServiceBooked + "</serviceCode>";
                                    temxml += "<custIDNo></custIDNo>";
                                    temxml += "<printingContent></printingContent>";
                                    temxml += "<currency>" + "</currency>";
                                    temxml += "<blockAmtID></blockAmtID>";
                                    temxml += "<receiptDataTitle>" + "</receiptDataTitle>";
                                    temxml += "<receiptDataValue>" + "</receiptDataValue>";
                                    temxml += "<qrCodeName></qrCodeName>";
                                    temxml += "<bookedDevice>EBANK</bookedDevice>";
                                    temxml += "<eformId>" + formId + "</eformId>";
                                    temxml += "<linkEditEform>" + linkEditInfo + "</linkEditEform>";
                                    temxml += "</child>";
                                    break;
                                case "EF005":
                                    listServiceBooked = "060";

                                    temxml += "<child>";
                                    temxml += "<CIF>" + "</CIF>";
                                    temxml += "<sourceAccountNo>" + "</sourceAccountNo>";
                                    temxml += "<sourceAccountName>" + name_customer + "</sourceAccountName>";
                                    temxml += "<desAccountNo>" + "</desAccountNo>";
                                    temxml += "<desAccountName>" + "</desAccountName>";
                                    temxml += "<amount>" + "</amount>";

                                    temxml += "<desc>" + "</desc>";
                                    temxml += "<serviceCode>" + listServiceBooked + "</serviceCode>";
                                    temxml += "<custIDNo></custIDNo>";
                                    temxml += "<printingContent></printingContent>";
                                    temxml += "<currency>" + "</currency>";
                                    temxml += "<blockAmtID></blockAmtID>";
                                    temxml += "<receiptDataTitle>" + "</receiptDataTitle>";
                                    temxml += "<receiptDataValue>" + "</receiptDataValue>";
                                    temxml += "<qrCodeName></qrCodeName>";
                                    temxml += "<bookedDevice>EBANK</bookedDevice>";
                                    temxml += "<eformId>" + formId + "</eformId>";
                                    temxml += "<linkEditEform>" + linkEditInfo + "</linkEditEform>";
                                    temxml += "</child>";

                                    break;

                                case "EF006"://gửi tiết kiệm
                                    listServiceBooked = "066";
                                    if (type_payment.Equals("2")) // => trích nợ trả qua tài khoản
                                    {
                                        listServiceBooked = "066";
                                    }
                                    else if (type_payment.Equals("1"))// => trích nợ trả bằng tiền mặt
                                    { 
                                        listServiceBooked = "067";
                                    }

                                    temxml += "<child>";
                                    temxml += "<CIF>" + "</CIF>";
                                    temxml += "<sourceAccountNo>" + "</sourceAccountNo>";
                                    temxml += "<sourceAccountName>" + name_customer + "</sourceAccountName>";
                                    temxml += "<desAccountNo>" + "</desAccountNo>";
                                    temxml += "<desAccountName>" + "</desAccountName>";
                                    temxml += "<amount>" + "</amount>";

                                    temxml += "<desc>" + "</desc>";
                                    temxml += "<serviceCode>" + listServiceBooked + "</serviceCode>";
                                    temxml += "<custIDNo></custIDNo>";
                                    temxml += "<printingContent></printingContent>";
                                    temxml += "<currency>" + "</currency>";
                                    temxml += "<blockAmtID></blockAmtID>";
                                    temxml += "<receiptDataTitle>" + "</receiptDataTitle>";
                                    temxml += "<receiptDataValue>" + "</receiptDataValue>";
                                    temxml += "<qrCodeName></qrCodeName>";
                                    temxml += "<bookedDevice>EBANK</bookedDevice>";
                                    temxml += "<eformId>" + formId + "</eformId>";
                                    temxml += "<linkEditEform>" + linkEditInfo + "</linkEditEform>";
                                    temxml += "</child>";

                                    break;

                                default:
                                    break;
                            }

                        }
                    }
                    temxml += "</root>";
                    listBooking += "</O_LIST_EFORM></root>";
                    if (is_book)
                    {
                        xmlBookResult = BookInstantlyForEForm(code, xmlCusName, temxml, counterUsername);
                    }
                    else
                    {
                        xmlBookResult += "<RESP_CODE>2000</RESP_CODE>";
                        xmlBookResult += "<RESP_CONTENT></RESP_CONTENT>";
                        xmlBookResult += "<CUSTOMER_TRACKING_ID></CUSTOMER_TRACKING_ID>";
                    }
                    xmlDoc.LoadXml("<root>" + xmlBookResult + "</root>");
                    string customer_tracking_id = xmlDoc.GetElementsByTagName("CUSTOMER_TRACKING_ID")[0].InnerText;
                    DataSet ds = new DataSet();
                    SqlConnection con = getConnection();
                    SqlCommand cmd;
                    SqlDataAdapter da;
                    if (con.State == ConnectionState.Closed)
                    {
                        con.Open();
                    }
                    try
                    {
                        if (customer_tracking_id != "")
                        {
                            cmd = new SqlCommand("sp_GetCustomerForEForm", con);
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@CUSTOMER_TRACKING_ID", customer_tracking_id);
                            da = new SqlDataAdapter(cmd);
                            da.Fill(ds);
                            string result = addDataToPageForEForm(ds, listBooking, is_book, xmlCusName, identifier);
                            xmlResult = "<RESP_CODE>00</RESP_CODE>";
                            xmlResult += "<MESSAGE_CONTENT>" + result + "</MESSAGE_CONTENT>";
                        }
                        else
                        {

                            string result = addDataToPageForEForm(ds, listBooking, is_book, xmlCusName, identifier);
                            xmlResult = "<RESP_CODE>00</RESP_CODE>";
                            xmlResult += "<MESSAGE_CONTENT>" + result + "</MESSAGE_CONTENT>";
                        }
                    }
                    catch (Exception ex)
                    {
                        xmlResult = "<RESP_CODE>2000</RESP_CODE>";
                        xmlResult += "<MESSAGE_CONTENT>Không có dữ liệu</MESSAGE_CONTENT>";
                        if (con.State == ConnectionState.Open)
                            con.Close();
                    }
                    if (con.State == ConnectionState.Open)
                        con.Close();

                }
                else
                {
                    xmlResult = "<RESP_CODE>2001</RESP_CODE>";
                    xmlResult += "<MESSAGE_CONTENT>Không có dữ liệu</MESSAGE_CONTENT>";
                }

            }
            catch (Exception ex)
            {
                xmlResult = "<RESP_CODE>2000</RESP_CODE>";
                xmlResult += "<MESSAGE_CONTENT>Không có dữ liệu</MESSAGE_CONTENT>";
            }
            return xmlResult;
        }

        [WebMethod]
        public string UpdateStatusEForm(string formID, string formStatus, string userRequest)
        {
            string xmlResult = "";
            string result = "";
            XmlDocument xDoc = new XmlDocument();
            try
            {
                eCounterWebReference1.ECounterWebserviceVer5 webService = new eCounterWebReference1.ECounterWebserviceVer5();
                result = webService.updateStatusEForm(formID, formStatus, userRequest);
                xDoc.LoadXml("<root>" + result + "</root>");
                string respCode = xDoc.GetElementsByTagName("O_ERRCODE")[0].InnerText;

                if (respCode != "" && respCode.Equals("0"))
                {
                    xmlResult = "<RESP_CODE>00</RESP_CODE>";
                    xmlResult += "<MESSAGE_CONTENT>Cập nhật trạng thái thành công</MESSAGE_CONTENT>";
                }
                else
                {
                    xmlResult = "<RESP_CODE>" + respCode + "</RESP_CODE>";
                    xmlResult += "<MESSAGE_CONTENT>Xử lý không thành công</MESSAGE_CONTENT>";
                }

            }
            catch (Exception ex)
            {
                xmlResult = "<RESP_CODE>2000</RESP_CODE>";
                xmlResult += "<MESSAGE_CONTENT>" + ex.Message + "</MESSAGE_CONTENT>";
            }
            return xmlResult;
        }

        [WebMethod]
        public string UpdateListStatusEForm(string listFormID, string userRequest)
        {
            string xmlResult = "";
            string result = "";
            string xmlrq = "";
            XmlDocument xDoc = new XmlDocument();
            try
            {
                string[] arrformID = listFormID.Split('|');
                xmlrq += "<?xml version='1.0' ?><items>";
                for (int i = 0; i < arrformID.Length; i++)
                {
                    xmlrq += "<row>";
                    xmlrq += "<form_id>" + arrformID[i] + "</form_id>";
                    xmlrq += "<form_state>Y</form_state>";
                    xmlrq += "<user_request>" + userRequest + "</user_request>";
                    xmlrq += "<ip_request></ip_request>";
                    xmlrq += "</row>";
                }
                xmlrq += "</items>";
                eCounterWebReference1.ECounterWebserviceVer5 webService = new eCounterWebReference1.ECounterWebserviceVer5();
                result = webService.updateListStatusEForm(listFormID);
                xDoc.LoadXml("<root>" + result + "</root>");
                string respCode = xDoc.GetElementsByTagName("O_ERRCODE")[0].InnerText;

                if (respCode != "" && respCode.Equals("0"))
                {
                    xmlResult = "<RESP_CODE>00</RESP_CODE>";
                    xmlResult += "<MESSAGE_CONTENT>Cập nhật trạng thái thành công</MESSAGE_CONTENT>";
                }
                else
                {
                    xmlResult = "<RESP_CODE>" + respCode + "</RESP_CODE>";
                    xmlResult += "<MESSAGE_CONTENT>Xử lý không thành công</MESSAGE_CONTENT>";
                }

            }
            catch (Exception ex)
            {
                xmlResult = "<RESP_CODE>2000</RESP_CODE>";
                xmlResult += "<MESSAGE_CONTENT>" + ex.Message + "</MESSAGE_CONTENT>";
            }
            return xmlResult;
        }

        public string dateFormatToFCCddMMyyyy(string date)
        {
            if (date == "")
                return "";
            string[] arrDate = date.Split(' ');

            return arrDate[0].Split('-')[2] + "-" + arrDate[0].Split('-')[1] + "-" + arrDate[0].Split('-')[0];
        }

        public string dateFormatToFCCyyyyMMdd(string date)
        {
            if (date == "")
                return "";
            string[] arrDate = date.Split(' ');

            return arrDate[0].Split('-')[0] + "-" + arrDate[0].Split('-')[1] + "-" + arrDate[0].Split('-')[2];
        }

        public string formatPhone(string phone)
        {
            string result = "";
            int j = 0;
            for (int i = 0; i < phone.Length; i++)
            {
                j++;
                if (i % 3 == 0 && j < 8)
                {
                    result += " ";
                }
                result += phone[i];
            }
            return result;
        }
        public string formatCIF(string cif)
        {
            string result = "";
            for (int i = 0; i < cif.Length; i++)
            {
                if (i % 4 == 0)
                {
                    result += " ";
                }
                result += cif[i];
            }
            return result;
        }
        public string formatDateTime(string date)
        {
            string result = "";
            string[] arrdate = date.Split(' ');
            try
            {
                return arrdate[0].Split('-')[2] + "/" + arrdate[0].Split('-')[1] + "/" + arrdate[0].Split('-')[0] + " " + arrdate[1].Split('.')[0];
            }
            catch (Exception ex)
            {
                return result;
            }
        }

        // KienNT 2018/11/22
        [WebMethod]
        public string AddOrUpdateGiveGift(string currentCustTrackingID, string giftName, string giftReason)
        {
            string xml = "";
            SqlConnection con = getConnection();
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            try
            {
                SqlCommand cmd = new SqlCommand("SP_AddOrUpdateGift", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TRACKING_ID", currentCustTrackingID);
                cmd.Parameters.AddWithValue("@GIFT_NAME", giftName);
                cmd.Parameters.AddWithValue("@GIFT_REASON", giftReason);

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
                xml = "ERROR: " + ex.Message;
            }
            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
            return xml;
        }

        [WebMethod]
        public string GetGiftInfo(string currentCustTrackingID)
        {
            string xml = "";
            SqlConnection con = getConnection();
            SqlDataAdapter da;
            DataSet ds = new DataSet();
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            try
            {
                SqlCommand cmd = new SqlCommand("SP_GetGiftInfo", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TRACKING_ID", currentCustTrackingID);
                da = new SqlDataAdapter(cmd);
                da.Fill(ds);

                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    xml += "<TIME_WAIT>" + ds.Tables[0].Rows[0]["TIME_WAIT_M"].ToString() + " phút : " + ds.Tables[0].Rows[0]["TIME_WAIT_S"].ToString() + " giây</TIME_WAIT>";
                    xml += "<CUSTOMER_NAME>" + ds.Tables[0].Rows[0]["CUSTOMER_NAME"].ToString() + "</CUSTOMER_NAME>";
                    xml += "<GIFT_NAME>" + ds.Tables[0].Rows[0]["GIFT_NAME"].ToString() + "</GIFT_NAME>";
                    xml += "<GIFT_REASON>" + ds.Tables[0].Rows[0]["GIFT_REASON"].ToString() + "</GIFT_REASON>";
                }
            }
            catch (Exception ex)
            {
                xml += "<TIME_WAIT></TIME_WAIT>";
                xml += "<CUSTOMER_NAME></CUSTOMER_NAME>";
                xml += "<GIFT_NAME></GIFT_NAME>";
                xml += "<GIFT_REASON></GIFT_REASON>";
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }

            return xml;
        }
        // END KienNT 2018/11/22

        public static string ExtractStringIncludeTag(string s, string tag)
        {
            // You should check for errors in real-world code, omitted for brevity
            var startTag = "<" + tag + ">";
            var endTag = "</" + tag + ">";
            int startIndex = s.IndexOf(startTag) + startTag.Length;
            int endIndex = s.IndexOf("</" + tag + ">", startIndex);
            return s.Substring(startIndex, endIndex - startIndex);
        }

        // hungtt4.os 2019/11/05
        [WebMethod]
        public string InsertMonitorInfo(List<MonitorInfo> lstMonitorInfo)
        {
            List<string> listIdReturn = new List<string>();
            SqlConnection con = getConnection();
            DataSet ds = new DataSet();
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            try
            {
                foreach (MonitorInfo monitorInfo in lstMonitorInfo)
                {
                    SqlCommand cmd = new SqlCommand("SP_INSERT_MONITOR_INFO", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BRANCH_CODE", BRANCH_CODE);
                    cmd.Parameters.AddWithValue("@BRANCH_NAME", BRANCH_NAME);
                    cmd.Parameters.AddWithValue("@REQUESTED_ON", ParseStringToDate(monitorInfo.requestedOn));
                    cmd.Parameters.AddWithValue("@RESPONSED_ON", ParseStringToDate(monitorInfo.responsedOn));
                    cmd.Parameters.AddWithValue("@MONITOR_TYPE", monitorInfo.monitorType);
                    cmd.Parameters.AddWithValue("@SOURCE", monitorInfo.source);
                    cmd.Parameters.AddWithValue("@TARGET", monitorInfo.target);
                    cmd.Parameters.AddWithValue("@STATUS", monitorInfo.status);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(ds);
                    if (ds.Tables.Count > 0)
                    {
                        listIdReturn.Add(ds.Tables[0].Rows[0][0].ToString());
                    }
                    ds.Clear();
                }                
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
            //return listIdReturn;
            if (listIdReturn.Count == lstMonitorInfo.Count)
            {
                return "SUCCESS";
            }
            else
            {
                Utils.Log.WriteLog("Insert MonitorInfo to DB fail!!!!");
                for (int i = listIdReturn.Count; i < lstMonitorInfo.Count; i++)
                {
                    Utils.Log.WriteLog(lstMonitorInfo.ElementAt(i).ToString());
                }
                    return "FAIL";
            }
        }

        private DateTime ParseStringToDate(string dateTimeStr)
        {
            CultureInfo provider = CultureInfo.InvariantCulture;
            try
            {
                return String.IsNullOrEmpty(dateTimeStr) ? DateTime.Now : DateTime.ParseExact(dateTimeStr, DATE_TIME_FORMAT, provider);
            }
            catch (Exception e)
            {
                Utils.Log.WriteLog("Error parse date time: " + e.Message);
                return DateTime.Now;
            }
        }
        // end hungtt4.os
    }
}
