using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkStation_Wpf
{
    /// <summary>
    /// Robot 控制类
    /// </summary>
    
    public class Robot
    {
        private static Robot m_UniqueRobot = new Robot();
        private static readonly object m_Locker = new object();

        private Robot()
        {

        }

        public static Robot GetInstance()
        {
            if (m_UniqueRobot == null)
            {
                lock (m_Locker)
                {
                    if (m_UniqueRobot == null)
                        m_UniqueRobot = new Robot();
                }
            }

            return m_UniqueRobot;
        }
    }
}
