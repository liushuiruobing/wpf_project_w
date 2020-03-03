using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace WorkStation
{
    public enum TcpMeasType
    {
        MEAS_TYPE_NONE = 0,
        MEAS_TYPE_MES,
        MEAS_TYPE_PLC,
        MEAS_TYPE_CONTROLER_BOARD,
        MEAS_TYPE_VISUAL_SERVICE,
    }

    public enum TcpMeasCode
    {
        MEAS_CODE_NONE = 0,
        //...........命令码
    }

    //Tcp Meassage Class
    public class TcpMeas
    {
        public TcpClient Client;
        public TcpMeasType MeasType;
        public byte MeasCode;
        public byte[] Param = new byte[CommunicationProtocol.MessageLength];

        public TcpMeas()
        {
            Array.Clear(Param, 0, Param.Length);
        }
    }

    //Tcp Client Class
    public partial class MyTcpClient
    {
        private TcpClient m_TcpClient = null;    
        public ConcurrentQueue<TcpMeas> m_RecvMeasQueue = new ConcurrentQueue<TcpMeas>();
        private Byte[] m_RecvBytes = new Byte[8192];

        public ConcurrentQueue<byte[]> m_SetCommandQueue = new ConcurrentQueue<byte[]>();
        public ConcurrentQueue<byte[]> m_GetCommandQueue = new ConcurrentQueue<byte[]>();

        private CancellationTokenSource m_RecvTaskCancelTokenSource = new CancellationTokenSource();
        private CancellationToken m_RecvTaskCancelToken;
        private Task RecvTask = null;

        public MyTcpClient()
        {
            m_TcpClient = new TcpClient();
            m_RecvTaskCancelToken = m_RecvTaskCancelTokenSource.Token;
        }

        public bool IsConnected
       {
            get
            {
                if (m_TcpClient != null)
                    return m_TcpClient.Connected;
                else
                    return false;
            }
       }

        public void InitClient()
        {
            //IPEndPoint EndPoint = new IPEndPoint(m_TcpParam.nIpAddress, m_TcpParam.nPort);
            //m_TcpClient = new TcpClient(EndPoint);
          
            //m_TcpClient.ReceiveTimeout = RecvTimeOut;
            //m_TcpClient.SendTimeout = SendTimeOut;
        }

        public void CreateConnect(IPAddress nIpAddress, int nPort)
        {
            if (m_TcpClient != null)
            {
                try
                {
                    m_TcpClient.Connect(nIpAddress, nPort);
                    if (m_TcpClient.Connected)
                    {
                        RecvTask = new Task(TcpClientRecvTask, m_TcpClient, m_RecvTaskCancelToken);
                        RecvTask.Start();
                    }
                }
                catch (System.Exception ex)
                {
                    Global.m_Log.Warn(ex.Message);
                }
            }
        }

        public void Close()
        {
            if (m_TcpClient != null)
            {
                try
                {
                    m_RecvTaskCancelTokenSource.Cancel();
                    int Count = 200;
                    while (Count-- > 0)
                    {
                        if(RecvTask.Status == TaskStatus.RanToCompletion || RecvTask.Status == TaskStatus.Canceled)
                            break;

                        Thread.Sleep(10);
                    }
                    m_TcpClient.Close();
                }
                catch (System.Exception ex)
                {
                    Global.m_Log.Warn("22222: " + ex.Message);
                }
            }
        }

        private void TcpClientRecvTask(object Client)
        {           
            int RecvCount = 0;
            int parseCount = 0;  //解析计数器
            byte[] arrayParse = new byte[CommunicationProtocol.MessageLength];  //解析缓冲区

            while (Client != null && ((TcpClient)Client).Connected)
            {
                if (m_RecvTaskCancelToken.IsCancellationRequested)
                    break;

                try
                {
                    NetworkStream stream = ((TcpClient)Client).GetStream();
                    while (stream != null && stream.DataAvailable && (RecvCount = stream.Read(m_RecvBytes, 0, m_RecvBytes.Length)) != 0)
                    {
                        if(m_RecvTaskCancelToken.IsCancellationRequested)
                            break; 

                        //主线程创建消息处理的线程处理消息
                        ParseAndAddMessageToQueue(m_RecvBytes, RecvCount, m_TcpClient, ref parseCount, arrayParse);
                    }
                }
                catch (Exception ex)
                {
                    Global.m_Log.Warn("33333: " + ex.Message);
                    break;
                }
                Thread.Sleep(1);
            }
        }

        public void ParseAndAddMessageToQueue(byte[] RecvBytes, int RecvCount, TcpClient Client, ref int parseCount, byte[] arrayParse)
        {
            //匹配比较数组, -1表示不需要比较,忽略
            int[] arrayCompare = new int[CommunicationProtocol.MessageLength]
            {
                CommunicationProtocol.MessStartCode, CommunicationProtocol.MessVID1, CommunicationProtocol.MessVID2, -1, CommunicationProtocol.MessRightState, -1, -1, -1,
                -1, -1, -1, -1, -1, -1, -1, -1,
                -1, -1, -1, -1, -1, -1, -1, -1,
                -1, -1, -1, -1, -1, -1, -1, CommunicationProtocol.MessEndCode,
            };

            for (int i = 0; i < RecvCount; i++)
            {
                if (arrayCompare[parseCount] != -1)  //需要匹配
                {
                    if (RecvBytes[i] == arrayCompare[parseCount])  //相等
                    {
                        arrayParse[parseCount++] = RecvBytes[i];
                    }
                    else  //不相等,复位计数器
                    {
                        parseCount = 0;
                    }
                }
                else  //不需要比较,直接赋值,并更新计数器
                {
                    arrayParse[parseCount++] = RecvBytes[i];
                }

                if (parseCount >= CommunicationProtocol.MessageLength)
                {
                    parseCount = 0; //和校验

                    if (MyMath.CheckSum(arrayParse, CommunicationProtocol.MessageLength))  //分析数据，把数据添加到队列m_TcpMeas
                    {
                        TcpMeasType MeasType = TcpMeasType.MEAS_TYPE_CONTROLER_BOARD;
                        byte MeasCode = arrayParse[CommunicationProtocol.MessageCommandIndex];
                        TcpMeas TempMeas = new TcpMeas();
                        if (TempMeas != null)
                        {
                            TempMeas.Client = Client;
                            TempMeas.MeasType = MeasType;
                            TempMeas.MeasCode = MeasCode;
                            Array.Copy(arrayParse, TempMeas.Param, TempMeas.Param.Length);
                            m_RecvMeasQueue.Enqueue(TempMeas);
                        }

                        Array.Clear(arrayParse, 0, arrayParse.Length);
                    }
                    else  //校验和错误,则更新错误码后发回
                    {
                        arrayParse[CommunicationProtocol.MessageStateIndex] = CommunicationProtocol.MessErrorState;
                        arrayParse[CommunicationProtocol.MessageSumCheck] = 0x00;
                        arrayParse[CommunicationProtocol.MessageSumCheck] = MyMath.CalculateSum(arrayParse, CommunicationProtocol.MessageLength);

                        Client.GetStream().Write(arrayParse, 0, arrayParse.Length);
                    }
                }
            }
        }

        public async void ClientWrite(byte[] WriteBytes)
        {
            if (m_TcpClient.Connected)
            {
                try
                {
                    NetworkStream stream = m_TcpClient.GetStream();
                    await stream.WriteAsync(WriteBytes, 0, WriteBytes.Length);
                    await stream.FlushAsync();
                }
                catch (System.Exception ex)
                {
                    //异常发生后应该结束 await的任务，避免任务一直阻塞而导致异常不断抛出
                    //Global.DebugMessageBoxShow("44444: " + ex.Message);
                    Debug.WriteLine("44444: " + ex.Message);
                }
            }
        }

        public async void ClientWrite(string StrSend)
        {
            if (m_TcpClient.Connected)
            {
                try
                {
                    //NetworkStream stream = m_TcpClient.GetStream();
                    //StreamWriter s = new StreamWriter(stream);
                    //await s.WriteAsync(StrSend);
                    //await s.FlushAsync();

                    NetworkStream stream = m_TcpClient.GetStream();
                    using (var writer = new StreamWriter(stream, Encoding.ASCII, StrSend.Length, leaveOpen: true))
                    {
                        writer.AutoFlush = true;
                        await writer.WriteAsync(StrSend);
                    }
                }
                catch (System.Exception ex)
                {
                    Global.m_Log.Warn("55555: " + ex.Message);
                    return;
                }
            }
        }

    }

    //Tcp Server Class
    public partial class MyTcpServer
    {
        private static MyTcpServer m_UniqueTcpServer = null;
        private static readonly object m_Locker = new object();
        private TcpListener m_TcpListener = null;
        public ConcurrentQueue<TcpMeas> m_RecvMeasQueue = new ConcurrentQueue<TcpMeas>();
        private Byte[] m_RecvBytes = new Byte[8192];

        private Thread m_TcpServerListenThread = null;
        private volatile bool m_ShouldExit = false;

        private MyTcpServer()
        {

        }

        /// <summary>
        /// 定义公有方法提供一个全局访问点,同时你也可以定义公有属性来提供全局访问点
        /// </summary>
        /// <returns></returns>
        public static MyTcpServer GetInstance()
        {
            if (m_UniqueTcpServer == null)
            {
                lock (m_Locker)
                {
                    if (m_UniqueTcpServer == null)
                        m_UniqueTcpServer = new MyTcpServer();
                }
            }

            return m_UniqueTcpServer;
        }

        public bool CreatServer(IPAddress ServerIp, int ServerPort)
        {
            try
            {
                //m_TcpListener = new TcpListener(ServerIp, ServerPort);
                m_TcpListener = new TcpListener(IPAddress.Any, ServerPort);
                m_TcpListener.Start();

                m_TcpServerListenThread = new Thread(new ThreadStart(TcpListenThread));
                m_TcpServerListenThread.Start();

                if (m_TcpListener != null && m_TcpServerListenThread != null)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                Global.m_Log.Warn("66666: " + ex.Message);
                return false;
            }
        }

        public void CloseServer()
        {
            if (m_TcpListener != null)
            {
                try
                {
                    m_ShouldExit = true;

                    int Count = 20;
                    while (Count-- > 0)
                    {
                        Thread.Sleep(10);
                    }
                    m_TcpListener.Stop();
                }
                catch (System.Exception ex)
                {
                    Global.m_Log.Warn("999999: " + ex.Message);
                }
            }
        }

        private void TcpListenThread()
        {
            while (true)
            {
                if (m_ShouldExit)
                    break;

                try
                {
                    while (m_TcpListener != null && m_TcpListener.Pending())  
                    {
                        if (m_ShouldExit)
                            break;

                        Task RecvTask = new Task(TcpListenRecvTask, m_TcpListener.AcceptTcpClient());
                        RecvTask.Start();
                    }
                }
                catch (SocketException ex)
                {
                    Global.m_Log.Warn("77777: " + ex.Message);
                    break;
                }

                Thread.Sleep(1); //释放CPU
            }
        }

        private void TcpListenRecvTask(object Client)
        {
            int RecvCount = 0;
            int parseCount = 0;  //解析计数器
            byte[] arrayParse = new byte[CommunicationProtocol.MessageLength];  //解析缓冲区

            while (Client != null && ((TcpClient)Client).Connected)
            {
                if (m_ShouldExit)
                    break;

                try
                {
                    NetworkStream stream = ((TcpClient)Client).GetStream();
                    while (stream != null && stream.DataAvailable && (RecvCount = stream.Read(m_RecvBytes, 0, m_RecvBytes.Length)) != 0)
                    {
                        if (m_ShouldExit)
                            break;

                        ParseAndAddMessageToQueue(m_RecvBytes, RecvCount, (TcpClient)Client, ref parseCount, arrayParse);
                    }
                }
                catch (Exception ex)
                {
                    Global.m_Log.Warn("88888: " + ex.Message);
                    break;
                }
                Thread.Sleep(1);
            }
        }

        /// <summary>
        /// 解析数据,收到一包有效数据后,添加到消息队列
        /// </summary>
        /// <param name="recvBytes">接收到的数据</param>
        /// <param name="recvCount">接收到的数据长度</param>
        /// <param name="client">客户端</param>
        /// <param name="parseCount">解析计数器</param>
        /// <param name="arrayParse">解析缓冲区</param>
        public void ParseAndAddMessageToQueue(byte[] RecvBytes, int RecvCount, TcpClient Client, ref int parseCount, byte[] arrayParse)
        {
            //匹配比较数组, -1表示不需要比较,忽略
            int[] arrayCompare = new int[CommunicationProtocol.MessageLength]
            {
                CommunicationProtocol.MessStartCode, CommunicationProtocol.MessVID1, CommunicationProtocol.MessVID2, -1, CommunicationProtocol.MessRightState, -1, -1, -1,
                -1, -1, -1, -1, -1, -1, -1, -1,
                -1, -1, -1, -1, -1, -1, -1, -1,
                -1, -1, -1, -1, -1, -1, -1, CommunicationProtocol.MessEndCode,
            };

            for (int i = 0; i < RecvCount; i++)
            {
                if (arrayCompare[parseCount] != -1)  //需要匹配
                {
                    if (RecvBytes[i] == arrayCompare[parseCount])  //相等
                    {
                        arrayParse[parseCount++] = RecvBytes[i];
                    }
                    else  //不相等,复位计数器
                    {
                        parseCount = 0;
                    }
                }
                else  //不需要比较,直接赋值,并更新计数器
                {
                    arrayParse[parseCount++] = RecvBytes[i];
                }

                if (parseCount >= CommunicationProtocol.MessageLength)
                {                   
                    parseCount = 0; //和校验

                    if (MyMath.CheckSum(arrayParse, CommunicationProtocol.MessageLength))  //分析数据，把数据添加到队列m_TcpMeas
                    {
                        TcpMeasType MeasType = TcpMeasType.MEAS_TYPE_NONE;
                        byte MeasCode = 0;

                        if (arrayParse[CommunicationProtocol.MessageCommandIndex] == (byte)CommunicationProtocol.CommandCode_PLC.GetCurStationState)  //根据命令码区分消息类型
                        {
                            MeasType = TcpMeasType.MEAS_TYPE_PLC;
                            MeasCode = arrayParse[CommunicationProtocol.MessageCommandIndex];
                        }

                        TcpMeas TempMeas = new TcpMeas();
                        if (TempMeas != null)
                        {
                            TempMeas.Client = Client;
                            TempMeas.MeasType = MeasType;
                            TempMeas.MeasCode = MeasCode;
                            Array.Copy(arrayParse, TempMeas.Param, TempMeas.Param.Length);                                           
                            m_RecvMeasQueue.Enqueue(TempMeas);
                        }
                    }
                    else  //校验和错误,则更新错误码后发回
                    {
                        arrayParse[CommunicationProtocol.MessageStateIndex] = CommunicationProtocol.MessErrorState;
                        arrayParse[CommunicationProtocol.MessageSumCheck] = 0x00;
                        arrayParse[CommunicationProtocol.MessageSumCheck] = MyMath.CalculateSum(arrayParse, CommunicationProtocol.MessageLength);

                        Client.GetStream().Write(arrayParse, 0, arrayParse.Length);
                    }
                }
            }
        }
    }
}
