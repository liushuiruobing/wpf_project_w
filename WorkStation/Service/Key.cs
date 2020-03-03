using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkStation_Wpf
{
    public enum KeyType
    {
        Run = 0,
        Pause,
        Stop,
        Reset,
        Scram,

        Total
    }

    public class Key
    {
        //单例子模式创建类
        private static Key m_UniqueKey = null;
        private static readonly object m_Locker = new object();

        //用于按键消斗处理
        private int[] m_KeyPressedCount = new int[(int)KeyType.Total];
        private readonly int m_KeyPressedCountTotal = 50;

        private Key()
        {

        }

        public static Key GetInstance()
        {
            if (m_UniqueKey == null)
            {
                lock (m_Locker)
                {
                    if (m_UniqueKey == null)
                        m_UniqueKey = new Key();
                }
            }

            return m_UniqueKey;
        }

        private void ProcessKey(KeyType key)
        {
            switch (key)
            {
                case KeyType.Run:
                    {

                    }
                    break;
                case KeyType.Pause:
                case KeyType.Stop:
                    {
                    }
                    break;
                case KeyType.Reset:
                    {
                    }
                    break;
                case KeyType.Scram:
                    {
                    }
                    break;
                default:
                    break;
            }
        }

        //UI按钮处理
        public void ProcessKeyByRunFormButton(KeyType key)
        {
            ProcessKey(key);
        }

        //机械按钮处理
        public void ProcessKeyByInputKey(KeyType key, bool BeenPressed)
        {
            //手动调试对话框时系统按键不处理具体的业务
            //if (MainForm.SelectedManual && key < KeyType.Scram)
            //    return;

            if (BeenPressed)
            {
                m_KeyPressedCount[(int)key]++;
                if (m_KeyPressedCount[(int)key] >= m_KeyPressedCountTotal)
                {
                    m_KeyPressedCount[(int)key] = 0;
                    ProcessKey(key);
                }
            }
            else
            {
                m_KeyPressedCount[(int)key] = 0;
            }
        }
    }
}
