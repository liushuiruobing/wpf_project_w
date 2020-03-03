using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wpf_Memo.Service
{
    /// <summary>
    /// 告警信息类
    /// </summary>
    public class SysAlarm
    {       
        /*告警数据结构*/
        public struct StructAlarm
        {
            public bool IsAlarm;     //是否报警
            public string DateTimeStr;
            public string ID;        //ID号
            public string Level;     //级别
            public string Type;
            public string Informat;  //告警信息
            public string Solution;  //解决方法
        }

        /*告警类型*/
        public enum Type
        {           
            ConnectVisualFailed,
            Max,
        }

        //告警信息的索引枚举
        public enum SysAlarmInforIndex
        {
            ErrorCodeIndex = 0,
            LevelIndex,
            TypeIndex,
            InformatIndex,
            SolutionIndex,
            Total
        }

        /*告警编码、告警级别、告警类型、告警信息、解决方法*/
        public static readonly string[,] SysAlarmInfor = new string[(int)Type.Max, (int)SysAlarmInforIndex.Total]
        {
            {"E001", "1", "ConnectVisualFailed", "连接视觉服务失败！",  "请检查视觉服务是否开启！"},
        };

        private ILogger m_Log = LogManager.GetCurrentClassLogger();

        private static readonly object m_locker = new object();  // 定义一个标识确保线程同步
        private static SysAlarm m_UniqueSysAlarm;  // 定义一个静态变量来保存类的实例

        StructAlarm[] m_Alarm = new StructAlarm[(int)Type.Max];  //报警信息

        // 定义私有构造函数，使外界不能创建该类实例
        private SysAlarm()
        {
            InitData();
        }

        /// <summary>
        /// 定义公有方法提供一个全局访问点,同时你也可以定义公有属性来提供全局访问点
        /// </summary>
        /// <returns></returns>
        public static SysAlarm GetInstance()
        {
            // 当第一个线程运行到这里时，此时会对locker对象 "加锁"，
            // 当第二个线程运行该方法时，首先检测到locker对象为"加锁"状态，该线程就会挂起等待第一个线程解锁
            // lock语句运行完之后（即线程运行完之后）会对该对象"解锁"
            // 双重锁定只需要一句判断就可以了
            if (m_UniqueSysAlarm == null)
            {
                lock (m_locker)
                {
                    if (m_UniqueSysAlarm == null)  // 如果类的实例不存在则创建，否则直接返回
                    {
                        m_UniqueSysAlarm = new SysAlarm();
                    }
                }
            }

            return m_UniqueSysAlarm;
        }

        /*初始化*/
        private void InitData()
        {
            for (int i = 0; i < (int)Type.Max; i++)
            {
                m_Alarm[i].DateTimeStr = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                m_Alarm[i].IsAlarm = false;

                m_Alarm[i].ID = SysAlarmInfor[i,(int)SysAlarmInforIndex.ErrorCodeIndex];            
                m_Alarm[i].Level = SysAlarmInfor[i, (int)SysAlarmInforIndex.LevelIndex];
                m_Alarm[i].Type = SysAlarmInfor[i, (int)SysAlarmInforIndex.TypeIndex];
                m_Alarm[i].Informat = SysAlarmInfor[i, (int)SysAlarmInforIndex.InformatIndex];
                m_Alarm[i].Solution = SysAlarmInfor[i, (int)SysAlarmInforIndex.SolutionIndex];
            }
        }

        /*获取报警信息*/
        public StructAlarm GetAlarm(Type type)
        {
            return m_Alarm[(int)type];
        }

        /*设置报警信息*/
        public void SetAlarm(Type type, bool isAlarm)
        {        
            m_Alarm[(int)type].DateTimeStr = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            m_Alarm[(int)type].IsAlarm = isAlarm;
        }
       
        /*清除所有*/
        public void ClearAll()
        {
            for (int i = 0; i < (int)SysAlarm.Type.Max; i++)
            {
                m_Alarm[i].IsAlarm = false;
            }
        }

        public string GetAlarmInformation(SysAlarm.Type type)
        {
            return SysAlarmInfor[(int)type, (int)SysAlarmInforIndex.InformatIndex];
        }

        public string GetAlarmSolution(SysAlarm.Type type)
        {            
            return SysAlarmInfor[(int)type, (int)SysAlarmInforIndex.SolutionIndex];
        }

        public void SaveAlarmMessageToFile(SysAlarm.Type type)
        {
            m_Log.Warn(m_Alarm[(int)type].Informat);
        }
    }
}
