using System;
using System.Collections.Generic;
using System.Text;

namespace WorkStation
{
    class MyMath
    {
        /*
        Description: 检查通信协议校验和
        Input: buffer 数据
        Output:	length 数据长度
        Return:			
        */
        static public bool CheckSum(byte[] buffer, int length)
        {
            byte sum = 0;
            for (int i = 0; i < length; i++)
            {
                sum += buffer[i];
            }
            return sum == 0;
        }

        /*
        Description: 计算检查通信协议校验和
        Input: buffer 数据
        Output:	length 数据长度
        Return:			
        */
        static public byte CalculateSum(byte[] buffer, int length)
        {
            byte sum = 0;
            for (int i = 0; i < length; i++)
            {
                sum += buffer[i];
            }
            return (byte)(0x100 - sum);
        }

        public static bool IsEquals(byte[] ArrayA, byte[] ArrayB)
        {
            bool Re = true;

            if (ArrayA.Length == ArrayB.Length)
            {
                for (int i = 0; i < ArrayA.Length; i++)
                {
                    if (ArrayA[i] != ArrayB[i])
                    {
                        Re = false;
                        break;
                    }
                }
            }
            else
            {
                Re = false;
            }
                
            return Re;
        }
    }
}
