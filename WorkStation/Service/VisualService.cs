using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkStation
{
    /// <summary>
    /// 视觉服务类
    /// </summary>
    public class VisualService
    {
        public enum CommandCode
        {
            GetCameraCoords = 0x40,
            GetVisualState,
            CameraTotal
        }

        private static VisualService m_UniqueVisualService = null;
        private static readonly object m_Locker = new object();
        public MyTcpClient m_TcpClient = new MyTcpClient();
        private byte[] m_SendMeas = new byte[CommunicationProtocol.MessageLength];

        private VisualService()
        {

        }

        public bool IsVisualConnected
        {
            get
            {
                if (m_TcpClient != null)
                    return m_TcpClient.IsConnected;
                else
                    return false;
            }
        }

        public static VisualService GetInstance()
        {
            if (m_UniqueVisualService == null)
            {
                lock (m_Locker)
                {
                    if (m_UniqueVisualService == null)
                        m_UniqueVisualService = new VisualService();
                }
            }
            return m_UniqueVisualService;
        }

        //发送指令

        //处理视觉消息
        public void ProcessVisualMessage(TcpMeas meassage)
        {
            if (meassage != null)
            {
                CommandCode Code = (CommandCode)meassage.MeasCode;
                switch (Code)
                {
                    case CommandCode.GetVisualState:
                        {

                        }
                        break;
                   
                    default:
                        break;
                }
            }
        }
    }
}
