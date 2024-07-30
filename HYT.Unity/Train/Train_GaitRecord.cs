using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KT.TCP.Train
{
    public class GaitRecord
    {
        /// <summary>
        /// 时间模式
        /// </summary>
        public GaitRecordModes Mode = GaitRecordModes.StartAdd;


        public override string ToString()
        {
            return $"\r\n" +
                $"〓〓 总步数={StepCount}  左脚步数={StepCountL}  右脚步数={StepCountR} \r\n" +
                $"〓〓 总时间={TimeAll}秒  总距离={Distance}厘米\r\n" +
                $"〓〓 平均速度={Speed_Average}米/秒  平均步态周期={T_Average}秒  平均步频={Rhythm_Average}次/分  \r\n" +
                $"〓〓 平均步长={StepLength_Average}cm  平均左步长={StepLengthL_Average}cm  平均右步长={StepLengthR_Average}cm \r\n" +
                $"〓〓 对称性={Symm_Average}%";
        }
        /// <summary>
        /// 每一步的记录
        /// </summary>
        public List<GaitRecordStep> Steps = new List<GaitRecordStep>();


        /// <summary>
        /// 【模式2：可以暂停】积累时间 单位毫秒
        /// </summary>
        public int TimeAdd { get; set; }

        /// <summary>
        /// 【模式1：不可暂停】开始记录时间
        /// </summary>
        public DateTime StartTime { get; set; } = DateTime.MinValue;

        /// <summary>
        /// 【模式1：不可暂停】结束记录时间
        /// </summary>
        public DateTime EndTime { get; set; } = DateTime.MinValue;


        /// <summary>
        /// 平均速度：米每秒
        /// </summary>
        public float Speed_Average
        {
            get
            {
                if (TimeAll <= 0)
                {
                    return 0;
                }
                return (float)(Distance / 100f / TimeAll);
            }
        }

        /// <summary>
        /// 总时长：单位秒
        /// </summary>
        public int TimeAll
        {
            get
            {
                switch (Mode)
                {
                    case GaitRecordModes.StartEnd:
                        {
                            if (EndTime == DateTime.MinValue)
                            {
                                return (int)((DateTime.Now - StartTime).TotalSeconds);
                            }
                            else
                            {
                                return (int)((EndTime - StartTime).TotalSeconds);
                            }
                        }
                    case GaitRecordModes.StartAdd:
                        {
                            return (int)Math.Round(TimeAdd / 1000f);
                        }
                    default:
                        return TimeAdd;
                }

            }
        }

        /// <summary>
        /// 总距离：单位厘米
        /// </summary>
        public int Distance
        {
            get { return Steps.Sum(o => o.BC); }
        }

        /// <summary>
        /// 总步数
        /// </summary>
        public int StepCount
        {
            get { return Steps.Count; }
        }

        /// <summary>
        /// 左脚总步数
        /// </summary>
        public int StepCountL
        {
            get { return Steps.Count(o => o.F == 1); }
        }

        /// <summary>
        /// 右脚总步数
        /// </summary>
        public int StepCountR
        {
            get { return Steps.Count(o => o.F == 2); }
        }

        /// <summary>
        /// 平均步辐
        /// </summary>
        public int BF_Average
        {
            get
            {
                double bfAll = Steps.Sum(o => o.BF);
                if (StepCount <= 0 || bfAll == 0)
                {
                    return 0;
                }

                return (int)Math.Round((bfAll / StepCount));
            }
        }

        /// <summary>
        /// 平均步态周期=所有的步态周期时间总和除以步数  单位秒
        /// </summary>
        public int T_Average
        {
            get
            {
                var timeAll = Steps.Sum(o => o.T);
                if (Steps.Count <= 0 || timeAll == 0)
                {
                    return 0;
                }

                return (int)Math.Round((timeAll / 1000f / Steps.Count));
            }
        }

        /// <summary>
        /// 平均步频 ：次每分钟
        /// </summary>
        public int Rhythm_Average
        {
            get
            {
                if (Steps.Count <= 0 || TimeAll <= 0)
                {
                    return 0;
                }

                return (int)Math.Round((Steps.Count / (TimeAll / 60f)));
            }
        }

        /// <summary>
        /// 平均步长 单位厘米
        /// </summary>
        public int StepLength_Average
        {
            get
            {
                if (StepCount <= 0)
                {
                    return 0;
                }

                return (int)Math.Round((float)Distance / (float)StepCount);
            }
        }

        /// <summary>
        /// 左脚平均步长 单位厘米
        /// </summary>
        public int StepLengthL_Average
        {
            get
            {
                if (StepCountL <= 0)
                {
                    return 0;
                }
                int stepLengthLAll = Steps.Where(o => o.F == 1).Sum(o => o.BC);

                return (int)Math.Round((float)stepLengthLAll / (float)StepCountL);
            }
        }

        /// <summary>
        /// 右脚平均步长 单位厘米
        /// </summary>
        public int StepLengthR_Average
        {
            get
            {
                if (StepCountR <= 0)
                {
                    return 0;
                }
                int stepLengthRAll = Steps.Where(o => o.F == 2).Sum(o => o.BC);

                return (int)Math.Round((float)stepLengthRAll / (float)StepCountR);
            }
        }

        /// <summary>
        /// 平均步时 单位毫秒
        /// </summary>
        public int StepTime_Average
        {
            get
            {
                if (StepCount <= 0)
                {
                    return 0;
                }
                int stepTimeAll = Steps.Sum(o => o.BS);

                return (int)Math.Round((float)stepTimeAll / (float)StepCount);
            }
        }

        /// <summary>
        /// 左脚平均步时 单位毫秒
        /// </summary>
        public int StepTimeL_Average
        {
            get
            {
                if (StepCountL <= 0)
                {
                    return 0;
                }
                int stepTimeLAll = Steps.Where(o => o.F == 1).Sum(o => o.BS);

                return (int)Math.Round((float)stepTimeLAll / (float)StepCountL);
            }
        }

        /// <summary>
        /// 右脚平均步时 单位毫秒
        /// </summary>
        public int StepTimeR_Average
        {
            get
            {
                if (StepCountR <= 0)
                {
                    return 0;
                }
                int stepTimeRAll = Steps.Where(o => o.F == 2).Sum(o => o.BS);

                return (int)Math.Round((float)stepTimeRAll / (float)StepCountR);
            }
        }

        /// <summary>
        /// 平均步宽 单位厘米
        /// </summary>
        public int StepWidth_Average
        {
            get
            {
                if (StepCount <= 0)
                {
                    return 0;
                }
                var stepWidthAll = Steps.Sum(o => o.BK);

                return (int)Math.Round((float)stepWidthAll / (float)StepCount);
            }
        }

        /// <summary>
        /// 平均对称性=(右脚的总步时/右脚步数)/(左脚的总步时/左脚步数)
        /// </summary>
        public int Symm_Average
        {
            get
            {
                if (StepCountL == 0 || StepCountR == 0)
                {
                    return 0;
                }
                var timeL = Steps.Where(o => o.F == 1).Sum(o => o.BS);
                var timeR = Steps.Where(o => o.F == 2).Sum(o => o.BS);
                if (timeR == 0 || timeL == 0)
                {
                    return 0;
                }
                return (int)Math.Round((timeL / StepCountL * 1f) / (timeR / StepCountR * 1f) * 100f);
            }
        }

        /// <summary>
        /// 上一个步态周期的对称性
        /// </summary>
        public int Symm_LastOne
        {
            get
            {
                if (Steps.Count < 2)
                {
                    return 0;
                }
                var stepCountL = Steps.Last(o => o.F == 1);
                var stepCountR = Steps.Last(o => o.F == 2);

                return (int)Math.Round(stepCountR.BS / stepCountL.BS * 100f);
            }
        }
    }

    /// <summary>
    /// 发送给Unity的记录
    /// </summary>
    public class GaitRecordUnity
    {
        public GaitRecordUnity() { }
        public GaitRecordUnity(GaitRecord record)
        {
            var count = 10;
            if (record.StepCount < 10) count = record.StepCount;
            GaitRecordStep[] stepArray = new GaitRecordStep[count];
            record.Steps.CopyTo(record.StepCount - count, stepArray, 0, count);
            Steps.AddRange(stepArray);

            TimeAdd = record.TimeAdd;
            StartTime = record.StartTime;
            EndTime = record.EndTime;
            Speed_Average = record.Speed_Average;
            TimeAll = record.TimeAll;
            Distance = record.Distance;
            StepCount = record.StepCount;
            StepCountL = record.StepCountL;
            StepCountR = record.StepCountR;
            T_Average = record.T_Average;
            BF_Average = record.BF_Average;
            Rhythm_Average = record.Rhythm_Average;
            StepLength_Average = record.StepLength_Average;
            StepLengthL_Average = record.StepLengthL_Average;
            StepLengthR_Average = record.StepLengthR_Average;
            StepTime_Average = record.StepTime_Average;
            StepTimeL_Average = record.StepTimeL_Average;
            StepTimeR_Average = record.StepTimeR_Average;
            StepWidth_Average = record.StepWidth_Average;
            Symm_Average = record.Symm_Average;
            Symm_LastOne = record.Symm_LastOne;
        }
        public override string ToString()
        {
            return $"\r\n" +
                $"〓〓 总步数={StepCount}  左脚步数={StepCountL}  右脚步数={StepCountR} \r\n" +
                $"〓〓 总时间={TimeAll}秒  总距离={Distance}厘米\r\n" +
                $"〓〓 平均速度={Speed_Average}米/秒  平均步态周期={T_Average}秒  平均步频={Rhythm_Average}次/分  \r\n" +
                $"〓〓 平均步长={StepLength_Average}cm  平均左步长={StepLengthL_Average}cm  平均右步长={StepLengthR_Average}cm \r\n" +
                $"〓〓 对称性={Symm_Average}%";
        }
        /// <summary>
        /// 每一步的记录
        /// </summary>
        public List<GaitRecordStep> Steps = new List<GaitRecordStep>();


        /// <summary>
        /// 【模式2：可以暂停】积累时间 单位毫秒
        /// </summary>
        public int TimeAdd { get; set; }

        /// <summary>
        /// 【模式1：不可暂停】开始记录时间
        /// </summary>
        public DateTime StartTime { get; set; } = DateTime.MinValue;

        /// <summary>
        /// 【模式1：不可暂停】结束记录时间
        /// </summary>
        public DateTime EndTime { get; set; } = DateTime.MinValue;


        /// <summary>
        /// 平均速度：米每秒
        /// </summary>
        public float Speed_Average { get; set; }


        /// <summary>
        /// 总时长：单位秒
        /// </summary>
        public int TimeAll { get; set; }

        /// <summary>
        /// 总距离：单位厘米
        /// </summary>
        public int Distance { get; set; }

        /// <summary>
        /// 总步数
        /// </summary>
        public int StepCount { get; set; }

        /// <summary>
        /// 左脚总步数
        /// </summary>
        public int StepCountL { get; set; }

        /// <summary>
        /// 右脚总步数
        /// </summary>
        public int StepCountR { get; set; }

        /// <summary>
        /// 平均步态周期=所有的步态周期时间总和除以步数  单位秒
        /// </summary>
        public int T_Average { get; set; }

        /// <summary>
        /// 平均步辐
        /// </summary>
        public int BF_Average { get; set; }

        /// <summary>
        /// 平均步频 ：次每分钟
        /// </summary>
        public int Rhythm_Average { get; set; }

        /// <summary>
        /// 平均步长 单位厘米
        /// </summary>
        public int StepLength_Average { get; set; }

        /// <summary>
        /// 左脚平均步长 单位厘米
        /// </summary>
        public int StepLengthL_Average { get; set; }

        /// <summary>
        /// 右脚平均步长 单位厘米
        /// </summary>
        public int StepLengthR_Average { get; set; }

        /// <summary>
        /// 平均步时 单位毫秒
        /// </summary>
        public int StepTime_Average { get; set; }

        /// <summary>
        /// 左脚平均步时 单位毫秒
        /// </summary>
        public int StepTimeL_Average { get; set; }

        /// <summary>
        /// 右脚平均步时 单位毫秒
        /// </summary>
        public int StepTimeR_Average { get; set; }

        /// <summary>
        /// 平均步宽 单位厘米
        /// </summary>
        public int StepWidth_Average { get; set; }

        /// <summary>
        /// 平均对称性=(右脚的总步时/右脚步数)/(左脚的总步时/左脚步数)
        /// </summary>
        public int Symm_Average { get; set; }

        /// <summary>
        /// 上一个步态周期的对称性
        /// </summary>
        public int Symm_LastOne { get; set; }
    }

    /// <summary>
    /// 步态单步记录
    /// </summary>
    public class GaitRecordStep
    {

        public override string ToString()
        {
            string name = F == 1 ? "左" : "右";
            return $"第{I.ToString("D3")}步 - {name} - 步态周期={T}ms  步时={BS}ms  步长={BC}cm  步幅={BF.ToString("D3")}cm  步宽={BK}cm";
        }
        /// <summary>
        /// 序号
        /// </summary>
        public int I { get; set; }

        /// <summary>
        /// 标记 1=左 2=右
        /// </summary>
        public int F { get; set; }

        /// <summary>
        /// 步时 毫秒 不同侧足跟两次触地时长
        /// </summary>
        public int BS { get; set; }

        /// <summary>
        /// 步态周期 毫秒 同侧足跟两次触地时长
        /// </summary>
        public int T { get; set; }

        /// <summary>
        /// 步长 cm 不同侧足两次触地距离
        /// </summary>
        public int BC { get; set; }

        /// <summary>
        /// 步幅 cm  同侧足跟两次触地距离
        /// </summary>
        public int BF { get; set; }

        /// <summary>
        /// 步宽 cm
        /// </summary>
        public int BK { get; set; }
        /// <summary>
        /// 踩下时间点
        /// </summary>

        public long Time { get; set; }
        /// <summary>
        /// 对称性
        /// </summary>
        public int DCX { get; set; }
        /// <summary>
        /// 步频/节律
        /// </summary>
        public int BP { get; set; }
    }

    /// <summary>
    /// 步态记录模式
    /// </summary>
    public enum GaitRecordModes
    {
        /// <summary>
        /// 时间差模式：开始时间和结束时间，暂停会有问题
        /// </summary>
        StartEnd = 0,
        /// <summary>
        /// 时间累计模式：开始时间和增加时间，可以暂停
        /// </summary>
        StartAdd
    }
}
