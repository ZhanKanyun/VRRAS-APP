using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KCL
{
    /// <summary>
    /// 配置文件管理器
    /// </summary>
    public class ConfigManager
    {
        #region ■------------------ 单例

        private ConfigManager() { }
        public static readonly ConfigManager Instance = new ConfigManager();

        #endregion

        public JToken? JTokenSystem;
        public JToken? JTokenDevelop;

        /// <summary>
        /// 公共系统设置
        /// </summary>
        public SystemSetting? SSetting = new SystemSetting();

        /// <summary>
        /// 开发者配置，私人配置 不辅助到输出目录
        /// </summary>
        public DevelopSetting? DSetting = new DevelopSetting();


        public void Load()
        {
            string address = AppDomain.CurrentDomain.BaseDirectory + @"Content\config_system.json";
            using (StreamReader sw = new StreamReader(address))
            {
                string json = sw.ReadToEnd();
                JTokenSystem = JsonConvert.DeserializeObject<JToken>(json);
                SSetting = JsonConvert.DeserializeObject<SystemSetting>(this.JTokenSystem.ToString());
            }

            try
            {
                address = AppDomain.CurrentDomain.BaseDirectory + @"Content\config_develop.json";
                using (StreamReader sw = new StreamReader(address))
                {
                    string json = sw.ReadToEnd();
                    JTokenDevelop = JsonConvert.DeserializeObject<JToken>(json);
                    DSetting = JsonConvert.DeserializeObject<DevelopSetting>(this.JTokenDevelop.ToString());
                }
            }
            catch (Exception ex2)
            {
                //LogHelper.Error(ex2);
            }
        }
    }

    /// <summary>
    /// 系统设置
    /// </summary>
    public class SystemSetting
    {
        /// <summary>
        /// 程序名称
        /// </summary>
        public string AppName { get; set; } = "HYT";

        /// <summary>
        /// Web程序正式路径，当测试路径为空的时候就用正式路径
        /// </summary>
        public string APPPath { get; set; } = "Web\\index.html";

        public int BaudRate { get; set; } = 115200;

        /// <summary>
        /// 是否调试模式 可以打开调试窗口
        /// </summary>
        public bool IsDebug { get; set; }

        public int TCPPort { get; set; } = 8888;

        /// <summary>
        /// 0-不使用 1-触摸键盘 2-屏幕键盘OSK
        /// </summary>
        public int KeyBoardType { get; set; } = 2;

        /// <summary>
        /// Y坐标对应的距离 单位mm
        /// </summary>
        public float CoordinateYToDistance { get; set; } = 7.8125f;

        /// <summary>
        /// X坐标对应的距离 单位mm
        /// </summary>
        public float CoordinateXToDistance { get; set; } = 12.5f;

        /// <summary>
        /// 坐标系模式 1=原点在左上（目前） 2-原点在左下
        /// </summary>
        public int CoordinateMode { get; set; } = 1;

        /// <summary>
        /// 添加用户是否自动登录
        /// </summary>
        public bool AddUserIsAutoLogin { get; set; } = true;
    }

    /// <summary>
    /// 个人设置
    /// </summary>
    public class DevelopSetting
    {
        /// <summary>
        /// Web程序所在测试路径，开发测试用
        /// </summary>
        public string AppTestPath { get; set; } = "";

    }
}
