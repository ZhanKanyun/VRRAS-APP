using System;
using System.Collections.Generic;
using System.Linq;

namespace KT.TCP
{
    /// <summary>
    /// 需要确认回复的消息管理器
    /// </summary>
    public class NeedAckMsgManager
    {
        #region 单例

        private NeedAckMsgManager() { }
        public readonly static NeedAckMsgManager Instance = new NeedAckMsgManager();

        #endregion

        /// <summary>
        /// 需要确认的消息集合，未收到确认消息之前，会重发
        /// </summary>
        private List<TCPNeedAckMsg> NeedAckMsgs = new List<TCPNeedAckMsg>();

        #region 事件

        /// <summary>
        /// 告诉服务器或客户端重发消息
        /// </summary>
        public event EventHandler<TCPNeedAckMsg> OnNeedSend;

        /// <summary>
        /// 告诉服务器或客户端该消息重发多少次失败，让服务器或客户端自己决定怎么处理
        /// </summary>
        public event EventHandler<TCPNeedAckMsg> OnCountExit;

        #endregion

        #region 外部接口

        /// <summary>
        /// 添加一条待确认消息
        /// </summary>
        /// <param name="temp"></param>
        public void Add(TCPNeedAckMsg temp)
        {
            try
            {
                if (NeedAckMsgs.Count(o => o.Msg.Type == temp.Msg.Type) > 0)//不添加重复消息
                {
                    return;
                }
                temp.StartTick = DateTime.Now.Ticks;
                temp.SN = NeedAckMsgs.Count + 1;
                NeedAckMsgs.Add(temp);
            }
            finally
            {
            }
        }

        /// <summary>
        /// 重置组件
        /// </summary>
        public void Reset()
        {
            try
            {

                NeedAckMsgs.Clear();
            }
            finally
            {

            }
        }


        public void Update()
        {
            try
            {
                if (NeedAckMsgs.Count > 0)
                {
                    for (int i = 0; i < NeedAckMsgs.Count; i++)
                    {
                        var temp = NeedAckMsgs[i];
                        if (!temp.IsValid)
                        {
                            NeedAckMsgs.Remove(temp);
                        }
                    }
                    foreach (var NeedAckMsg in NeedAckMsgs)
                    {
                        var nowTicks = DateTime.Now.Ticks;
                        if (NeedAckMsg.LastSendTick == 0)
                        {
                            NeedAckMsg.LastSendTick = nowTicks - NeedAckMsg.Interval;//马上发一次
                        }
                        if (nowTicks - NeedAckMsg.LastSendTick > NeedAckMsg.Interval)
                        {
                            switch (NeedAckMsg.NeedAckMsgType)
                            {
                                case NeedAckMsgTypes.Infinite://无限发
                                    {
                                        NeedAckMsg.CountAlready += 1;
                                        NeedAckMsg.LastSendTick = DateTime.Now.Ticks;
                                        OnNeedSend?.Invoke(null, NeedAckMsg);
                                    }
                                    break;
                                case NeedAckMsgTypes.Count://发送几次之后不再发
                                    {
                                        NeedAckMsg.CountAlready += 1;
                                        NeedAckMsg.LastSendTick = DateTime.Now.Ticks;
                                        OnNeedSend?.Invoke(null, NeedAckMsg);
                                        if (NeedAckMsg.CountAlready >= NeedAckMsg.CountNeed)
                                        {
                                            NeedAckMsg.IsValid = false;
                                        }
                                    }
                                    break;
                                case NeedAckMsgTypes.CountExit://发送指定次数后没成功就提示连接断开
                                    {
                                        NeedAckMsg.CountAlready += 1;
                                        NeedAckMsg.LastSendTick = DateTime.Now.Ticks;
                                        OnNeedSend?.Invoke(null, NeedAckMsg);
                                        if (NeedAckMsg.CountAlready >= NeedAckMsg.CountNeed)
                                        {
                                            NeedAckMsg.IsValid = false;
                                            OnCountExit?.Invoke(null, NeedAckMsg);
                                        }
                                    }
                                    break;

                                default:
                                    break;
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //AddLog(ex);
                System.Diagnostics.Debug.WriteLine("重发检测异常：" + ex.Message);
            }
            finally
            {

            }
        }

        /// <summary>
        /// 通过ID确认某条消息收到回复
        /// </summary>
        /// <param name="id"></param>
        public void ACK(string id)
        {
            var temp = NeedAckMsgs.Find(o => o.Msg.ID.Trim() == id.Trim());
            if (temp != null && temp.IsValid)
            {
                temp.IsValid = false;
                temp.Callback?.Invoke();
            }
        }

        #endregion
    }

    /// <summary>
    /// 需要接收端确认收到的消息，间隔重发，一些重要消息使用
    /// </summary>
    public class TCPNeedAckMsg
    {
        /// <summary>
        /// 序号
        /// </summary>
        public int SN { get; set; }

        public TrainMsg Msg { get; set; }

        /// <summary>
        /// 进入队列时刻
        /// </summary>
        public long StartTick { get; set; }

        /// <summary>
        /// 发送成功的回调 需要接收端的确认收到消息
        /// </summary>
        public Action Callback { get; set; }

        /// <summary>
        /// 上次发送时刻
        /// </summary>
        public long LastSendTick { get; set; }

        /// <summary>
        /// 发送间隔
        /// </summary>
        public long Interval { get; set; } = 10000000;

        /// <summary>
        /// 需要发送次数
        /// </summary>
        public int CountNeed { get; set; } = 5;

        /// <summary>
        /// 已经发送次数
        /// </summary>
        public int CountAlready { get; set; }

        /// <summary>
        /// 是否有效 无效会马上回收
        /// </summary>
        public bool IsValid { get; set; } = true;

        /// <summary>
        /// 重发类型
        /// </summary>
        public NeedAckMsgTypes NeedAckMsgType = NeedAckMsgTypes.Infinite;
    }

    /// <summary>
    /// 重发机制类型
    /// </summary>
    public enum NeedAckMsgTypes
    {
        /// <summary>
        /// 无限发
        /// </summary>
        Infinite,
        /// <summary>
        /// 发送多少次后不再发
        /// </summary>
        Count,
        /// <summary>
        /// 发送多少次后没收到回复就不再发，并触发OnCountExit事件
        /// </summary>
        CountExit,
    }
}
