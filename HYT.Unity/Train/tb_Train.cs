using System;
using System.Collections.Generic;
using System.Text;

namespace KT.TCP
{
    /// <summary>
    /// JS本地数据库 训练配置信息
    /// </summary>
    public class tb_Train
    {
        public string id { get; set; }

        public string name { get; set; }

        public string img { get; set; }

        public string configFile { get; set; } = "config.json";

        public string exePath { get; set; }

        public int maxTime { get; set; }

        public int maxBuSu { get; set; } = 10;

        public int maxBuPin { get; set; } = 200;

        public int maxDifficulty { get; set; } = 5;

        public string runMode { get; set; } = "Unity";
    }
}
