using System;
using System.Collections.Generic;

namespace KT.TCP
{

    /// <summary>
    /// 任务队列
    /// </summary>
    public class TaskInfo
    {
        /// <summary>
        /// 客户信息
        /// </summary>
        public ClientInfo Client { get; set; }

        /// <summary>
        /// 封包
        /// </summary>
        public Packet Packet { get; set; }
    }

    /// <summary>
    /// 客户信息
    /// </summary>
    public class ClientInfo
    {
        /// <summary>
        /// 连接id
        /// </summary>
        public IntPtr ConnId { get; set; }

        /// <summary>
        /// 封包数据
        /// </summary>
        public List<byte> PacketData { get; set; }

    }

    /// <summary>
    /// 客户信息
    /// </summary>
    public class ClientInfo<TDataType> : ClientInfo
    {
        /// <summary>
        /// 封包数据
        /// </summary>
        public new TDataType PacketData { get; set; }
    }

    /// <summary>
    /// 封包
    /// </summary>
    public class Packet
    {
        /// <summary>
        /// 封包类型
        /// </summary>
        public PacketType Type { get; set; }

        /// <summary>
        /// 数据
        /// </summary>
        public string Data { get; set; }
    }

    /// <summary>
    /// 封包类型
    /// </summary>
    public enum PacketType
    {
        /// <summary>
        /// 回显
        /// </summary>
        Echo = 1,
        /// <summary>
        /// 时间
        /// </summary>
        Time,
        /// <summary>
        /// 训练消息 都是非耗时任务，没有耗时任务
        /// </summary>
        TrainMsg
    }
}
