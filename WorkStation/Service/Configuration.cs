using System;
using System.Xml.Serialization;
using System.IO;

namespace WorkStation
{
    /// <summary>
    /// 配置类
    /// </summary>
    
    public class Configuration
    {       
        public string StationClientIp = "192.168.1.20";
        public string StationServerIp = "192.168.1.20";
        public int StationServerPort = 20000;

        public string Board_A_Ip = "192.168.1.10";
        public int Board_A_Port = 20001;

        public AxisType Conveyor_1 = new AxisType(0, 1);

        //输入IO
        public IoInType IO_IN_KeyRun = new IoInType(0, 1);
        public IoInType IO_IN_KeyPause = new IoInType(0, 2);
        public IoInType IO_IN_KeyStop = new IoInType(0, 3);
        public IoInType IO_IN_KeyReset = new IoInType(0, 4);
        public IoInType IO_IN_KeyScramSignal = new IoInType(0, 5);  

        //输出IO
        public IoOutType IO_OUT_LedKeyRun = new IoOutType(0, 1);
        public IoOutType IO_OUT_LedKeyPause = new IoOutType(0, 2);
        public IoOutType IO_OUT_LedKeyStop = new IoOutType(0, 3);

        public IoOutType IO_OUT_LedRed = new IoOutType(0, 4);
        public IoOutType IO_OUT_LedOriange = new IoOutType(0, 5);
        public IoOutType IO_OUT_LedGreen = new IoOutType(0, 6);
        public IoOutType IO_OUT_Beep = new IoOutType(0, 7);

        public Configuration()
        {
          
        }
    }

    //配置文件
    public static class Profile
    {
        private static readonly string m_FileName = "Config.xml";  //配置文件名
        private static readonly string m_FileNameBackup = "ConfigBackup.xml";  //配置文件名
        public static Configuration m_Config = new Configuration();

        public static bool LoadConfigFile()
        {
            string strFile = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + m_FileName;
            if (!File.Exists(strFile))
                return false;           

            using (FileStream fStream = new FileStream(strFile, FileMode.Open))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Configuration));
                try
                {
                    m_Config = xmlSerializer.Deserialize(fStream) as Configuration;
                    return true;
                }
                catch //(InvalidOperationException)
                {
                    return false;
                }
            }
        }

        public static void SaveConfigFile()
        {
            string strFile = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + m_FileName;
            if (File.Exists(strFile))  //先把原来的配置文件备份
            {
                string strFileBackUp = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + m_FileNameBackup;
                if (File.Exists(strFileBackUp))
                    File.Delete(strFileBackUp);
					
               File.Copy(strFile, strFileBackUp);
            }

            using (FileStream fStream = new FileStream(strFile, FileMode.Create))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Configuration));
                try
                {
                    xmlSerializer.Serialize(fStream, m_Config);
                }
                catch
                {
                    
                }
            }
        }
    }
}
