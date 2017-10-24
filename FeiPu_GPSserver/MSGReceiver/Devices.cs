using CMSGReFeiPu_GPSserver.MSGReceiver;
using DbComponent;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace FeiPu_GPSserver.MSGReceiver
{
    class Devices
    {

  
        // TaskScheduler _ts = TaskScheduler.FromCurrentSynchronizationContext();

        private int _msgSdk = 0;
        private log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
      


        public  void GetDeviceFormWebservice()
        {
            
            string username = System.Configuration.ConfigurationSettings.AppSettings["PlatLoginName"].ToString();
            string PlatLoginPWD = System.Configuration.ConfigurationSettings.AppSettings["PlatLoginPWD"].ToString();
            string PlatIP = System.Configuration.ConfigurationSettings.AppSettings["PlatIP"].ToString();

            Getresouce.VmsSdkWebServicePortTypeClient gt = new Getresouce.VmsSdkWebServicePortTypeClient("VmsSdkWebServiceHttpSoap11Endpoint");
            string xml = gt.sdkLogin(username, SHA256Encrypt(PlatLoginPWD), PlatIP, "", "");
            ServiceResult sr = ServiceResult.Parse(xml);


            string tokenxml = gt.applyToken(sr.Rows[0]["tgt"]);
            ServiceResult token = ServiceResult.Parse(tokenxml);

            string getdevicepoint = gt.getResourceByPage(token.Rows[0]["st"], 10000, 1, 200, "", "", 1);//获取监控点资源
            string getdevice = gt.getResourceByPage(token.Rows[0]["st"], 30000, 1, 200, "", "", 1);//获取设备资源，1个单兵设备有两个监控点

            ServiceResult rsdevice = ServiceResult.Parse(getdevice);

            ServiceResult rspoint = ServiceResult.Parse(getdevicepoint);
            string strsql = System.Configuration.ConfigurationSettings.AppSettings["m_connectionString"].ToString();//数据库链接字符串
       
            SqlConnection conStr = new SqlConnection(strsql);//SQL数据库连接对象，以数据库链接字符串为参数
            conStr.Open();//打开数据库连接

            SQLHelper.ExecuteNonQuery(CommandType.Text, "UPDATE [Device_Info] SET [c_enable] =0");
            

            ServiceResult rs = new ServiceResult();

            for (int i = 0; i < rspoint.Rows.Count; i++)
            {
                Dictionary<string, string> rowItem = new Dictionary<string, string>();
                rowItem["c_name"] = rspoint.Rows[i]["c_name"];
                rowItem["c_device_index_code"] = rspoint.Rows[i]["c_device_index_code"];
                rowItem["i_id"] = rspoint.Rows[i]["i_id"];
                rowItem["c_org_name"] = rspoint.Rows[i]["c_org_name"];
                rowItem["i_status"] = rspoint.Rows[i]["i_status"];
                rowItem["c_index_code"] = rspoint.Rows[i]["c_index_code"];
                rowItem["c_cascade_code"] = rspoint.Rows[i]["c_cascade_code"];
                rowItem["i_domain_id"] = rspoint.Rows[i]["i_domain_id"];
                rowItem["c_device_ip"] = rspoint.Rows[i]["c_device_ip"];
                rowItem["i_device_port"] = rspoint.Rows[i]["i_device_port"];
                rowItem["c_user_name"] = "";
                rowItem["c_user_pwd"] = "";
                for (int ide = 0; ide < rsdevice.Rows.Count; i++)
                {
                    if (rspoint.Rows[i]["c_device_ip"] == rsdevice.Rows[ide]["c_device_ip"])
                    {
                        rowItem["c_user_name"] = rsdevice.Rows[ide]["c_user_name"];
                        rowItem["c_user_pwd"] = rsdevice.Rows[ide]["c_user_pwd"];
                    }

                }
                rs.Rows.Add(rowItem);
            }
         

            for (int i = 0; i < rs.Rows.Count; i++)
            {
              
                if (rs.Rows[i]["c_org_name"] == "null") { continue; };
                string sql = "Device_INSERT";//要调用的存储过程名
                SqlCommand comStr = new SqlCommand(sql, conStr);//SQL语句执行对象，第一个参数是要执行的语句，第二个是数据库连接对象
                comStr.CommandType = CommandType.StoredProcedure;//因为要使用的是存储过程，所以设置执行类型为存储过程
                //依次设定存储过程的参数
                comStr.Parameters.Add("@c_name", SqlDbType.VarChar, 30).Value = rs.Rows[i]["c_name"];
                comStr.Parameters.Add("@deviceIndexcode", SqlDbType.VarChar, 32).Value = rs.Rows[i]["c_device_index_code"];
                comStr.Parameters.Add("@elementId", SqlDbType.Int).Value = rs.Rows[i]["i_id"];
                comStr.Parameters.Add("@c_org_name", SqlDbType.VarChar, 30).Value = rs.Rows[i]["c_org_name"];
                comStr.Parameters.Add("@i_status", SqlDbType.VarChar,2).Value = rs.Rows[i]["i_status"];
                comStr.Parameters.Add("@c_index_code", SqlDbType.VarChar, 12).Value = rs.Rows[i]["c_index_code"];
                comStr.Parameters.Add("@c_cascade_code", SqlDbType.VarChar, 12).Value = rs.Rows[i]["c_cascade_code"];
                comStr.Parameters.Add("@i_domain_id", SqlDbType.VarChar, 12).Value = rs.Rows[i]["i_domain_id"];
                comStr.Parameters.Add("@c_device_ip", SqlDbType.VarChar, 20).Value = rs.Rows[i]["c_device_ip"];
                comStr.Parameters.Add("@i_device_port", SqlDbType.VarChar, 6).Value = rs.Rows[i]["i_device_port"];
                comStr.Parameters.Add("@c_user_name", SqlDbType.VarChar, 10).Value = rs.Rows[i]["c_user_name"];
                comStr.Parameters.Add("@c_user_pwd", SqlDbType.VarChar, 10).Value = rs.Rows[i]["c_user_pwd"];

                comStr.ExecuteNonQuery().ToString();

                comStr = null;

            }
            conStr.Close();//关闭连接

        }

        public static string SHA256Encrypt(string str)//256加密
        {
            byte[] SHA256Data = Encoding.UTF8.GetBytes(str);
            System.Security.Cryptography.SHA256 Sha256 = new System.Security.Cryptography.SHA256Managed();
            byte[] by = Sha256.ComputeHash(SHA256Data);
            return BitConverter.ToString(by).Replace("-", "").ToLower();
        }
    }
}
