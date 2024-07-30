using System;
using System.Collections.Generic;
using System.Text;

namespace KT.TCP
{
    /// <summary>
    /// 主程序发给Unity训练启动消息
    /// </summary>
    public class TrainStartInfo : tb_Train
    {
        /// <summary>
        /// 【此项目无用】设置难度
        /// </summary>
        public int set_Difficulty { get; set; }

        /// <summary>
        /// 【实景训练】设置时长
        /// </summary>
        public int set_Time { get; set; }

        /// <summary>
        /// 【实景训练】设置步速
        /// </summary>
        public float set_BuSu { get; set; }

        /// <summary>
        /// 【实景训练】设置步频
        /// </summary>
        public float set_BuPin { get; set; }

        /// <summary>
        /// 【实景训练】设置对称性
        /// </summary>
        public float set_DuiChenXing { get; set; }

        /// <summary>
        /// JSON排行信息
        /// </summary>
        public string Rank { get; set; } = "";

        /// <summary>
        /// JSON曲库信息
        /// </summary>
        public string MusicLibrary { get; set; } = "";

        /// <summary>
        /// 索引
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 【程序设置】处理游戏变为独立进程不退出的问题  在游戏中检测该进程退出就退出游戏
        /// </summary>
        public int ParentProcessID { get; set; } = -1;


        /// <summary>
        /// JSON用户信息：用户ID、用户姓名、症状
        /// </summary>
        public string UserInfo { get; set; } = "";

    }

    /// <summary>
    /// 曲库启动信息 发给训练
    /// </summary>
    public class TrainMusicLibrary
    {
        public string TrainID { get; set; }
        public string TrainName { get; set; }

        public List<MusicLibrary_Music> Musics= new List<MusicLibrary_Music>();
    }

    public class MusicLibrary_Music
    {
        /// <summary>
        /// 音乐ID
        /// </summary>
        public string MusicID { get; set; }

        /// <summary>
        /// 音乐名称
        /// </summary>           
        public string MusicName { get; set; }

        /// <summary>
        /// 音乐BPM
        /// </summary>           
        public int BPM { get; set; }

        /// <summary>
        /// 用户准确率
        /// </summary>
        public int ZQL { get; set; }

        /// <summary>
        /// 用户得分
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// 不同难度 最高的训练记录
        /// </summary>
        public List<MusicLibrary_Record> Records = new List<MusicLibrary_Record>();
    }

    /// <summary>
    /// 最高训练记录
    /// </summary>
    public class MusicLibrary_Record
    {
        public int Difficulty { get; set; }

        public int ZQL { get; set; }

        public int Score { get; set; }

    }
}
