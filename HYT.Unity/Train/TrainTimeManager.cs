using System;
using System.Collections.Generic;
using System.Text;

namespace KT.TCP
{
    /// <summary>
    /// 训练时间管理器
    /// </summary>
    public class TrainTimeManager
    {
        #region ■------------------ 单例
        private TrainTimeManager() { }
        public static readonly TrainTimeManager Instance = new TrainTimeManager();

        #endregion

        #region ■------------------ 字段相关

        public long LastTick { get; set; }

        /// <summary>
        /// 总时长 单位毫秒
        /// </summary>
        public float TotalTime { get; set; }

        /// <summary>
        /// 剩余时长  单位毫秒
        /// </summary>
        public float RemainingTime { get; set; }

        /// <summary>
        /// 开始时刻
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 是否休息
        /// </summary>
        public bool IsRest { get; set; }

        /// <summary>
        /// 休息时长 秒
        /// </summary>
        public int RestScenonds { get; set; }

        /// <summary>
        /// 休息间隔 秒
        /// </summary>
        public int RestInterval { get; set; }

        /// <summary>
        /// 多久没休息了 毫秒
        /// </summary>
        public float RestIntervalAlready { get; set; }

        /// <summary>
        /// 已经休息了多久 毫秒
        /// </summary>
        public float RestAlready { get; set; }

        /// <summary>
        /// 显示结果时长倒计时 毫秒
        /// </summary>
        public float ShowReslutCountDown { get; set; }

        /// <summary>
        /// UI 展示信息
        /// </summary>
        public string Info
        {
            get
            {
                return ToTimeString((RemainingTime / 1000f));//(RemainingTime / 1000f).ToTimeString();
            }
        }

        public string ToTimeString(float value, string fmt = "")
        {
            var deffmt = value > 86400 ? "d\\.hh\\:mm\\:ss" : value > 3600 ? "h\\:mm\\:ss" : "mm\\:ss";
            var format = string.IsNullOrWhiteSpace(fmt) ? deffmt : fmt;
            System.TimeSpan ts = System.TimeSpan.FromSeconds(value);
            return ts.ToString(format);
        }

        /// <summary>
        /// 上次更新UI时间
        /// </summary>
        public float LastTimeChange;

        private float _Accelerator = 1;
        /// <summary>
        /// 加速器，方便调试 最大10 最小0.1
        /// </summary>
        public float Accelerator
        {
            get { return _Accelerator; }
            set
            {
                if (value < 0.1f) value = 0.1f;
                if (value > 10) value = 10;
                _Accelerator = value;
            }
        }

        /// <summary>
        /// 休息倒计时
        /// </summary>
        public float RestCountDown;

        #endregion

        #region ■------------------ 初始化

        /// <summary>
        /// 是否初始化
        /// </summary>
        public bool IsInit { get; set; } = false;
        /// <summary>
        /// 开始
        /// </summary>
        /// <param name="seconds">训练时长，秒</param>
        /// <param name="isRest">是否休息</param>
        /// <param name="restInterval">休息间隔多少秒</param>
        /// <param name="restSeconds">每次休息多少秒</param>
        public void Init(long seconds, bool isRest = false, int restInterval = 0, int restSeconds = 0)
        {
            if (!IsInit)
            {
                IsInit = true;
                IsRest = isRest;
                RestInterval = restInterval;
                RestScenonds = restSeconds;
                RestIntervalAlready = 0;
                TotalTime = seconds * 1000f;
                RemainingTime = TotalTime;
                LastTimeChange = RemainingTime;
                StartTime = DateTime.Now;
                LastTick = DateTime.Now.Ticks;
                Accelerator = 1;
            }

        }

        #endregion

        #region ■------------------ 外部接口（未使用）

        /// <summary>
        /// 减少训练时间
        /// </summary>
        /// <param name="passSeconds"></param>
        public void ReduceTime(float passSeconds)
        {
            RemainingTime -= passSeconds * 1000f * Accelerator;
            if (RemainingTime <= 0)//结束
            {
                RemainingTime = 0;
                OnTimeOver?.Invoke(null, 0);
            }
            else//未结束
            {
                if (LastTimeChange - RemainingTime > 1000)
                {
                    LastTimeChange = RemainingTime * Accelerator;
                    OnSecondChange?.Invoke(null, 0);
                }

                //休息检测
                if (IsRest && RestInterval > 0 && RestScenonds > 0)
                {
                    RestIntervalAlready += passSeconds * 1000f * Accelerator;
                    if (RestIntervalAlready > RestInterval * 1000f)
                    {
                        RestAlready = 0;
                        RestIntervalAlready = 0;
                        OnRestStart?.Invoke(null, 0);
                    }
                }
            }
        }

        /// <summary>
        /// 减少显示结果时间
        /// </summary>
        public void ReduceReslutTime(float passSeconds)
        {
            ShowReslutCountDown -= passSeconds * 1000f;
            if (ShowReslutCountDown <= 0)
            {
                ShowReslutCountDown = 0;
                OnResultEnd?.Invoke(null, 0);
            }
        }


        /// <summary>
        /// 增加休息时间
        /// </summary>
        public void AddRestTime(float passSeconds)
        {
            if (IsRest && RestInterval > 0 && RestScenonds > 0)
            {
                RestAlready += passSeconds * 1000f;
                if (RestAlready >= RestScenonds * 1000)
                {
                    RestAlready = 0;
                    RestIntervalAlready = 0;
                    OnRestEnd?.Invoke(null, 0);
                }
                else
                {
                    int countDown = (int)Math.Ceiling((RestScenonds * 1000f - RestAlready) / 1000f);
                    if (countDown != RestCountDown)
                    {
                        RestCountDown = countDown;
                        OnRestTimeChange?.Invoke(null, (int)RestCountDown);
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// 秒数变化
        /// </summary>
        public event EventHandler<int> OnSecondChange;

        /// <summary>
        /// 训练倒计时结束
        /// </summary>
        public event EventHandler<int> OnTimeOver;

        /// <summary>
        /// 休息开始
        /// </summary>
        public event EventHandler<int> OnRestStart;

        /// <summary>
        /// 休息倒计时改变
        /// </summary>
        public event EventHandler<int> OnRestTimeChange;

        /// <summary>
        /// 休息结束
        /// </summary>
        public event EventHandler<int> OnRestEnd;

        /// <summary>
        /// 结果显示倒计时结束
        /// </summary>
        public event EventHandler<int> OnResultEnd;
    }
}
