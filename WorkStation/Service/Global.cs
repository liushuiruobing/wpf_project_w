#define Debug_Show

using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using NLog;
using System;
using System.Windows;

namespace WorkStation
{
    /// <summary>
    /// 此类中是全局使用的字符串和弹窗提示
    /// </summary>
    public class Global
    {
        #region 属性
        public static ILogger m_Log = LogManager.GetCurrentClassLogger();

        public static readonly string StationName = "工作站";
        public static readonly string StrLoadConfigFailed = "加载配置文件失败！";
        public static readonly string StrSaveConfigFailed = "保存配置文件失败！";
        public static readonly string StrSelectedFormError = "请关闭自动测试后，再切换菜单！";
        public static readonly string StrInputError = "请输入正确的数值！";
        public static readonly string StrIfReadyToSaveDataToConfigFile = "是否要将当前参数保存到配置文件中 ?";

        #endregion

        #region 方法

        #endregion
    }
}
