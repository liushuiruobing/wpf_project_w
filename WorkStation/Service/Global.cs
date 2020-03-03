#define Debug_Show


namespace WorkStation_Wpf
{
    /// <summary>
    /// 此类中是全局使用的字符串和弹窗提示
    /// </summary>
    public class Global
    {
        #region 属性

        public static readonly string StationName = "工作站";
        public static readonly string StrLoadConfigFailed = "加载配置文件失败！";
        public static readonly string StrSaveConfigFailed = "保存配置文件失败！";
        public static readonly string StrSelectedFormError = "请关闭自动测试后，再切换菜单！";
        public static readonly string StrInputError = "请输入正确的数值！";
        public static readonly string StrIfReadyToSaveDataToConfigFile = "是否要将当前参数保存到配置文件中 ?";

        #endregion

        #region 方法

        public static void MessageBoxShow_Question(string StrMessage)
        {
            //return MessageBox.Show(StrMessage, StationName, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }

        public static void MessageBoxShow_Error(string StrMessage)
        {
            //return MessageBox.Show(StrMessage, StationName, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }

        #endregion
    }
}
