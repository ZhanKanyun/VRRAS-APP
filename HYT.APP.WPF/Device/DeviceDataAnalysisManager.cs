using CL.Common;
using HYT.APP.WPF.Device;
using KT.TCP.Train;
using Microsoft.VisualBasic.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KCL
{
    /// <summary>
    /// 设备数据记录分析管理器
    /// </summary>
    public class DeviceDataAnalysisManager
    {
        #region ■------------------ 单例

        private DeviceDataAnalysisManager()
        {

        }
        public static readonly DeviceDataAnalysisManager Instance = new DeviceDataAnalysisManager();

        #endregion

        #region ■------------------ 外部方法

        /// <summary>
        /// 开始记录数据
        /// </summary>
        public bool Start(GaitRecordModes mode = GaitRecordModes.StartAdd)
        {
            if (!DeviceManager.Instance.IsCommunication)
            {
                return false;
            }
            CurrentGaitRecord.Steps.Clear();
            CurrentGaitRecord.Mode = mode;
            CurrentGaitRecord.StartTime = DateTime.Now;
            CurrentGaitRecord.EndTime = DateTime.MinValue;
            CurrentGaitRecord.TimeAdd = 0;
            ListDeviceData.Clear();
            State = DeviceDataAnalysisManagerStates.Run;
            return true;
        }

        /// <summary>
        /// 记录数据 最大2000步
        /// </summary>
        /// <param name="data"></param>
        public void AddRecord(DeviceData current)
        {
            if (State == DeviceDataAnalysisManagerStates.Run)
            {
                GaitRecordStep currentStepRecord = new GaitRecordStep();

                //跳过抬起
                if (current.StepType == StepTypes.LeftUp || current.StepType == StepTypes.RightUp)
                {
                    return;
                }
                //最多记录2000步数据
                if (CurrentGaitRecord.Steps.Count >= 2000)
                {
                    return;
                }

                #region 〓〓〓〓〓〓〓 计算步长和步时 步宽
                bool isFirst = false;
                switch (current.StepType)
                {
                    case StepTypes.None:
                        break;
                    case StepTypes.LeftUp:
                        {
                            currentStepRecord.F = 1;
                        }
                        break;
                    case StepTypes.LeftGround://左脚触地
                        {
                            currentStepRecord.F = 1;

                            //左脚步长：不同侧足跟两次触地距离
                            //左脚步时：不同侧足跟两次触地时长
                            if (ListDeviceData.Count(o => o.StepType == StepTypes.RightGround) > 0)
                            {
                                var lastRightGround = ListDeviceData.Last(o => o.StepType == StepTypes.RightGround);
                                currentStepRecord.BC = JSStepLength(lastRightGround.RightFoot.Point.Y, current.LeftFoot.Point.Y);
                                currentStepRecord.BS = JSStepTime(lastRightGround.Time, current.Time);


                                currentStepRecord.BK = JSStepWidth(lastRightGround.RightFoot.Point.X, current.LeftFoot.Point.X);



                                //如果暂停又继续
                                if (PauseTime != DateTime.MinValue && ContinueTime != DateTime.MinValue)
                                {
                                    if (CurrentGaitRecord.StepTimeL_Average <= 0)//之前无记录
                                    {
                                        PauseTime = DateTime.MinValue;
                                        ContinueTime = DateTime.MinValue;
                                        return;//实际上不能要，坐标在变，步长无法计算
                                        // 右脚踩了之后，暂停继续，然后踩了左脚，需要减去暂停的时间 。如果是还没走的时候就暂停继续，会导致第一部步时为负数，排除这种情况
                                        //注意：需要考虑多次点击暂停继续：暂停之后没有走下一步，暂停标记就不能清空
                                        if (PauseTime>= lastRightGround.Time&&ContinueTime<= current.Time)
                                        {
                                            currentStepRecord.BS -= (int)Math.Round((ContinueTime - PauseTime).TotalMilliseconds);
                                        }
                                       // return;//抛弃暂停继续后且之前无记录的左脚第一次触地，比如 1.右 暂停继续 2.左
                                        
                                    }
                                    else//之前有记录，取左脚平均步时
                                    {
                                        currentStepRecord.BS = CurrentGaitRecord.StepTimeL_Average;
                                        currentStepRecord.BC = CurrentGaitRecord.StepLengthL_Average;
                                    }
                                    PauseTime = DateTime.MinValue;
                                    ContinueTime = DateTime.MinValue;
                                }
                            }
                            else//第一步不要
                            {
                                isFirst = true;
                            }
                        }
                        break;
                    case StepTypes.RightUp:
                        {
                            currentStepRecord.F = 1;
                        }
                        break;
                    case StepTypes.RightGround://右脚触地
                        {
                            currentStepRecord.F = 2;

                            //右脚步长：不同侧足跟两次触地距离
                            //右脚步时：不同侧足跟两次触地时长
                            if (ListDeviceData.Count(o => o.StepType == StepTypes.LeftGround) > 0)
                            {
                                var lastLeftGround = ListDeviceData.Last(o => o.StepType == StepTypes.LeftGround);
                                currentStepRecord.BC = JSStepLength(lastLeftGround.LeftFoot.Point.Y, current.RightFoot.Point.Y);
                                currentStepRecord.BS = JSStepTime(lastLeftGround.Time, current.Time);
        

                                currentStepRecord.BK = JSStepWidth(lastLeftGround.LeftFoot.Point.X, current.RightFoot.Point.X);

                                //如果暂停又继续 ：
                                if (PauseTime != DateTime.MinValue && ContinueTime != DateTime.MinValue)
                                {
                                    if (CurrentGaitRecord.StepTimeR_Average <= 0)//之前无记录 
                                    {
                                        PauseTime = DateTime.MinValue;
                                        ContinueTime = DateTime.MinValue;
                                        return;//实际上不能要，坐标在变，步长无法计算
                                        // 左脚踩了之后，暂停继续，然后踩了右脚，需要减去暂停的时间 。如果是还没走的时候就暂停继续，会导致第一部步时为负数，排除这种情况
                                        //注意：需要考虑多次点击暂停继续：暂停之后没有走下一步，暂停标记就不能清空
                                        if (PauseTime >= lastLeftGround.Time && ContinueTime <= current.Time)
                                        {
                                            currentStepRecord.BS -= (int)Math.Round((ContinueTime - PauseTime).TotalMilliseconds);
                                        }
                                    }
                                    else//之前有记录，取右脚平均步时
                                    {
                                        currentStepRecord.BS = CurrentGaitRecord.StepTimeR_Average;
                                        currentStepRecord.BC = CurrentGaitRecord.StepLengthR_Average;
                                    }

                                    PauseTime = DateTime.MinValue;
                                    ContinueTime = DateTime.MinValue;
                                }
                            }
                            else//第一步不要
                            {
                                isFirst = true;
                            }
                        }
                        break;
                    default:
                        break;
                }

                #endregion


                GaitRecordStep lastStepRecord = null;
                if (CurrentGaitRecord.Steps.Count > 0)
                {
                    lastStepRecord = CurrentGaitRecord.Steps.Last();
                }

                #region 〓〓〓〓〓〓〓 计算步幅：同侧足跟两次触地距离 = 本次步长+上次步长

                if (lastStepRecord != null)
                {
                    currentStepRecord.BF = currentStepRecord.BC + lastStepRecord.BC;
                }
                else
                {
                    currentStepRecord.BF = currentStepRecord.BC * 2;// 第一步
                }

                #endregion

                #region 〓〓〓〓〓〓〓 计算步态周期：同侧足两次触地时长 = 本次步时+上次步时

                if (lastStepRecord != null)
                {
                    currentStepRecord.T = currentStepRecord.BS + lastStepRecord.BS; ;
                }
                else
                {
                    currentStepRecord.T = currentStepRecord.BS * 2;// 第一步
                }

                #endregion

                #region 〓〓〓〓〓〓〓 计算步频/节律：单位时间内的步数：本项目中节律 rhythm = 60 / T x 2 （次 / 分）

                if (currentStepRecord.BS != 0)
                {
                    currentStepRecord.BP = (int)(60d / ((double)currentStepRecord.BS / 1000d));
                }
                else
                {
                    currentStepRecord.BP = 0;
                }


                #endregion

                #region 〓〓〓〓〓〓〓 计算对称性：步态周期中右足触地到下一个左足触地的时间差/左足触地到下一个右足触地的时间差，即：对称性 = t1 / t2 x 100%

                var lastStep = CurrentGaitRecord.Steps.FindLast(o => o.F == 1);
                if (currentStepRecord.F == 1) lastStep = CurrentGaitRecord.Steps.FindLast(o => o.F == 2);

                if (lastStep != null && currentStepRecord.BS != 0 && lastStep.BS != 0)
                {
                    if (currentStepRecord.F == 1)
                    {
                        double aa = (double)currentStepRecord.BS / (double)lastStep.BS;
                        currentStepRecord.DCX = (int)Math.Round(aa * 100d);
                    }
                    else
                    {
                        double aa = (double)lastStep.BS / (double)currentStepRecord.BS;
                        currentStepRecord.DCX = (int)Math.Round(aa * 100d);
                    }
                }
                else
                {
                    currentStepRecord.DCX = 100;
                }

                #endregion


                if (!isFirst)//第一步不要
                {
                    if (CurrentGaitRecord.Mode == GaitRecordModes.StartAdd)
                    {
                        var pass = (int)(DateTime.Now - LastAddRecordTime).TotalMilliseconds;
                        CurrentGaitRecord.TimeAdd += pass;//时间累计
                        LastAddRecordTime = DateTime.Now;
                    }

                    currentStepRecord.I = CurrentGaitRecord.Steps.Count + 1;
                    currentStepRecord.Time = current.Time.Ticks;
                    CurrentGaitRecord.Steps.Add(currentStepRecord);

                    BrowserManager.Instance.ExecuteJSAsync($"API_CSharp.accessdataupdata('评测数据更新')");

                    AppManager.Instance.Output($"【数据记录分析】{currentStepRecord}");
                }
                else
                {
                    LastAddRecordTime = DateTime.Now;
                    AppManager.Instance.Output($"【数据记录分析】丢弃 = {currentStepRecord}");
                }

                if (ListDeviceData.Count > 20)
                {
                    ListDeviceData.RemoveAt(0);
                }
                ListDeviceData.Add(current);
            }
        }

        /// <summary>
        /// 上一次记录时间
        /// </summary>
        public DateTime LastAddRecordTime { get; private set; } = DateTime.MinValue;

        /// <summary>
        /// 暂停时间
        /// </summary>
        public DateTime PauseTime = DateTime.MinValue;

        /// <summary>
        /// 继续时间
        /// </summary>
        public DateTime ContinueTime = DateTime.MinValue;

        /// <summary>
        /// 暂停
        /// </summary>
        public void Pause()
        {
            if (State == DeviceDataAnalysisManagerStates.Run)
            {
                if (PauseTime== DateTime.MinValue)//连续暂停继续，没有走下一步，不能覆盖第一次暂停时间
                {
                    PauseTime = DateTime.Now;
                }
              
                State = DeviceDataAnalysisManagerStates.Pause;
            }
        }

        /// <summary>
        /// 继续
        /// </summary>
        public void Continue()
        {
            if (State == DeviceDataAnalysisManagerStates.Pause)
            {
                ContinueTime = DateTime.Now;
                State = DeviceDataAnalysisManagerStates.Run;
            }
        }

        /// <summary>
        /// 停止记录数据
        /// </summary>
        public GaitRecord Stop()
        {
            if (State != DeviceDataAnalysisManagerStates.Stop)
            {
                CurrentGaitRecord.EndTime = DateTime.Now;

                State = DeviceDataAnalysisManagerStates.Stop;

                return JsonConvert.DeserializeObject<GaitRecord>(JsonConvert.SerializeObject(CurrentGaitRecord));
            }
            return null;
        }

        /// <summary>
        /// 停止记录数据-web调用
        /// </summary>
        public string StopAccess()
        {

            if (State != DeviceDataAnalysisManagerStates.Stop)
            {
                CurrentGaitRecord.EndTime = DateTime.Now;

                State = DeviceDataAnalysisManagerStates.Stop;

                return JsonConvert.SerializeObject(CurrentGaitRecord);
            }
            return "";
        }

        /// <summary>
        /// 返回数据-web调用
        /// </summary>
        public string GetCurrentGaitRecord()
        {
            return JsonConvert.SerializeObject(CurrentGaitRecord);
        }

        #endregion

        #region ■------------------ 状态

        private DeviceDataAnalysisManagerStates _state = DeviceDataAnalysisManagerStates.Stop;

        /// <summary>
        /// 记录器状态
        /// </summary>
        public DeviceDataAnalysisManagerStates State
        {
            get { return _state; }
            set
            {
                _state = value;
                switch (_state)
                {
                    case DeviceDataAnalysisManagerStates.Stop:
                        break;
                    case DeviceDataAnalysisManagerStates.Run:
                        {
                            LastAddRecordTime = DateTime.Now;
                        }
                        break;
                    case DeviceDataAnalysisManagerStates.Pause:
                        break;
                    default:
                        break;
                }


            }
        }

        #endregion

        #region ■------------------ 数据记录

        /// <summary>
        /// 步态记录 只记录触地
        /// </summary>
        public GaitRecord CurrentGaitRecord = new GaitRecord();

        /// <summary>
        /// 设备数据记录 触地和离地都会记录
        /// </summary>
        public List<DeviceData> ListDeviceData = new List<DeviceData>();

        /// <summary>
        /// 计算步长 受坐标模式影响 厘米
        /// </summary>
        /// <param name="y1">之前</param>
        /// <param name="y2">当前</param>
        /// <returns></returns>  
        private int JSStepLength(int y1, int y2)
        {
            var gridCount = 0;
            DeviceCoordinateModes deviceCoordinateModes = (DeviceCoordinateModes)ConfigManager.Instance.SSetting.CoordinateMode;
            switch (deviceCoordinateModes)
            {
                case DeviceCoordinateModes.OriginRightTop://Y坐标从大到小
                    {

                        if (y1 > y2)
                        {
                            gridCount = y1 - y2;
                        }
                        else
                        {
                            gridCount = y1 + 128 - y2;
                        }
                    }
                    break;
                case DeviceCoordinateModes.OriginLeftBottom://Y坐标从小到大
                    {
                        if (y1 < y2)
                        {
                            gridCount = y2 - y1;
                        }
                        else
                        {
                            gridCount = y2 + 128 - y1;
                        }
                    }
                    break;
                default:
                    break;
            }

            return (int)Math.Round(gridCount * ConfigManager.Instance.SSetting.CoordinateYToDistance / 10f);
        }

        private int JSStepWidth(int xold, int xnew)
        {
            var gridCount = Math.Abs(xnew - xold);


            return (int)Math.Round(gridCount * ConfigManager.Instance.SSetting.CoordinateXToDistance / 10f);
        }

        /// <summary>
        /// 计算步时
        /// </summary>
        /// <param name="last">之前</param>
        /// <param name="current">当前</param>
        /// <returns></returns>
        private int JSStepTime(DateTime last, DateTime current)
        {
            return (int)Math.Round((current - last).TotalMilliseconds);
        }




        #endregion
    }

    /// <summary>
    /// 设备数据记录分析管理器状态
    /// </summary>
    public enum DeviceDataAnalysisManagerStates
    {
        Stop = 0,
        Run,
        Pause
    }
}
