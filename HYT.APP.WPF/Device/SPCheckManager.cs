using CL.Common;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace HYT.APP.WPF.Device
{
    /// <summary>
    /// 串口检测管理器
    /// 注意：
    /// 1.串口拔插，获取的串口数会变； 一轮一轮，检测一轮更新一次；
    /// 2.已经在通信的设备端口不要检测； 忽略
    /// 3.检测完成要停止；
    /// </summary>
    public class SPCheckManage
    {
        private object _lockListCheckTask = new object();

        /// <summary>
        /// 正在检测的任务
        /// </summary>
        public List<SPCheckTask> ListCheckTask = new List<SPCheckTask>();

        /// <summary>
        /// 当前正在检测的所有系统串口信息
        /// </summary>
        public List<SPCheckInfo> ListSPInfo = new List<SPCheckInfo>();

        #region ■------------------ 单例

        private SPCheckManage()
        {
            _timerMain.Elapsed += _timerMain_Tick;
            _timerMain.Start();

            _dataReceiveThread = new Thread(DataReceive_Thread);
            _dataReceiveThreadState = 1;
            _dataReceiveThread.Start();
            ManangeState = SPCheckManageStates.CheckReady;
        }
        public readonly static SPCheckManage Instance = new SPCheckManage();

        #endregion

        #region ■------------------ 外部接口

        public void Stop()
        {
            try
            {
                if (_timerMain != null) _timerMain.Stop();
                _dataReceiveThreadState = 0;
                _dataReceiveThread = null;

                if (CurrentCheckSPInfo != null)
                {
                    if (CurrentCheckSPInfo.SPort != null)
                    {
                        if (CurrentCheckSPInfo.SPort.IsOpen)
                        {
                            CurrentCheckSPInfo.SPort.Close();
                        }
                        CurrentCheckSPInfo.SPort = null;
                    }
                    CurrentCheckSPInfo = null;
                }

                lock (_lockListCheckTask)
                {
                    ListCheckTask.Clear();
                }

                ListSPInfo.Clear();
                ManangeState = SPCheckManageStates.Rest;
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        /// <summary>
        /// 根据规则检测
        /// </summary>
        /// <param name="action"></param>
        public void StartTask(SPCheckTask item)
        {
            lock (_lockListCheckTask)
            {
                if (ListCheckTask.Count(o => o.ID == item.ID) <= 0)
                {
                    ListCheckTask.Add(item);
                }
            }
        }

        public void ClearTask()
        {
            lock (_lockListCheckTask)
            {
                ListCheckTask.Clear();
            }
        }

        #endregion

        #region ■------------------ 计时器：串口循环检测

        private System.Timers.Timer _timerMain = new System.Timers.Timer(250);

        /// <summary>
        /// 一个串口检测的秒数
        /// </summary>
        public int CheckSecond { get; private set; } = 1;

        public SPCheckManageStates ManangeState = SPCheckManageStates.Rest;

        public string StateText
        {
            get
            {
                switch (ManangeState)
                {
                    case SPCheckManageStates.Rest:
                        return "停止";
                    case SPCheckManageStates.CheckReady:
                        return "准备中";
                    case SPCheckManageStates.Checking:
                        return "检测中";
                    default:
                        return "";
                }
            }
        }

        /// <summary>
        /// 当前正在检测的窗口信息
        /// </summary>
        public SPCheckInfo CurrentCheckSPInfo;

        private void _timerMain_Tick(object sender, EventArgs e)
        {
            try
            {
                switch (ManangeState)
                {
                    case SPCheckManageStates.Rest:
                        {
                            AddLog($"【串口检测】停止中");
                        }
                        break;
                    case SPCheckManageStates.CheckReady://检测准备
                        {
                            lock (_lockListCheckTask)
                            {
                                if (ListCheckTask.Count > 0)//没有检测任务跳过
                                {
                                    //清理数据
                                    ListSPInfo.Clear();
                                    CurrentCheckSPInfo = null;

                                    //读取所有串口
                                    string[] names = SerialPort.GetPortNames();
                                    List<string> checkNames = new List<string>();
                                    for (int i = 0; i < names.Length; i++)
                                    {
                                        string name = names[i];
                                        if (name == SPDevice_ZBJ.Instance.COM)
                                        {
                                            continue;
                                        }

                                        checkNames.Add(name);
                                        ListSPInfo.Add(new SPCheckInfo()
                                        {
                                            PortName = name,
                                            Index = i,
                                            SPort = new SerialPort()
                                            {
                                                PortName = name,
                                                BaudRate = 115200,
                                                DataBits = 8,
                                                StopBits = StopBits.One,
                                                Parity = Parity.None,
                                                ReadTimeout = 2000
                                            }
                                        });
                                    }

                                    AddLog($"【串口检测】---------------------- 检测开始，检测任务 {ListCheckTask.Count} 个 ，待检测串口 {ListSPInfo.Count} 个 = {string.Join(" ", checkNames.ToArray())}");

                                    ManangeState = SPCheckManageStates.Checking;
                                }
                            }
                        }
                        break;
                    case SPCheckManageStates.Checking://开始检测
                        {
                            if (ListSPInfo.Count <= 0)
                            {
                                ManangeState = SPCheckManageStates.CheckReady;
                            }
                            else
                            {
                                if (CurrentCheckSPInfo != null)
                                {
                                    AddLog($"【串口检测】正在检测 =  {CurrentCheckSPInfo.PortName} - {CurrentCheckSPInfo.CheckResult}");
                                }
                                else
                                {
                                    AddLog($"【串口检测】正在检测：无");
                                }

                                //没有就执行第一个
                                if (CurrentCheckSPInfo == null)
                                    CurrentCheckSPInfo = ListSPInfo[0];
                                switch (CurrentCheckSPInfo.CheckResult)
                                {
                                    case SPCheckTaskResults.Prepare:
                                        {
                                            try
                                            {
                                                //打开串口
                                                if (!CurrentCheckSPInfo.SPort.IsOpen)
                                                {
                                                    CurrentCheckSPInfo.SPort.Open();
                                                    if (CurrentCheckSPInfo.SPort.IsOpen)
                                                    {
                                                        //没有异常
                                                        LastError = "";
                                                        CurrentCheckSPInfo.LastTick = DateTime.Now.Ticks;
                                                        ReceiveBuff = new byte[2048];
                                                        ReceiveData = new byte[2048];
                                                        RecDataHead = 0;
                                                        RecDataTail = 0;
                                                        CurrentCheckSPInfo.CheckResult = SPCheckTaskResults.Checking;

                                                        //启动至就绪状态
                                                        byte[] data = { 0x5A, 0xB2, 0x00, 0xA5 };
                                                        CurrentCheckSPInfo.SPort.Write(data, 0, 4);
                                                    }
                                                    else
                                                    {
                                                        CurrentCheckSPInfo.CheckResult = SPCheckTaskResults.Error;
                                                        LastError = $"{CurrentCheckSPInfo.PortName}打开失败";
                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                CurrentCheckSPInfo.CheckResult = SPCheckTaskResults.Error;
                                                LastError = $"{CurrentCheckSPInfo.PortName}打开失败 {ex.Message}";
                                            }
                                        }
                                        break;
                                    case SPCheckTaskResults.Checking:
                                        {
                                            long tick = DateTime.Now.Ticks - CurrentCheckSPInfo.LastTick;
                                            CurrentCheckSPInfo.LastTick = DateTime.Now.Ticks;
                                            CurrentCheckSPInfo.WaitTime += tick;

                                            if (CurrentCheckSPInfo.WaitTime > TimeSpan.FromSeconds(CheckSecond).Ticks)//超时
                                            {
                                                try
                                                {
                                                    CurrentCheckSPInfo.Dispose();
                                                }
                                                catch (Exception)
                                                {

                                                }
                                                CurrentCheckSPInfo.CheckResult = SPCheckTaskResults.TimeOut;
                                            }
                                        }
                                        break;
                                    case SPCheckTaskResults.Error:
                                    case SPCheckTaskResults.TimeOut:
                                    case SPCheckTaskResults.True:
                                        {
                                            var index = CurrentCheckSPInfo.Index + 1;
                                            if (ListSPInfo.Count <= index)
                                            {
                                                AddLog($"【串口检测】---------------------- 检测结束");
                                                ManangeState = SPCheckManageStates.CheckReady;//下一轮检测
                                            }
                                            else
                                            {
                                                CurrentCheckSPInfo = ListSPInfo[index];//继续检测下一个
                                            }
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }

                Thread.Sleep(50);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        public string byteArrayToHexStr(byte[] bytes)
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    returnStr += " " + bytes[i].ToString("X2");
                }
            }
            return returnStr;
        }

        #endregion

        #region ■------------------ 数据接收

        /// <summary>
        /// 数据接收线程
        /// </summary>
        private Thread _dataReceiveThread = null;

        /// <summary>
        /// 数据接收线程状态 0-线程停止  1-线程运行
        /// </summary>
        private int _dataReceiveThreadState = 0;

        /// <summary>
        /// 数据接收线程代码
        /// </summary>
        private void DataReceive_Thread()
        {
            while (_dataReceiveThreadState == 1)
            {
                try//异常继续检测
                {
                    if (ManangeState == SPCheckManageStates.Checking)
                    {
                        if (CurrentCheckSPInfo != null && CurrentCheckSPInfo.SPort != null && CurrentCheckSPInfo.SPort.IsOpen)
                        {
                            if (CurrentCheckSPInfo.CheckResult == SPCheckTaskResults.Checking)
                            {
                                if (CurrentCheckSPInfo.SPort.BytesToRead > 0 && CurrentCheckSPInfo.SPort.IsOpen)
                                {
                                    int len = CurrentCheckSPInfo.SPort.Read(ReceiveBuff, 0, 1024);//从串口缓冲区读取数据
                                    if (len > 0)
                                    {
                                        //接收数据到数据缓存 数据缓存头递增
                                        for (int pos = 0; pos < len; pos++)
                                        {
                                            ReceiveData[RecDataHead] = ReceiveBuff[pos];
                                            RecDataHead = (RecDataHead + 1) % 2048;
                                        }
                                    }

                                    int count = (RecDataHead - RecDataTail + 2048) % 2048;//头和尾之间的数据为未处理的数据

                                    byte[] data = new byte[count];
                                    for (int i = 0; i < count; i++)
                                    {
                                        data[i] = ReceiveData[(RecDataTail + i) % 2048];
                                    }
                                    AddLog($"【串口检测】收到数据  " + byteArrayToHexStr(data));

                                    //检测匹配

                                    lock (_lockListCheckTask)
                                    {
                                        foreach (var item in ListCheckTask)
                                        {
                                            var recDataTail = RecDataTail;
                                            var count2 = count;
                                            if (item.DataFormat.Length <= 0) continue;
                                            if (ReceiveData.Length < item.DataFormat.Length) continue;

                                            while (count2 >= item.DataFormat.Length)
                                            {
                                                if (ReceiveData[recDataTail] == item.Head && ReceiveData[recDataTail + 1] == item.DataFormat[1])
                                                {
                                                    //无CRC验证

                                                    CurrentCheckSPInfo.CheckResult = SPCheckTaskResults.True;
                                                    //读取到正确数据
                                                    AddLog($"【串口检测】{item.ID} 检测成功 ");
                                                    CurrentCheckSPInfo.SPort.Close();


                                                    Task.Run(() => item.Callback.Invoke(CurrentCheckSPInfo.SPort.PortName));//开启线程


                                                    item.IsSuccess = true;
                                                    //listSuccess.Add(item);

                                                    break;
                                                }
                                                else
                                                {
                                                    count2--;
                                                    recDataTail = (recDataTail + 1) % 2048;
                                                }
                                            }
                                        }
                                    }


                                    RecDataTail = (RecDataTail + count) % 2048;

                                    lock (_lockListCheckTask)
                                    {
                                        ListCheckTask.RemoveAll(o => o.IsSuccess);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                }

                Thread.Sleep(50);
            }
        }

        /// <summary>
        /// 接收数据缓存
        /// </summary>
        public byte[] ReceiveBuff = new byte[2048];

        /// <summary>
        /// 数据缓存
        /// </summary>
        public byte[] ReceiveData = new byte[2048];

        /// <summary>
        /// 数据缓存对头
        /// </summary>
        public int RecDataHead = 0;

        /// <summary>
        /// 数据缓存对尾
        /// </summary>
        public int RecDataTail = 0;

        #endregion

        #region ■------------------ 发送数据

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool Send(byte[] data)
        {
            try
            {
                if (CurrentCheckSPInfo.SPort == null || !CurrentCheckSPInfo.SPort.IsOpen)
                {
                    return false;
                }
                CurrentCheckSPInfo.SPort.Write(data, 0, data.Length);
                //DebugLog($"【串口通信】{PortName} 发送数据 {PublicMethod.ByteArrayToHexStr(data)} {DateTime.Now.ToString("HH:mm:ss.ffff")}");
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return false;
            }
        }

        #endregion

        #region ■------------------ 日志

        private void AddLog(string line)
        {
            LogLast = line;
            OnLog?.Invoke(null, line);
            if (string.IsNullOrEmpty(LogHistory))
            {
                LogHistory = line;
            }
            else
            {
                if (LogHistory.Length > 250)
                {
                    LogHistory = "";
                    //Log = Log.Remove(0, Log.IndexOf("\r\n"));
                }
                LogHistory += "\r\n" + line;
            }
        }

        public string LastError { get; set; }
        public string LastData = "";
        public string LogHistory = "";
        public string LogLast = "";

        public event EventHandler<string> OnLog;

        #endregion



    }

    /// <summary>
    /// 串口检测管理器状态
    /// </summary>
    public enum SPCheckManageStates
    {
        /// <summary>
        /// 休息中，不运行计时器，不检测
        /// </summary>
        Rest,
        /// <summary>
        /// 检测准备，读取串口
        /// </summary>
        CheckReady,
        /// <summary>
        /// 检测中
        /// </summary>
        Checking,
    }

    /// <summary>
    /// 串口检测任务
    /// </summary>
    public class SPCheckTask
    {
        public bool IsSuccess { get; set; }
        public string ID { get; set; }

        /// <summary>
        /// 数据格式
        /// </summary>
        public byte[] DataFormat;
        public byte Head
        {
            get
            {
                if (DataFormat != null && DataFormat.Length > 0)
                {
                    return DataFormat[0];
                }
                return 0xFF;
            }
        }
        public byte Trail
        {
            get
            {
                if (DataFormat != null && DataFormat.Length > 0)
                {
                    return DataFormat[DataFormat.Length - 1];
                }
                return 0xFF;
            }
        }

        /// <summary>
        /// 检测到的回调
        /// </summary>
        public Action<string> Callback;
    }

    /// <summary>
    /// 串口检测任务结果
    /// </summary>
    public enum SPCheckTaskResults
    {
        /// <summary>
        /// 准备，打开串口
        /// </summary>
        Prepare,
        /// <summary>
        /// 检测中，还没有结果
        /// </summary>
        Checking,
        /// <summary>
        /// 检测错误
        /// </summary>
        Error,
        /// <summary>
        /// 检测正确
        /// </summary>
        True,
        /// <summary>
        /// 超时
        /// </summary>
        TimeOut
    }

    /// <summary>
    /// 串口检测信息
    /// </summary>
    public class SPCheckInfo : IDisposable
    {
        #region ■------------------ 资源释放

        ~SPCheckInfo()
        {
            Dispose(false);
        }
        /// <summary>
        /// 检测冗余调用
        /// </summary>
        private bool isDispose = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing">是否释放托管资源</param>
        protected void Dispose(bool disposing)
        {
            if (!isDispose)
            {
                if (disposing)
                {
                    if (SPort != null && SPort.IsOpen)
                    {
                        SPort.Close();
                    }
                    SPort = null;
                }

                //释放非托管资源
                isDispose = true;// 标识此对象已释放
            }
        }

        #endregion

        public int Index { get; set; }
        public string PortName { get; set; }
        public bool IsCheck { get; set; }
        public int Priority { get; set; }

        /// <summary>
        /// 检测结果
        /// </summary>
        public SPCheckTaskResults CheckResult { get; set; }

        public SerialPort SPort { get; set; }

        /// <summary>
        /// 已经检测的时长 用于检测超时
        /// </summary>
        public long WaitTime { get; set; }

        public long LastTick { get; set; }
    }
}
