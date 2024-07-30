using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HYT.APP.WPF.Device
{
    /// <summary>
    /// 设备状态
    /// </summary>
    public struct DeviceState
    {
        /// <summary>
        /// 是否正常   电机停止不算异常，电机只在评估和训练中启动
        /// </summary>
        public bool IsNormal {
            get {
                return string.IsNullOrEmpty(COM)==false && IsTXGZ_PLC==0 && IsDJGZ==0 && IsJJTZ==0 && IsDanger==0 && IsTXGZ_CJ==0;
            }
        }
        /// <summary>
        /// 串口是否连接 1连接 0未连接
        /// </summary>
        public string COM { get; set; }

        /// <summary>
        /// 是否启动  0启动 1停止
        /// </summary>
        public int IsRun { get; set; }

        /// <summary>
        /// 是否PLC通信故障
        /// </summary>
        public int IsTXGZ_PLC { get; set; }

        /// <summary>
        /// 是否电机故障
        /// </summary>
        public int IsDJGZ { get; set; }

        /// <summary>
        /// 是否紧急停止
        /// </summary>
        public int IsJJTZ { get; set; }

        /// <summary>
        /// 是否危险
        /// </summary>
        public int IsDanger { get; set; }

        /// <summary>
        /// 是否采集通讯故障
        /// </summary>
        public int IsTXGZ_CJ { get; set; }

    }
}
