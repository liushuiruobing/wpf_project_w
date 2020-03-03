using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WorkStation
{
    public enum ControlerCommandCode : byte  
    {
        SetOutput = 0x10,               //设置输出口
        GetOutput,                      //读取输出口缓冲区数据
        SetOutputDefault,               //设置输出口开机默认状态（需要存储到SPI-Flash）
        SetOutputByInput = 0x15,        //根据输入口状态设置输出口
        GetInput = 0x18,                //读取输入口

        GetAxisParameters = 0x20,       //读取电机轴运动参数
        SetAxisParametersDefault,       //设置电机轴默认运动参数（需要存储到SPI-Flash）
        SetAxisParameters,              //设置电机轴当前运动参数

        GetAxisStepsAbs = 0x28,         //读取电机轴当前步数
        SetAxisStepsAbs,                //设置电机轴步数(绝对值)
        SetAxisStepsRef,                //设置电机轴步数(相对值)
        SetAxisMoveContinuous,          //设置电机连续运动
        SetAxisStepsMax,                //设置电机轴最大步数
        StopAxis,                       //停止电机轴

        GetAxisState = 0x30,            //读取电机轴状态
        ResetAxisError,                 //复位电机轴错误状态

        AxisGoHome = 0x38,             //设置电机轴回原点

        SetIp = 0x40,                   //设置板卡IP地址（需要存储到SPI-Flash）
        SetVersionHardware,            //设置板卡硬件版本（需要存储到SPI-Flash）

        ResetFactory = 0x48,           //恢复出厂设置（恢复除IP地址外的所有SPI-FLASH数据）

        GetBoardInformation = 0x50,    //读取板卡信息
    }

    public enum Board
    {
        Board_A = 0,
        Total
    }

    public enum Axis
    {
        Conveyor_1 = 0,
        Module_1,
        Total
    }

    public enum TransportType
    {
        Absolute = 0,  //绝对运动
        Reference      //相对运动
    }

    public enum AxisDir
    {
        Forward = 0,  //正转
        Reverse = 1,  //反转
    }

    public enum AxisState
    {
        None = 0,
        Ready,      //轴已准备就绪
        ErrorStop,  //出现错误，轴停止
        Busy        //轴正在执行运动
    }

    public struct AxisType
    {
        public int BoardIndex;
        public int AxisIndex;

        public AxisType(int board, int axis)
        {
            BoardIndex = board;
            AxisIndex = axis;
        }
    }

    public struct IoInType
    {
        public int BoardIndex;
        public int IoInIndex;

        public IoInType(int board, int InIndex)
        {
            BoardIndex = board;
            IoInIndex = InIndex;
        }
    }

    public struct IoOutType
    {
        public int BoardIndex;
        public int IoOutIndex;

        public IoOutType(int board, int InOutIndex)
        {
            BoardIndex = board;
            IoOutIndex = InOutIndex;
        }
    }

    public class ControlerBoard
    {
        private static ControlerBoard m_UniqueControlerBoard = null;
        private static readonly object m_Locker = new object();

        public MyTcpClient[] m_MyTcpClient = new MyTcpClient[(int)Board.Total];

        public static readonly int IO_POINT_TOTAL = 4 * 8;        //一块板卡包括32/32个隔离数字量输入/输出通道         
        public static readonly int AXIS_TOTAL = 8;          //一块板卡包括8个电机轴
        
        private ControlerBoard()
        {
            InitAllControlerTcpClient();
        }

        public static ControlerBoard GetInstance()
        {
            if (m_UniqueControlerBoard == null)
            {
                lock (m_Locker)
                {
                    if (m_UniqueControlerBoard == null)
                        m_UniqueControlerBoard = new ControlerBoard();
                }
            }

            return m_UniqueControlerBoard;
        }

        public void InitAllControlerTcpClient()
        {
            for (int i = 0; i < m_MyTcpClient.Length; i++)
            {
                m_MyTcpClient[i] = new MyTcpClient();
            }
        }

        public bool IsControlerConnected(Board BoardIndex)
        {
            if (m_MyTcpClient[(int)BoardIndex] != null)
                return m_MyTcpClient[(int)BoardIndex].IsConnected;
            else
                return false;
        }

        public bool IsAllControlerConnected()
        {
            bool Re = true;
            for (int i = 0; i < (int)Board.Total; i++)
            {
                if (!IsControlerConnected((Board)i))
                {
                    Re = false;
                    break;
                }
            }

            return Re;
        }

        public void OpenControler()
        {
            IPAddress ControlIp = IPAddress.Parse(Profile.m_Config.Board_A_Ip);
            int ControlPort = Profile.m_Config.Board_A_Port;
            if (m_MyTcpClient[(int)Board.Board_A] != null)
                m_MyTcpClient[(int)Board.Board_A].CreateConnect(ControlIp, ControlPort);
        }

        public void CloseControler()
        {
            if (IsControlerConnected(Board.Board_A))
                m_MyTcpClient[(int)Board.Board_A].Close();
        }

        public void GetMotorBoardAndAxisIndexByAxisType(Axis Type, ref int BoadrIndex, ref int AxisIndex)
        {
            switch (Type)
            {
                //空盘传输线
                case Axis.Conveyor_1:
                    {
                        BoadrIndex = Profile.m_Config.Conveyor_1.BoardIndex;
                        AxisIndex = Profile.m_Config.Conveyor_1.AxisIndex;
                    }
                    break;

                default: break;
            }

            if (AxisIndex < 1)
                AxisIndex = 1;
            else if (AxisIndex > AXIS_TOTAL)
                AxisIndex = AXIS_TOTAL;
        }

        public Axis GetAxisTypeByMotorBoardAndAxisIndex(int BoadrIndex, int AxisIndex)
        {
            Axis axis = Axis.Conveyor_1;

            //空盘传输线
            if (BoadrIndex == Profile.m_Config.Conveyor_1.BoardIndex && AxisIndex == Profile.m_Config.Conveyor_1.AxisIndex)
                axis = Axis.Conveyor_1;

            return axis;
        }

        public void GetIoInBoardAndAxisIndexByIO_IN_Type(IO_IN_Type IoInType, ref int BoadrIndex, ref int IoInIndex)
        {
            switch (IoInType)
            {
                case IO_IN_Type.IO_IN_KeyRun:
                    {
                        BoadrIndex = Profile.m_Config.IO_IN_KeyRun.BoardIndex;
                        IoInIndex = Profile.m_Config.IO_IN_KeyRun.IoInIndex;
                    }
                    break;
                case IO_IN_Type.IO_IN_KeyPause:
                    {
                        BoadrIndex = Profile.m_Config.IO_IN_KeyPause.BoardIndex;
                        IoInIndex = Profile.m_Config.IO_IN_KeyPause.IoInIndex;
                    }
                    break;
                case IO_IN_Type.IO_IN_KeyStop:
                    {
                        BoadrIndex = Profile.m_Config.IO_IN_KeyStop.BoardIndex;
                        IoInIndex = Profile.m_Config.IO_IN_KeyStop.IoInIndex;
                    }
                    break;
                case IO_IN_Type.IO_IN_KeyReset:
                    {
                        BoadrIndex = Profile.m_Config.IO_IN_KeyReset.BoardIndex;
                        IoInIndex = Profile.m_Config.IO_IN_KeyReset.IoInIndex;
                    }
                    break;
                case IO_IN_Type.IO_IN_KeyScramSignal:
                    {
                        BoadrIndex = Profile.m_Config.IO_IN_KeyScramSignal.BoardIndex;
                        IoInIndex = Profile.m_Config.IO_IN_KeyScramSignal.IoInIndex;
                    }
                    break;

                default:
                    break;
            }

            IoInIndex = IoInIndex - 1;  //配置文件中都是从1开始，和单片机交互时从0开始

            int IoTotal = IO_POINT_TOTAL;
            if (IoInIndex < 0)
                IoInIndex = 0;
            else if (IoInIndex >= IoTotal)
                IoInIndex = IoTotal - 1;
        }

        public void GetIoOutBoardAndAxisIndexByIO_OUT_Type(IO_OUT_Type IoOutType, ref int BoadrIndex, ref int IoOutIndex)
        {
            switch (IoOutType)
            {
                case IO_OUT_Type.IO_OUT_LedRed:
                    {
                        BoadrIndex = Profile.m_Config.IO_OUT_LedRed.BoardIndex;
                        IoOutIndex = Profile.m_Config.IO_OUT_LedRed.IoOutIndex;
                    }
                    break;
                case IO_OUT_Type.IO_OUT_LedOriange:
                    {
                        BoadrIndex = Profile.m_Config.IO_OUT_LedOriange.BoardIndex;
                        IoOutIndex = Profile.m_Config.IO_OUT_LedOriange.IoOutIndex;
                    }
                    break;
                case IO_OUT_Type.IO_OUT_LedGreen:
                    {
                        BoadrIndex = Profile.m_Config.IO_OUT_LedGreen.BoardIndex;
                        IoOutIndex = Profile.m_Config.IO_OUT_LedGreen.IoOutIndex;
                    }
                    break;
                case IO_OUT_Type.IO_OUT_LedKeyRun:
                    {
                        BoadrIndex = Profile.m_Config.IO_OUT_LedKeyRun.BoardIndex;
                        IoOutIndex = Profile.m_Config.IO_OUT_LedKeyRun.IoOutIndex;
                    }
                    break;
                case IO_OUT_Type.IO_OUT_LedKeyPause:
                    {
                        BoadrIndex = Profile.m_Config.IO_OUT_LedKeyPause.BoardIndex;
                        IoOutIndex = Profile.m_Config.IO_OUT_LedKeyPause.IoOutIndex;
                    }
                    break;
                case IO_OUT_Type.IO_OUT_LedKeyStop:
                    {
                        BoadrIndex = Profile.m_Config.IO_OUT_LedKeyStop.BoardIndex;
                        IoOutIndex = Profile.m_Config.IO_OUT_LedKeyStop.IoOutIndex;
                    }
                    break;
                case IO_OUT_Type.IO_OUT_Beep:
                    {
                        BoadrIndex = Profile.m_Config.IO_OUT_Beep.BoardIndex;
                        IoOutIndex = Profile.m_Config.IO_OUT_Beep.IoOutIndex;
                    }
                    break;


                default:
                    break;
            }

            IoOutIndex = IoOutIndex - 1;

            if (IoOutIndex < 0)
                IoOutIndex = 0;
            else if (IoOutIndex >= IO_POINT_TOTAL)
                IoOutIndex = IO_POINT_TOTAL - 1;
        }

        public void SendCommandToSetControlBoardOutput(IO_OUT_Type Io, IOValue Value)
        {
            int BoardIndex = 0;
            int IoOutIndex = 0;
            GetIoOutBoardAndAxisIndexByIO_OUT_Type(Io, ref BoardIndex, ref IoOutIndex);

            if (!IsControlerConnected((Board)BoardIndex))
                return;

            uint outputBuffer = 0;  //输出口数据
            uint outputEnable = 0;  //输出口使能
            if (Value == IOValue.High)
                outputBuffer |= ((uint)1 << IoOutIndex);

            outputEnable |= ((uint)1 << IoOutIndex);

            byte[] temp = new byte[CommunicationProtocol.MessageLength];
            CommunicationProtocol.MakeSendArrayByCode((byte)ControlerCommandCode.SetOutput, ref temp);

            const int DataIndex = CommunicationProtocol.MessageCommandIndex + 1;
            temp[DataIndex + 0] = (byte)(outputEnable & 0xffU);
            temp[DataIndex + 1] = (byte)((outputEnable >> 8) & 0xffU);
            temp[DataIndex + 2] = (byte)((outputEnable >> 16) & 0xffU);
            temp[DataIndex + 3] = (byte)((outputEnable >> 24) & 0xffU);
            temp[DataIndex + 4] = (byte)(outputBuffer & 0xffU);
            temp[DataIndex + 5] = (byte)((outputBuffer >> 8) & 0xffU);
            temp[DataIndex + 6] = (byte)((outputBuffer >> 16) & 0xffU);
            temp[DataIndex + 7] = (byte)((outputBuffer >> 24) & 0xffU);

            temp[CommunicationProtocol.MessageSumCheck] = 0x00;
            temp[CommunicationProtocol.MessageSumCheck] = MyMath.CalculateSum(temp, CommunicationProtocol.MessageLength);

            AddCommandMessageToQueue((Board)BoardIndex, ControlerCommandCode.SetOutput, ref temp);
        }

        public void SendCommandToSetControlBoardOutputByInput(IO_IN_Type Io_In, int Value, IO_OUT_Type Io_Out1, int Out1_Value, IO_OUT_Type Io_Out2, int Out2_Value)
        {
            int BoardIndex = 0;
            int IoInIndex = 0;
            GetIoInBoardAndAxisIndexByIO_IN_Type(Io_In, ref BoardIndex, ref IoInIndex);

            int indexBoardOut1 = 0;
            int IoOut1Index = 0;
            GetIoOutBoardAndAxisIndexByIO_OUT_Type(Io_Out1, ref indexBoardOut1, ref IoOut1Index);

            int indexBoardOut2 = 0;
            int IoOut2Index = 0;
            GetIoOutBoardAndAxisIndexByIO_OUT_Type(Io_Out2, ref indexBoardOut2, ref IoOut2Index);

            if (!IsControlerConnected((Board)BoardIndex))
                return;

            byte[] temp = new byte[CommunicationProtocol.MessageLength];
            CommunicationProtocol.MakeSendArrayByCode((byte)ControlerCommandCode.SetOutputByInput, ref temp);

            const int DataIndex = CommunicationProtocol.MessageCommandIndex + 1;
            temp[DataIndex + 0] = (byte)IoInIndex;
            temp[DataIndex + 1] = (byte)Value;
            temp[DataIndex + 2] = (byte)IoOut1Index;
            temp[DataIndex + 3] = (byte)Out1_Value;
            temp[DataIndex + 4] = (byte)IoOut2Index;
            temp[DataIndex + 5] = (byte)Out2_Value;

            temp[CommunicationProtocol.MessageSumCheck] = 0x00;
            temp[CommunicationProtocol.MessageSumCheck] = MyMath.CalculateSum(temp, CommunicationProtocol.MessageLength);

            AddCommandMessageToQueue((Board)BoardIndex, ControlerCommandCode.SetOutputByInput, ref temp);
        }

        public void SendCommandToControler(Board BoardIndex, ControlerCommandCode Code)
        {
            if (IsControlerConnected(BoardIndex))
            {
                byte[] temp = new byte[CommunicationProtocol.MessageLength];
                CommunicationProtocol.MakeSendArrayByCode((byte)Code, ref temp);
                AddCommandMessageToQueue(BoardIndex, Code, ref temp);
            }
        }

        public void SendCommandToControlerWithAxis(Axis axis, ControlerCommandCode Code)
        {
            if (axis >= Axis.Total)
                return;

            int BoardIndex = 0;
            int AxisIndex = 0;
            GetMotorBoardAndAxisIndexByAxisType(axis, ref BoardIndex, ref AxisIndex);

            if (IsControlerConnected((Board)BoardIndex))
            {
                byte[] temp = new byte[CommunicationProtocol.MessageLength];
                CommunicationProtocol.MakeSendArrayByCode((byte)Code, ref temp);
                temp[CommunicationProtocol.MessageCommandIndex + 1] = (byte)AxisIndex;
                temp[CommunicationProtocol.MessageSumCheck] = 0x00;
                temp[CommunicationProtocol.MessageSumCheck] = MyMath.CalculateSum(temp, CommunicationProtocol.MessageLength);

                AddCommandMessageToQueue((Board)BoardIndex, Code, ref temp);
            }
        }

        public bool SendCommandToSetSpeedParam(Axis axis, int velLow, int velHigh, int acc, int dec, bool Default)
        {
            int BoardIndex = 0;
            int AxisIndex = 0;
            GetMotorBoardAndAxisIndexByAxisType(axis, ref BoardIndex, ref AxisIndex);

            if (!IsControlerConnected((Board)BoardIndex))
                return false;

            ControlerCommandCode Code = Default ? ControlerCommandCode.SetAxisParametersDefault : ControlerCommandCode.SetAxisParameters;

            byte[] temp = new byte[CommunicationProtocol.MessageLength];
            CommunicationProtocol.MakeSendArrayByCode((byte)Code, ref temp);

            int DataIndex = CommunicationProtocol.MessageCommandIndex + 1;
            temp[DataIndex] = (byte)AxisIndex;

            temp[DataIndex + 1] = (byte)(velLow & 0xffU);
            temp[DataIndex + 2] = (byte)((velLow >> 8) & 0xffU);
            temp[DataIndex + 3] = (byte)((velLow >> 16) & 0xffU);
            temp[DataIndex + 4] = (byte)((velLow >> 24) & 0xffU);
            temp[DataIndex + 5] = (byte)(velHigh & 0xffU);
            temp[DataIndex + 6] = (byte)((velHigh >> 8) & 0xffU);
            temp[DataIndex + 7] = (byte)((velHigh >> 16) & 0xffU);
            temp[DataIndex + 8] = (byte)((velHigh >> 24) & 0xffU);
            temp[DataIndex + 9] = (byte)(acc & 0xffU);
            temp[DataIndex + 10] = (byte)((acc >> 8) & 0xffU);
            temp[DataIndex + 11] = (byte)((acc >> 16) & 0xffU);
            temp[DataIndex + 12] = (byte)((acc >> 24) & 0xffU);
            temp[DataIndex + 13] = (byte)(dec & 0xffU);
            temp[DataIndex + 14] = (byte)((dec >> 8) & 0xffU);
            temp[DataIndex + 15] = (byte)((dec >> 16) & 0xffU);
            temp[DataIndex + 16] = (byte)((dec >> 24) & 0xffU);

            temp[CommunicationProtocol.MessageSumCheck] = 0x00;
            temp[CommunicationProtocol.MessageSumCheck] = MyMath.CalculateSum(temp, CommunicationProtocol.MessageLength);

            AddCommandMessageToQueue((Board)BoardIndex, ControlerCommandCode.SetAxisParametersDefault, ref temp);

            return true;
        }

        public void SendCommandToRunAxis(Axis axis, ControlerCommandCode Code, AxisDir Dir)
        {
            int BoardIndex = 0;
            int AxisIndex = 0;
            GetMotorBoardAndAxisIndexByAxisType(axis, ref BoardIndex, ref AxisIndex);

            if (!IsControlerConnected((Board)BoardIndex))
                return;

            byte[] temp = new byte[CommunicationProtocol.MessageLength];
            CommunicationProtocol.MakeSendArrayByCode((byte)Code, ref temp);

            temp[CommunicationProtocol.MessageCommandIndex + 1] = (byte)AxisIndex;
            if (Dir == AxisDir.Forward)
                temp[CommunicationProtocol.MessageCommandIndex + 2] = 0x00;
            else
                temp[CommunicationProtocol.MessageCommandIndex + 2] = 0x01;

            temp[CommunicationProtocol.MessageSumCheck] = 0x00;
            temp[CommunicationProtocol.MessageSumCheck] = MyMath.CalculateSum(temp, CommunicationProtocol.MessageLength);

            AddCommandMessageToQueue((Board)BoardIndex, Code, ref temp);
        }

        public void SendCommandToRunAxisByTransportType(Axis axis, TransportType type, int steps)
        {
            int BoardIndex = 0;
            int AxisIndex = 0;
            GetMotorBoardAndAxisIndexByAxisType(axis, ref BoardIndex, ref AxisIndex);

            if (!IsControlerConnected((Board)BoardIndex))
                return;

            byte TransportTypeCode = (byte)ControlerCommandCode.SetAxisStepsRef;
            if(type == TransportType.Absolute)
                TransportTypeCode = (byte)ControlerCommandCode.SetAxisStepsAbs;

            byte[] temp = new byte[CommunicationProtocol.MessageLength];
            CommunicationProtocol.MakeSendArrayByCode(TransportTypeCode, ref temp);

            int DataIndex = CommunicationProtocol.MessageCommandIndex + 1;
            temp[DataIndex] = (byte)AxisIndex;
            temp[DataIndex + 1] = (byte)(steps & 0xffU);
            temp[DataIndex + 2] = (byte)((steps >> 8) & 0xffU);
            temp[DataIndex + 3] = (byte)((steps >> 16) & 0xffU);
            temp[DataIndex + 4] = (byte)((steps >> 24) & 0xffU);

            temp[CommunicationProtocol.MessageSumCheck] = 0x00;
            temp[CommunicationProtocol.MessageSumCheck] = MyMath.CalculateSum(temp, CommunicationProtocol.MessageLength);

            AddCommandMessageToQueue((Board)BoardIndex, (ControlerCommandCode)TransportTypeCode, ref temp);
        }
      
        public void SendCommandToReadInputPoint(Board board)
        {
            SendCommandToControler(board, ControlerCommandCode.GetInput);
        }

        public void SendCommandToReadAxisPostion(Axis axis)
        {
            SendCommandToControlerWithAxis(axis, ControlerCommandCode.GetAxisStepsAbs);
        }

        public void SendCommandToReadAxisState(Axis axis)
        {
            SendCommandToControlerWithAxis(axis, ControlerCommandCode.GetAxisState);
        }

        public void AddCommandMessageToQueue(Board boardIndex, ControlerCommandCode Code, ref byte[] Command)
        {
            switch (Code)
            {
                    //设置相关的指令
                case ControlerCommandCode.SetOutput:
                case ControlerCommandCode.SetOutputByInput:
                case ControlerCommandCode.SetOutputDefault:
                case ControlerCommandCode.SetAxisParametersDefault:
                case ControlerCommandCode.SetAxisParameters:
                case ControlerCommandCode.SetAxisStepsAbs:
                case ControlerCommandCode.SetAxisStepsRef:
                case ControlerCommandCode.SetAxisMoveContinuous:
                case ControlerCommandCode.SetAxisStepsMax:
                case ControlerCommandCode.StopAxis:
                case ControlerCommandCode.ResetAxisError:
                case ControlerCommandCode.AxisGoHome:
                case ControlerCommandCode.SetIp:
                case ControlerCommandCode.SetVersionHardware:
                case ControlerCommandCode.ResetFactory:
                    {
                        if (m_MyTcpClient[(int)boardIndex].IsConnected)
                            m_MyTcpClient[(int)boardIndex]. m_SetCommandQueue.Enqueue(Command);
                    }
                    break;
                    //“获取”相关的指令
                case ControlerCommandCode.GetOutput:
                case ControlerCommandCode.GetInput:
                case ControlerCommandCode.GetAxisParameters:
                case ControlerCommandCode.GetAxisStepsAbs:
                case ControlerCommandCode.GetAxisState:
                case ControlerCommandCode.GetBoardInformation:
                    {                        
                        if (m_MyTcpClient[(int)boardIndex].IsConnected)
                        {
                            if (m_MyTcpClient[(int)boardIndex].m_GetCommandQueue.Count == 0)
                            {
                                m_MyTcpClient[(int)boardIndex].m_GetCommandQueue.Enqueue(Command);
                            }                                
                            else
                            {
                                bool AddToDequeue = true;
                                foreach (var temp in m_MyTcpClient[(int)boardIndex].m_GetCommandQueue)
                                {
                                    if (MyMath.IsEquals(temp, Command))  //避免加入的相同的指令过多
                                    {
                                        AddToDequeue = false;
                                        break;
                                    }
                                        
                                }

                                if(AddToDequeue)
                                    m_MyTcpClient[(int)boardIndex].m_GetCommandQueue.Enqueue(Command);
                            }
                        }                                    
                    }
                    break;
                default:
                    break;
            }
        }

        public void SendCommandToControlerByTcp(bool SendSetCommand)
        {
            byte[] temp = new byte[CommunicationProtocol.MessageLength];
            bool[] SendSetCommandFlag = new bool[(int)Board.Total];  //默认值是false

            for (int i = 0; i < (int)Board.Total; i++)
            {
                if (IsControlerConnected((Board)i))
                {
                    Array.Clear(temp, 0, temp.Length);
                    if (SendSetCommand)
                    {
                        if (m_MyTcpClient[i].m_SetCommandQueue.Count > 0)
                        {
                            m_MyTcpClient[i].m_SetCommandQueue.TryDequeue(out temp);
                            m_MyTcpClient[i].ClientWrite(temp);

                            SendSetCommandFlag[i] = true;
                        }
                    }

                    if (!SendSetCommandFlag[i])  //SendSetCommand为false 或者 m_MyTcpClient[i].m_SetCommandQueue队列为空
                    {
                        if (m_MyTcpClient[i].m_GetCommandQueue.Count > 0)
                        {
                            m_MyTcpClient[i].m_GetCommandQueue.TryDequeue(out temp);
                            m_MyTcpClient[i].ClientWrite(temp);

                            //Global.WriteLine($"发送查询指令 {BitConverter.ToString(temp)}，当前队列大小为：{m_MyTcpClient[i].m_GetCommandQueue.Count}");
                        }
                    }
                }
            }         
        }
    }
}
