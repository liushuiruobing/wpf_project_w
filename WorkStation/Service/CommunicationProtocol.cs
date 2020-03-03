using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkStation
{
    class CommunicationProtocol
    {
        public const int MessageLength = 32;        //通信协议的消息长度是32字节
        public const int MessageStateIndex = 4;     //状态码的索引是4
        public const int MessageCommandIndex = 5;   //命令吗的索引是5
        public const int MessageSumCheck = MessageLength - 2;

        public const byte MessStartCode = 0x7e;     //起始同步码
        public const byte MessEndCode = 0x0d;     //终止同步码
        public const byte MessVID1 = 0x44;     //厂商标识符1 字母‘D’
        public const byte MessVID2 = 0x52;     //厂商标识符2 字母‘R’
        public const byte MessStationCode = 0x53;    //工作站类型，字母'S'
        public const byte MessVer = 0x01;     //通信协议版本号
        public const byte MessRightState = 0x01;     //状态码，发送时初始为0x01，正确返回为0x01，错误返回为0x81
        public const byte MessErrorState = 0x81;

        //Robot相关
        public const byte MessRobotVID1 = 0x54;     //台达：‘T’
        public const byte MessRobotVID2 = 0x44;     //台达：‘D’
        public const byte MessRobotAxle4 = 0x04;     //4轴机械臂
        public const byte MessRobotAxle6 = 0x06;     //6轴机械臂
        public const byte MessRobotAddr = 0x01;     //机械臂地址

        public enum CommandCode_PLC
        {
            GetCurStationState = 0x00,
        }

        public static bool CheckRobotMessage(short[] message, int MessageLength)
        {
            bool Re = false;

            if ((message[0] == MessStartCode) && (message[MessageLength - 1] == MessEndCode)
                && (message[1] == MessRobotVID1) && (message[2] == MessRobotVID2) && (message[3] == MessRobotAxle6) && (message[4] == MessRobotAddr))

            {
                Re = true;
            }

            return Re;
        }

        public static void MakeSendArrayByCode(byte Code, ref byte[] SendMeas)
        {
            Array.Clear(SendMeas, 0, SendMeas.Length);

            if (SendMeas.Length >= MessageLength)
            {
                SendMeas[0] = MessStartCode;
                SendMeas[1] = MessVID1;
                SendMeas[2] = MessVID2;
                SendMeas[3] = MessVer;
                SendMeas[MessageStateIndex] = MessRightState;
                SendMeas[MessageCommandIndex] = Code;

                //其余位填充0x00
                for (int i = MessageCommandIndex + 1; i < MessageLength - 1; i++)
                    SendMeas[i] = 0x00;

                SendMeas[MessageLength - 1] = MessEndCode;

                byte Sum = 0;
                foreach (byte Temp in SendMeas)
                    Sum += Temp;

                SendMeas[MessageSumCheck] = (byte)(0 - Sum);  //校验和
            }
        }
    }
}
