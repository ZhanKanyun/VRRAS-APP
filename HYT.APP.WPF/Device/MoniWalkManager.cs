using CL.Common;
using HYT.APP.WPF.Device;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

namespace KCL
{
    /// <summary>
    /// 模拟行走管理器
    /// </summary>
    public class MoniWalkManager
    {
        #region ■------------------ 单例

        private MoniWalkManager()
        {
            var temp = new MoniWalkDeviceData() { Index = 0, DelayMillSecond = 400, Name = "左脚抬起" };
            temp.LeftFoot.IsGround = 0;
            temp.RightFoot.IsGround = 1;
            ListMoNiSteps.Add(temp);

            temp = new MoniWalkDeviceData() { Index = 1, DelayMillSecond = 100, Name = "左脚踩下" };
            temp.LeftFoot.IsGround = 1;
            temp.RightFoot.IsGround = 1;
            ListMoNiSteps.Add(temp);

            temp = new MoniWalkDeviceData() { Index = 2, DelayMillSecond = 400, Name = "右脚抬起" };
            temp.LeftFoot.IsGround = 1;
            temp.RightFoot.IsGround = 0;
            ListMoNiSteps.Add(temp);

            temp = new MoniWalkDeviceData() { Index = 3, DelayMillSecond = 100, Name = "右脚踩下" };
            temp.LeftFoot.IsGround = 1;
            temp.RightFoot.IsGround = 1;
            ListMoNiSteps.Add(temp);

            StartTick();
        }
        public static readonly MoniWalkManager Instance = new MoniWalkManager();

        #endregion

        #region ■------------------ 字段相关

        /// <summary>
        /// 模拟行走数据序列
        /// </summary>
        public List<MoniWalkDeviceData> ListMoNiSteps { get; private set; }= new List<MoniWalkDeviceData>();


        public int StepCount
        {
            get { return ListMoNiSteps.Count; }
        }

        /// <summary>
        /// 步态周期倍率
        /// </summary>
        public float WalkTimeMultiplier { get; set; } = 1;


        /// <summary>
        /// 当前步
        /// </summary>
        public MoniWalkDeviceData CurrentStep { get; private set; }

        /// <summary>
        /// 上一个触地坐标 不分左右
        /// </summary>
        public FootPoint LastGroundPoint { get; private set; } =new FootPoint();
        public MoniWalkDeviceData LastGround_L { get; private set; } = new MoniWalkDeviceData();
        public MoniWalkDeviceData LastGround_R { get; private set; } = new MoniWalkDeviceData();

        /// <summary>
        /// 事件：当前步改变
        /// </summary>
        public event EventHandler<MoniWalkDeviceData> OnNextStep;

        /// <summary>
        /// 模式
        /// </summary>
        public MoniWalkModes Mode = MoniWalkModes.None;

        #endregion

        /// <summary>
        /// 左脚走的距离 CM
        /// </summary>
        public int LeftWalkDistance = 50;
        /// <summary>
        /// 右脚走的距离 CM
        /// </summary>
        public int RightWalkDistance = 50;

        #region ■------------------ 外部方法

        public void SetParam(List<int> walkTimes, List<int> walkDistances)
        {
            if (walkTimes.Count != 4 || walkDistances.Count != 2)
            {
                return;
            }

            LeftWalkDistance = walkDistances[0];
            RightWalkDistance = walkDistances[1];

            foreach (var item in ListMoNiSteps)
            {
                int index = ListMoNiSteps.IndexOf(item);
                item.HoldMillSecond = walkTimes[index];
            }
        }

