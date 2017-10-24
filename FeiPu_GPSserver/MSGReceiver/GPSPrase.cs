using FeiPu_GPSserver.MSGReceiver;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace CMSGReFeiPu_GPSserver.MSGReceiver
{
    class GPSPrase
    {
        private log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public void AddGpsInfo(GPSInfo info)
        {
           
                //    info.dataTime,info.devisionEw,info.devisionNs,info.longitude.ToString("0.00"),
                 //   info.latitude.ToString("0.00"),info.direction.ToString("0.00"), info.speed.ToString("0.00"), info.sateNum.ToString()}, -1);
            try
            {
                SqlParameter[] sp = new SqlParameter[5];
                sp[0] = new SqlParameter("@deviceIndexcode", info.deviceIndexcode);
                sp[1] = new SqlParameter("@elementId", info.elementId);
                sp[2] = new SqlParameter("@latitude", info.longitude);
                sp[3] = new SqlParameter("@longitude", info.latitude);
                sp[4] = new SqlParameter("@sdataTime", info.dataTime);

                DbComponent.SQLHelper.ExecuteNonQuery(CommandType.StoredProcedure, "GPS_INSERT", sp);
            }
            catch (Exception ex) {
                log.Error(ex.ToString());
            }
            finally {
              
            }
        }

        public void UpdateStatus()
        {
            Timer tm = new Timer(int.Parse(System.Configuration.ConfigurationSettings.AppSettings["setGPSovertime"])*60*1000);
            tm.Enabled = true;
            tm.Elapsed += new ElapsedEventHandler(tmr_Elapsed);
            tm.AutoReset = true;
            tm.Start();

        }
        public void tmr_Elapsed(object sender, EventArgs e)
        {
            StringBuilder sbSQL = new StringBuilder("update de   set de.i_status=0 from Device_Info de LEFT  JOIN  GPS_Info GPS  on de.deviceIndexcode=GPS.deviceIndexcode where datediff(minute, GPS.cdataTime, getdate())>" + int.Parse(System.Configuration.ConfigurationSettings.AppSettings["setGPSovertime"]));
            try
            {
                DbComponent.SQLHelper.ExecuteNonQuery(CommandType.Text, sbSQL.ToString());
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
        }

    }
}
