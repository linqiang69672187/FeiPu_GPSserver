using CMSGReFeiPu_GPSserver.MSGReceiver;
using FeiPu_GPSserver.MSGReceiver;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using log4net;
using System.Timers;
using System.Threading;

namespace FeiPu_GPSserver
{
    public partial class Service1 : ServiceBase
    {
        private GPSInfoCallback _gpsCB;
        private NmsMsgCallback _nmsCB;
       // TaskScheduler _ts = TaskScheduler.FromCurrentSynchronizationContext();
        GPSPrase _gpsprase = new GPSPrase();
        private int _msgSdk = 0;
        private  log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private Devices de = new Devices();
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {

            _gpsCB = new GPSInfoCallback(OnGPSInfo);
            _nmsCB = new NmsMsgCallback(OnCamStatInfo);
             NativeMethods.Init();
             MSGSDKhandle(false);
             de.GetDeviceFormWebservice();//获取及更新设备资源  
            // _gpsprase.UpdateStatus();//通过GPS时间更新监控在线状态
             //半小时重新注册平台，并更新获取资源
            //ReRegAndUpDevice();
        }

        public void ReRegAndUpDevice()  //半小时重新注册平台，并更新获取资源
        {
            System.Timers.Timer tm = new System.Timers.Timer(30 * 60 * 1000);
            tm.Enabled = true;
            tm.Elapsed += new ElapsedEventHandler(tmr_Elapsed);
            tm.AutoReset = true;
            tm.Start();

        }

        public void tmr_Elapsed(object sender, EventArgs e)
        {
            NativeMethods.UnInit();
            Thread.Sleep(50);
            NativeMethods.Init();
            MSGSDKhandle(true);
            de.GetDeviceFormWebservice();
        }

        protected override void OnStop()
        {
        }
        protected int OnGPSInfo(ref GPSInfo gpsInfo, IntPtr userData)//GPS实时数据回调
        {
            Task.Factory.StartNew((info) =>
            {
                _gpsprase.AddGpsInfo((GPSInfo)info);
            }, gpsInfo);

        

            return 0;
        }

        protected int OnCamStatInfo(ref NmsMsgInfo nmsInfo, IntPtr userData)//监控点状态回调函数
        {
            Task.Factory.StartNew((info) =>
            {
                this.UpdateCamState(0, (NmsMsgInfo)info);
            }, nmsInfo);
            return 0;
        }


        protected void UpdateCamState(int user, NmsMsgInfo nmsInfo)
        {
         StringBuilder sbSQL= new StringBuilder("") ;
            switch (nmsInfo.alarmstate.ToString() + nmsInfo.alarmtype)
            {
                case "266049":  //停止告警，也就是在线

                 sbSQL.Append("update set i_status=1 from Device_Info  where [deviceIndexcode] ='" + nmsInfo.resindexcode + "'");
        

                    break;
                case "166048":  //开始告警了，也就是不在线
                case "166049":
                    sbSQL.Append("update set i_status=0 from Device_Info  where [deviceIndexcode] ='" + nmsInfo.resindexcode + "'");
                    break;
              
                   
                   
              }
            if (sbSQL.ToString() == "") { return; }

            try
            {
                DbComponent.SQLHelper.ExecuteNonQuery(CommandType.Text, sbSQL.ToString());
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
            
        }

        protected  void MSGSDKhandle(Boolean isTimer)
        {
            string PlatIP = System.Configuration.ConfigurationSettings.AppSettings["PlatIP"].ToString();
            _msgSdk = NativeMethods.GetHandle("iVMS-9800", PlatIP);
            if (_msgSdk != 0)
               log.Info("获取消息SDK句柄 PlatformMsgReceiver!GetHandle 成功:句柄"+_msgSdk );
            else
                log.Info("获取消息SDK句柄 PlatformMsgReceiver!GetHandle 失败:句柄"+ _msgSdk);

            if (_msgSdk != 0 && !isTimer)
            {
                // int ret = NativeMethods.SetAlarmInfoCallback(_msgSdk, _alarmCB, IntPtr.Zero, Program.LoginInfo.UserID);
                //Trace.TraceInformation("设置报警接收回调 PlatformMsgReceiver!SetAlarmInfoCallback {0}({1})", ret == 0 ? "成功" : "失败", ret);

                // ret = NativeMethods.SetEnvInfoCallback(_msgSdk, _envCB, IntPtr.Zero);
                // Trace.TraceInformation("设置实时值接收回调 PlatformMsgReceiver!SetEnvInfoCallback {0}({1})", ret == 0 ? "成功" : "失败", ret);
                
                int ret = NativeMethods.SetGPSInfoCallback(_msgSdk, _gpsCB, IntPtr.Zero);
                  log.Info("GPS接收回调(0成功|1失败)"+ret);

                 ret = NativeMethods.SetNmsMsgCallback(_msgSdk, _nmsCB, IntPtr.Zero);
                 log.Info("NMS接收回调(0成功|1失败)" + ret);
            }
        }
    }
}