        /// <summary>
        /// 自动：从左脚开始
        /// </summary>
        public void Auto()
        {
            try
            {
                if (Mode!= MoniWalkModes.Auto)
                {
                    Reset();
                    Mode = MoniWalkModes.Auto;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        /// <summary>
        /// 单步：指定脚抬起自动放下
        /// </summary>
        public void Single(bool isleft)
        {
            try
            {
                if (Mode!= MoniWalkModes.Single)
                {
                    //重置 并指定脚抬起
                    Reset();
                    NextStep(isleft ? 0 : 2);
                    Mode = MoniWalkModes.Single;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        /// <summary>
        /// 手动模式
        /// </summary>
        public void ManualNextStep(bool isSkipTaiQi=false)
        {
            try
            {
                if (isSkipTaiQi)
                {
                    if (CurrentStep == null)
                    {
                        NextStep(1);
                    }
                    else
                    {
                        NextStep(CurrentStep.Index + 1);
                        if (CurrentStep.Index % 2 == 0)
                        {
                            NextStep(CurrentStep.Index + 1);
                        }
                    }
                }
                else
                {
                    NextStep(CurrentStep == null ? 0 : CurrentStep.Index + 1);
                }

                Mode = MoniWalkModes.Manual;
            }
            catch (Exception ex)
            {

                LogHelper.Error(ex);
            }
        }

         /// <summary>
         /// 停止
         /// </summary>
        public void Reset()
        {
            
            //重置
            if (Mode != MoniWalkModes.None)
            {
                CurrentStep = null;
                Mode = MoniWalkModes.None;
            }
        }

        public void Stop()
        {
            StopTick();
        }

        #endregion

        #region ■------------------ 主计时器3：不能进行耗时任务 间隔10毫秒

        CancellationTokenSource _tokenSource3 = new CancellationTokenSource();
        Task _timerTask3;

        /// <summary>
        /// 主计时器最后一次Tick 时间
        /// </summary>
        public DateTime _lastTick3 = DateTime.MinValue;

        public Queue<double> _passRecord = new Queue<double>();
        private bool isJumpPass = false;

        /// <summary>
        /// 主计时器开始运行
        /// </summary>
        public void StartTick()
        {
            _lastTick3 = DateTime.Now;

            _timerTask3 = Task.Factory.StartNew(() => {
                while (true)
                {
                    var now = DateTime.Now;
                    var pass = (int)((now - _lastTick3).TotalMilliseconds);
                    _lastTick3 = now;

                    OnTick(null,pass);

                    if (_tokenSource3.IsCancellationRequested)
                    {
                        Thread.CurrentThread.Priority = ThreadPriority.Normal;
                        return;
                    }
                    //稳住时间
                    if (isJumpPass)
                    {
                        isJumpPass = false;
                        if (pass >= 9)
                        {
                            _passRecord.Enqueue(pass);
                        }
                    }
                    else
                    {
                        _passRecord.Enqueue(pass);
                    }

                    if (_passRecord.Count >= 2)
                    {
                        int sleep = (int)(Math.Floor(30d - _passRecord.Sum()));
                        if (sleep <= 0) sleep = 1;

                        Thread.Sleep(sleep);

                        _passRecord.Clear();
                        isJumpPass = true;
                    }
                    else
                    {
                        Thread.Sleep(8);
                    }
                }
            }, _tokenSource3.Token);
        }

        public void StopTick()
        {
            _tokenSource3.Cancel();
        }

        #endregion


        /// <summary>
        /// Update
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTick(object sender, int e)
        {
            try
            {
                var nowTime = DateTime.Now;
                switch (Mode)
                {
                    case MoniWalkModes.None:
                        break;
                    case MoniWalkModes.Single:
                        {
                            if (CurrentStep != null)
                            {
                                //左脚抬起或右脚抬起的时候要自动放下
                                if (CurrentStep.Index==0|| CurrentStep.Index == 2)
                                {
                                    var passTime = nowTime - CurrentStep.StartTime;
                                    if (passTime.TotalMilliseconds >= CurrentStep.HoldMillSecond * WalkTimeMultiplier)
                                    {
                                        CurrentStep.StartTime = DateTime.MinValue;
                                        NextStep(CurrentStep.Index + 1);
                                    }
                                }
                            }
                        }
                        break;
                    case MoniWalkModes.Auto:
                        {
                            if (CurrentStep == null)
                            {
                                NextStep(0);
                            }
                            else
                            {
                                var passTime = nowTime - CurrentStep.StartTime;
                                if (passTime.TotalMilliseconds >= CurrentStep.HoldMillSecond * WalkTimeMultiplier)
                                {
                                    CurrentStep.StartTime = DateTime.MinValue;
                                    NextStep(CurrentStep.Index + 1);
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }

        }

        /// <summary>
        /// 设置速度
        /// </summary>
        public void SetSpeed(float speed)
        {
            WalkTimeMultiplier = speed;
        }

        /// <summary>
        /// 下一步
        /// </summary>
        private void NextStep(int index)
        {
            if (index < StepCount)
            {
                CurrentStep = ListMoNiSteps[index];
                CurrentStep.StartTime = DateTime.Now;

                #region 〓〓〓〓〓〓〓 触地坐标模拟
                

                Random random = new Random();
                var walkRan = random.Next(5);//随机厘米数

                if (CurrentStep.Index == 0)//左脚离地
                {
                    CurrentStep.LeftFoot.SetPoint(LastGround_L.LeftFoot);
                    CurrentStep.RightFoot.SetPoint(LastGround_R.RightFoot);
                }
                else if(CurrentStep.Index == 1)//左脚触地
                {
                    var gridCount = (int)Math.Round((walkRan + LeftWalkDistance - 2) * 10 / ConfigManager.Instance.SSetting.CoordinateYToDistance);

                    switch ((DeviceCoordinateModes)ConfigManager.Instance.SSetting.CoordinateMode)
                    {
                        case DeviceCoordinateModes.OriginRightTop://Y坐标由大变小
                            {
                                //计算左脚坐标
                                CurrentStep.LeftFoot.Point.Y = (LastGroundPoint.Y - gridCount + 128) % 128;
                                CurrentStep.LeftFoot.Point.X = random.Next(4) + 18;
                            }
                            break;
                        case DeviceCoordinateModes.OriginLeftBottom://Y坐标由小变大
                            {
                                //计算左脚坐标
                                CurrentStep.LeftFoot.Point.Y = (LastGroundPoint.Y + gridCount) % 128;
                                CurrentStep.LeftFoot.Point.X = random.Next(4) + 8;
                            }
                            break;
                        default:
                            break;
                    }
                    //右脚坐标不变
                    CurrentStep.RightFoot.SetPoint(LastGround_R.RightFoot);
                    LastGroundPoint = CurrentStep.LeftFoot.Point;
                    LastGround_L = CurrentStep;
                }
                else if (CurrentStep.Index == 2)//右脚离地
                {
                    CurrentStep.LeftFoot.SetPoint(LastGround_L.LeftFoot);
                    CurrentStep.RightFoot.SetPoint(LastGround_R.RightFoot);
                }
                else if (CurrentStep.Index == 3)//右脚触地
                {
                    var gridCount = (int)Math.Round((walkRan + RightWalkDistance - 2) * 10 / ConfigManager.Instance.SSetting.CoordinateYToDistance);
                    switch ((DeviceCoordinateModes)ConfigManager.Instance.SSetting.CoordinateMode)
                    {
                        case DeviceCoordinateModes.OriginRightTop:
                            {
                                //计算右脚坐标
                                CurrentStep.RightFoot.Point.Y = (LastGroundPoint.Y - gridCount + 128) % 128;
                                CurrentStep.RightFoot.Point.X = random.Next(4) + 8;
                            }
                            break;
                        case DeviceCoordinateModes.OriginLeftBottom:
                            {
                                //计算右脚坐标
                                CurrentStep.RightFoot.Point.Y = (LastGroundPoint.Y + gridCount) % 128;
                                CurrentStep.RightFoot.Point.X = random.Next(4) + 18;
                            }
                            break;
                        default:
                            break;
                    }
                    //左脚坐标不变
                    CurrentStep.LeftFoot.SetPoint(LastGround_L.LeftFoot);
                    LastGroundPoint = CurrentStep.RightFoot.Point;
                    LastGround_R = CurrentStep;

                }

                #endregion


                OnNextStep?.Invoke(null, CurrentStep);
            }
            else
            {
                NextStep(0);//从第一步开始
            }
        }


    }

    /// <summary>
    /// 模拟行走数据
    /// </summary>
    public class MoniWalkDeviceData : DeviceData
    {
        public int Index { get; set; }

        public string Name { get; set; }

        public DateTime StartTime { get; set; } = DateTime.MinValue;

        /// <summary>
        /// 【未启用】延迟毫秒数
        /// </summary>
        public int DelayMillSecond { get; set; }

        /// <summary>
        /// 持续毫秒数 完毕进入下一步
        /// </summary>
        public int HoldMillSecond { get; set; }

        public override string ToString()
        {
            return $"{Index} - {Name} - {HoldMillSecond} 左脚坐标={LeftFoot.Point} 右脚坐标={RightFoot.Point} ";
        }
    }

    /// <summary>
    /// 模拟行走模式
    /// </summary>
    public enum MoniWalkModes
    {
        /// <summary>
        /// 无模式 无状态
        /// </summary>
        None = 0,
        /// <summary>
        /// 手动模式
        /// </summary>
        Manual,
        /// <summary>
        /// 半自动单步模式  脚抬起放下
        /// </summary>
        Single,
        /// <summary>
        /// 自动模式 连续模式
        /// </summary>
        Auto,
    }
}
