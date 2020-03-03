using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkStation
{
    public class AxisSpeedParam
    {
        public int StartSpeed = 0;
        public int RunSpeed = 0;
        public int AddSpeed = 0;
        public int DecSpeed = 0;

        public AxisSpeedParam()
        {
        }
    }

    public class MotorAxis
    {
        protected static ControlerBoard m_ControlerBoard = ControlerBoard.GetInstance();
        private static int[,] m_AxisState = new int[(int)Board.Total, ControlerBoard.AXIS_TOTAL];  //电机轴状态
        private static int[,] m_AxisPostion = new int[(int)Board.Total, ControlerBoard.AXIS_TOTAL];  //电机轴当前位置

        public MotorAxis()
        {
        }

        public static void SendCommandToGetAllAxisState()
        {
            if (m_ControlerBoard != null)
            {
                for (int i = 0; i < (int)Axis.Total; i++)
                {
                    m_ControlerBoard.SendCommandToReadAxisState((Axis)i);
                }
            }
        }

        public static void SendCommandToGetAllAxisPosition()
        {
            if (m_ControlerBoard != null)
            {
                for (int i = 0; i < (int)Axis.Total; i++)
                {
                    m_ControlerBoard.SendCommandToReadAxisPostion((Axis)i);
                }
            }
        }

        public static void SetAxisPostion(int BoardIndex, int AxisIndex, uint steps)
        {
            AxisIndex = AxisIndex - 1;  //单片机的轴号从1开始
            m_AxisPostion[BoardIndex, AxisIndex] = (int)steps;  //有负值
        }

        public static int GetAxisPostion(Axis axis)
        {
            int BoardIndex = 0;
            int AxisIndex = 0;
            m_ControlerBoard.GetMotorBoardAndAxisIndexByAxisType(axis, ref BoardIndex, ref AxisIndex);

            if (!m_ControlerBoard.IsControlerConnected((Board)BoardIndex))
                return 0;

            AxisIndex = AxisIndex - 1;  //单片机的轴号从1开始
            return m_AxisPostion[BoardIndex, AxisIndex];
        }

        public static void SetAxisState(int BoardIndex, int AxisIndex, int State)
        {
            AxisIndex = AxisIndex - 1;  //单片机的轴号从1开始
            m_AxisState[BoardIndex, AxisIndex] = State;  //m_AxisState 中轴的索引为DataIndex - 1
        }

        public static AxisState GetAxisState(Axis axis)
        {
            int BoardIndex = 0;
            int AxisIndex = 0;
            m_ControlerBoard.GetMotorBoardAndAxisIndexByAxisType(axis, ref BoardIndex, ref AxisIndex);

            return (AxisState)m_AxisState[BoardIndex, AxisIndex - 1]; //单片机的轴号从1开始
        }

        public static void ProcessArmControlerAxisPosition(Axis axis, int CurPos)
        {
            switch (axis)
            {
                case Axis.Module_1:
                    {
                    }break;
                default:
                    break;
            }
        }

        public static void ProcessArmControlerAxisState(Axis axis, AxisState State)   //单片机控制板电机轴的状态
        {
            switch (axis)
            {
                case Axis.Conveyor_1:  //空盘传输线
                    {
                    }break;
                default:
                    break;
            }
        }

        public void SetAixsSpeedParam(Axis axis, int velLow, int velHigh, int acc, int dec, bool Default)
        {
            if (m_ControlerBoard != null)
                m_ControlerBoard.SendCommandToSetSpeedParam(axis, velLow, velHigh, acc, dec, Default);
        }

        public void MoveContinuous(Axis axis, AxisDir dir)
        {
            if (m_ControlerBoard != null)
                m_ControlerBoard.SendCommandToRunAxis(axis, ControlerCommandCode.SetAxisMoveContinuous, dir);     
        }

        public void MoveReference(Axis axis, int steps)
        {
            if (m_ControlerBoard != null)
                m_ControlerBoard.SendCommandToRunAxisByTransportType(axis, TransportType.Reference, steps);
        }

        public void Stop(Axis axis)
        {
            if (m_ControlerBoard != null)
                m_ControlerBoard.SendCommandToControlerWithAxis(axis, ControlerCommandCode.StopAxis); 
        }
    }

    //连续正反转，不依赖绝对位置运动的电机
    public class Conveyor: MotorAxis
    {
        public Conveyor()
        {
        }
    }

    //具备绝对坐标位移的电机
    public class SingleModule: MotorAxis
    {
        public SingleModule()
        {
        }

        public void BackHome(Axis axis, AxisDir dir)
        {
            if (m_ControlerBoard != null)
                m_ControlerBoard.SendCommandToRunAxis(axis, ControlerCommandCode.AxisGoHome, dir);
        }

        public void MoveAbsulate(Axis axis, int steps)
        {
            if (m_ControlerBoard != null)
                m_ControlerBoard.SendCommandToRunAxisByTransportType(axis, TransportType.Absolute, steps);
        }
    }
}
