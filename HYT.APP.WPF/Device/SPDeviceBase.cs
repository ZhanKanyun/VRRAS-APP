using CL.Common;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HYT.APP.WPF.Device
{
    /// <summary>
    /// 串口通讯设备基类
    /// </summary>
    public abstract class SPDeviceBase
    {
        #region ■------------------ 日志输出

        /// <summary>
        /// 最后一次日志信息
        /// </summary>
        public DeviceLog LogLast { get; set; }

        public event EventHandler<DeviceLog> OnLog;
        public event EventHandler<string> OnConnect;
        public event EventHandler<string> OnDisconnect;

        public void ConnectSuccess(string com)
        {
            OnConnect?.Invoke(null, com);
        }

        public void ShowLog(string message, int type = 1)
        {
            LogLast = new DeviceLog(type, message);
            //OnLog?.BeginInvoke(this, LogLast, null, null);
            Task.Run(() => OnLog?.Invoke(this, LogLast));
        }
        public void ShowLog(DeviceLog log)
        {
            LogLast = log;
            OnLog?.Invoke(this, log);
        }

        #endregion

        #region ■------------------ 设备状态

        public DeviceStates DeviceState = DeviceStates.None;

        public bool IsCommunication
        {
            get
            {
                return DeviceState == DeviceStates.Run;
            }
        }


        public string StateText
        {
            get
            {
                switch (DeviceState)
                {
                    case DeviceStates.CheckStart:
                        return "未开始";
                    case DeviceStates.CheckWait:
                        return "检测中";
                    case DeviceStates.Connecting:
                        return "连接中";
                    default:
                        return "无";
                }
            }
        }

        #endregion

        /// <summary>
        /// 串口波特率
        /// </summary>
        public int BaudRate { get; private set; } = 115200;

        public string COM = "";

        public string Name = "";

        public string NameCOM
        {
            get { return Name + COM; }
        }
        public override string ToString()
        {
            return Name + COM;
        }

        public SerialPort SPort;

        public bool IsOpen
        {
            get
            {
                if (SPort != null && SPort.IsOpen)
                {
                    return true;
                }
                return false;
            }
        }





        public virtual void Start(int baudRate)
        {
            BaudRate = baudRate;
        }

        public virtual void ReStart()
        {
            DeviceState = DeviceStates.CheckStart;
            OnDisconnect?.Invoke(null, COM);
            COM = "";
            if (SPort != null)
            {
                SPort.Close();
                SPort = null;
            }
        }

        public virtual void Stop()
        {
            if (SPort != null)
            {
                if (SPort.IsOpen)
                {
                    SPort.Close();
                }
                SPort = null;
            }
            COM = "";
            DeviceState = DeviceStates.None;
        }

        public void SendData(byte[] data)
        {
            if (SPort != null && SPort.IsOpen)
            {
                SPort.Write(data, 0, data.Length);
                ShowLog(new DeviceLog(3, $"【{NameCOM}】" + PublicMethod.ByteArrayToHexStr(data)));
            }
        }

    }

    /// <summary>
    /// 设备日志事件参数
    /// </summary>
    public class DeviceLog
    {
        public DeviceLog(string message)
        {
            Message = message;
        }
        public DeviceLog(int type, string message)
        {
            Type = type;
            Message = message;
        }

        /// <summary>
        /// 1-日志 2-收到数据 3-发送数据
        /// </summary>
        public int Type { get; set; } = 1;

        public string Message { get; set; }
    }

    /// <summary>
    /// 设备数据事件参数
    /// </summary>
    public class DeviceDataEventArgs
    {
        public byte[] Data { get; set; }

        public string COM { get; set; }

        public DeviceDataTypes Type = DeviceDataTypes.Work_Data;

    }

    /// <summary>
    /// 设备数据类型
    /// </summary>
    public enum DeviceDataTypes
    {
        Ready_HeartBeat,
        Work_Data
    }


    /// <summary>
    /// 设备连接状态
    /// </summary>
    public enum DeviceStates
    {
        None,
        CheckStart,
        CheckWait,
        Connecting,
        Run
    }
}
