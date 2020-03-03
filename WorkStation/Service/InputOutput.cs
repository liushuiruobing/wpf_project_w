using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkStation_Wpf
{
    public enum AlarmLed
    {
        AlarmLed_None = 0,
        AlarmLed_Green,
        AlarmLed_Oriange,
        AlarmLed_Red,
    }

    public enum IO_IN_Type
    {
        //Key
        IO_IN_KeyRun = 0,
        IO_IN_KeyPause,
        IO_IN_KeyStop,
        IO_IN_KeyReset,
        IO_IN_KeyScramSignal,

        IO_IN_Total   //所有IO_IN的总点数
    }

    public enum IO_OUT_Type
    {
        //Led
        IO_OUT_LedRed = 0,
        IO_OUT_LedOriange,
        IO_OUT_LedGreen,

        IO_OUT_LedKeyRun,
        IO_OUT_LedKeyPause,
        IO_OUT_LedKeyStop,
        IO_OUT_Beep,

        IO_OUT_Total   //所有IO_OUT的总点数
    }

    public enum IOValue
    {
        Low = 0,
        High
    }

    public class InputOutput
    {
        private static InputOutput m_UniqueInputOutput = null;
        private static readonly object m_Locker = new object();
        private static ControlerBoard m_ControlerBoard = ControlerBoard.GetInstance();

        private uint[] m_InputValue = new uint[(int)Board.Total];  //ARM控制板IO输入的缓存 4个byte，每位代表1个IO，共32个,从而用uint来表示，32位每个位代表1个IO  
        private IOValue[] m_InputPointStateBackups = new IOValue[(int)IO_IN_Type.IO_IN_Total];

        private int AlarmTimerCount = 0;
        private readonly int MaxTimerCount = 10000000;
        private System.Timers.Timer m_BeepAlarmTimer = new System.Timers.Timer();
        private bool m_BeepAndTowerLampFlag = false;  //蜂鸣器和塔灯报警标志
        private static AlarmLed m_AlarmLedBackup = AlarmLed.AlarmLed_None;

        private InputOutput()
        {

        }

        public static InputOutput GetInstance()
        {
            if (m_UniqueInputOutput == null)
            {
                lock (m_Locker)
                {
                    if (m_UniqueInputOutput == null)
                        m_UniqueInputOutput = new InputOutput();
                }
            }

            return m_UniqueInputOutput;
        }

        public void InitBeepTimer()
        {
            if (m_BeepAlarmTimer != null)
            {
                m_BeepAlarmTimer.Elapsed += BeepAlarmTimerEventProcessor;
                m_BeepAlarmTimer.Interval = 1000;
                m_BeepAlarmTimer.Start();
            }
        }

        private void BeepAlarmTimerEventProcessor(Object source, System.Timers.ElapsedEventArgs e)
        {
            AlarmTimerCount++;
            if (AlarmTimerCount >= MaxTimerCount)
                AlarmTimerCount = 0;

            if (m_BeepAndTowerLampFlag)
            {
                if (AlarmTimerCount % 2 == 0)
                    OpenAlarmLedAndBeep();
                else
                    CloseAlarmLedAndBeep();
            }
        }

        public static void SendCommandToReadInputPoint()
        {
            if (m_ControlerBoard != null)
            {
                for (int i = 0; i < (int)Board.Total; i++)
                {
                    if(m_ControlerBoard.IsControlerConnected((Board)i))
                        m_ControlerBoard.SendCommandToReadInputPoint((Board)i);
                }
            }                
        }

        public void SetInputPoint(Board board, uint InputData)
        {
            m_InputValue[(int)board] = InputData;
        }

        public IOValue ReadInputPoint(IO_IN_Type IoIn)
        {
            bool Re = false;

            int BoardIndex = 0;  
            int IoInIndex = 0;   
            m_ControlerBoard.GetIoInBoardAndAxisIndexByIO_IN_Type(IoIn, ref BoardIndex, ref IoInIndex);

            uint mask = (uint)1 << IoInIndex;
            Re = (m_InputValue[BoardIndex] & mask) > 0;

            if (Re)
                return IOValue.High;
            else
                return IOValue.Low;
        }

        public void SetInputPointStateBackups(IO_IN_Type Point, IOValue Value)
        {
            m_InputPointStateBackups[(int)Point] = Value;
        }

        public IOValue ReadInputPointStateBackups(IO_IN_Type Point)
        {
            return m_InputPointStateBackups[(int)Point];
        }

        public void ResetInputPointStateBackups()
        {
            for (int i = 0; i < (int)IO_IN_Type.IO_IN_Total; i++)
            {
                m_InputPointStateBackups[i] = IOValue.Low;
            }
        }
        
        public void SetOutputPoint(IO_OUT_Type Io, IOValue Value)
        {
            if (m_ControlerBoard != null)
                m_ControlerBoard.SendCommandToSetControlBoardOutput(Io, Value);            
        }

        public void OpenAlarmLedAndBeep()
        {
            SetOutputPoint(IO_OUT_Type.IO_OUT_LedRed, IOValue.High);
            SetOutputPoint(IO_OUT_Type.IO_OUT_Beep, IOValue.High);
        }

        public void CloseAlarmLedAndBeep()
        {
            SetOutputPoint(IO_OUT_Type.IO_OUT_LedRed, IOValue.Low);
            SetOutputPoint(IO_OUT_Type.IO_OUT_Beep, IOValue.Low);
        }

        public void SetSysAlarmTowerLed(AlarmLed LedType)
        {
            if (m_AlarmLedBackup == LedType)
                return;
            else
                m_AlarmLedBackup = LedType;

            switch (LedType)
            {
                case AlarmLed.AlarmLed_Green:
                    {
                        SetOutputPoint(IO_OUT_Type.IO_OUT_LedGreen, IOValue.High);
                        SetOutputPoint(IO_OUT_Type.IO_OUT_LedOriange, IOValue.Low);
                        SetOutputPoint(IO_OUT_Type.IO_OUT_LedRed, IOValue.Low);

                        SetOutputPoint(IO_OUT_Type.IO_OUT_LedKeyRun, IOValue.High);
                        SetOutputPoint(IO_OUT_Type.IO_OUT_LedKeyPause, IOValue.Low);
                        SetOutputPoint(IO_OUT_Type.IO_OUT_LedKeyStop, IOValue.Low);
                    }
                    break;
                case AlarmLed.AlarmLed_Oriange:
                    {
                        SetOutputPoint(IO_OUT_Type.IO_OUT_LedGreen, IOValue.Low);
                        SetOutputPoint(IO_OUT_Type.IO_OUT_LedOriange, IOValue.High);
                        SetOutputPoint(IO_OUT_Type.IO_OUT_LedRed, IOValue.Low);

                        SetOutputPoint(IO_OUT_Type.IO_OUT_LedKeyRun, IOValue.Low);
                        SetOutputPoint(IO_OUT_Type.IO_OUT_LedKeyPause, IOValue.High);
                        SetOutputPoint(IO_OUT_Type.IO_OUT_LedKeyStop, IOValue.Low);
                    }
                    break;
                case AlarmLed.AlarmLed_Red:
                    {
                        SetOutputPoint(IO_OUT_Type.IO_OUT_LedGreen, IOValue.Low);
                        SetOutputPoint(IO_OUT_Type.IO_OUT_LedOriange, IOValue.Low);
                        SetOutputPoint(IO_OUT_Type.IO_OUT_LedRed, IOValue.High);

                        SetOutputPoint(IO_OUT_Type.IO_OUT_LedKeyRun, IOValue.Low);
                        SetOutputPoint(IO_OUT_Type.IO_OUT_LedKeyPause, IOValue.Low);
                        SetOutputPoint(IO_OUT_Type.IO_OUT_LedKeyStop, IOValue.High);
                    }
                    break;

                default:
                    break;
            }
        }

    }
}
