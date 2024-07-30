using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace KT.TCP
{
    /// <summary>
    /// 训练消息
    /// </summary>
    public class TrainMsg
    {
        public TrainMsg() { }
        public TrainMsg(TrainMsgTypes type)
        {
            Type = type;
        }
        public TrainMsg(TrainMsgTypes type, string data)
        {
            Type = type;
            Data = data;
        }

        /// <summary>
        /// 消息ID - 唯一标志，可用于确认收到
        /// </summary>
        public string ID { get; set; } = "";

        /// <summary>
        /// 训练ID
        /// </summary>
        public string TID { get; set; } = "";

        /// <summary>
        /// 消息类型
        /// </summary>
        public TrainMsgTypes Type { get; set; } = 0x00;

        /// <summary>
        /// 字符串型附加数据 根据消息类型决定怎么处理
        /// </summary>
        public string Data { get; set; } = "";

        /// <summary>
        /// 附加数据，根据消息类型决定怎么处理
        /// </summary>
        public int D1 { get; set; } = -1;

        /// <summary>
        /// 附加数据，根据消息类型决定怎么处理
        /// </summary>
        public int D2 { get; set; } = -1;

        /// <summary>
        /// 附加数据 float类型
        /// </summary>
        public float D3 { get; set; } = -1;

        /// <summary>
        /// 附加数据 float类型
        /// </summary>
        public float D4 { get; set; } = -1;

        /// <summary>
        /// 包的大小
        /// </summary>
        public int S { get; set; } = 0;

        public override string ToString()
        {
            return $"({Type.ToString()}) = {JsonConvert.SerializeObject(this)}";
        }
    }


    /// <summary>
    /// 训练消息类型
    ///  S2C_ 服务器发给客户端的消息
    ///  C2S_ 客户端发给服务器的消息
    /// </summary>
    public enum TrainMsgTypes : byte
    {
        /// <summary>
        /// 确认收到 以消息ID为唯一标志
        /// </summary>
        ALL_ACK = 0x01,

        #region 服务器 -> 客户端  A到C

        /// <summary>
        /// 训练启动信息（包含训练难度、配置文件等信息）
        /// </summary>
        S2C_TrainStartInfo = 0xA1,
        /// <summary>
        /// 解决解说视频还没完，游戏就开始的问题，游戏加载完成等信号再开始
        /// </summary>
        S2C_StartGame = 0xA2,
        /// <summary>
        /// 暂停
        /// </summary>
        S2C_Pause = 0xA3,

        /// <summary>
        /// 继续
        /// </summary>
        S2C_Resume = 0xA4,

        /// <summary>
        /// 重做
        /// </summary>
        S2C_Redo = 0xA5,
        /// <summary>
        /// 退出
        /// </summary>
        S2C_Exit = 0xA6,
        /// <summary>
        /// 分析后的设备数据：比如步时 步幅 对称性等 用于显示计算
        /// </summary>
        S2C_DeviceData = 0xA7,
        /// <summary>
        /// 设备状态
        /// </summary>
        S2C_DeviceState = 0xA8,
        /// <summary>
        /// 控制训练消息 比如触地 离地
        /// </summary>
        S2C_Controll = 0xAA,
        /// <summary>
        /// 训练简报
        /// </summary>
        S2C_TrainResult = 0xAB,
        /// <summary>
        /// 训练排行榜
        /// </summary>
        S2C_Rank = 0xAC,

        #endregion

        #region 客户端 -> 服务器  D到F

        //-------------------------------------------------------------------------------------客户端

        /// <summary>
        /// 客户端心跳 - 未启用
        /// </summary>
        C2S_Heartbeat = 0xD1,
        /// <summary>
        /// 训练加载进度
        /// </summary>
        C2S_LoadProgress = 0xD2,
        /// <summary>
        /// 训练加载完成
        /// </summary>
        C2S_LoadDone = 0xD3,
        /// <summary>
        /// 训练加载错误
        /// </summary>
        C2S_LoadError = 0xD4,

        /// <summary>
        /// 训练完成
        /// </summary>
        C2S_Done = 0xD5,
        /// <summary>
        /// 退出训练
        /// </summary>
        C2S_Exit = 0xD6,
        C2S_HidePause = 0xD7,
        C2S_ShowPause = 0xD8,
        /// <summary>
        /// 更新训练排行
        /// </summary>
        C2S_UpdateTrainRank = 0xD9,

        /// <summary>
        /// 开始记录
        /// </summary>
        C2S_StartRecord = 0xDA,
        /// <summary>
        /// 回合结束
        /// </summary>
        C2S_RoundDone = 0xDB,



        /// <summary>
        /// 播放MIDI音乐：需要指定音乐ID（节拍器为0，音乐小节为1）、BPM
        /// </summary>
        C2S_PlayMIDI = 0xDC,
        /// <summary>
        /// 暂停MIDI音乐
        /// </summary>
        C2S_PauseMIDI = 0xDD,
        /// <summary>
        /// 停止MIDI音乐
        /// </summary>
        C2S_StopMIDI = 0xDE,
        /// <summary>
        /// 设置MIDI音乐速度
        /// </summary>
        C2S_SetMIDISpeed = 0xDF,

        /// <summary>
        /// 设备控制 启停-设置速度
        /// </summary>
        C2S_DeviceControl = 0xF0,

        #endregion
    }
}
