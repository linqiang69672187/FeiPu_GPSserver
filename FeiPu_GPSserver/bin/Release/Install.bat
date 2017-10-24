%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\installutil.exe FeiPu_GPSserver.exe
Net Start FeipuGPS_server
sc config FeipuGPS_server start= auto