using CL.Common;
using CL.Common.Data;
using HYT.APP.WPF.Device;
using HYT.APP.WPF.Manager;
using HYT.MidiManager;
using KCL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Xml.Linq;

namespace HYT.APP.WPF
{
    /// <summary>
    /// WinDebug.xaml 的交互逻辑
    /// </summary>
    public partial class WinDebug : Window
    {
        #region ■------------------ 构造加载

        public VMDebug VM;
        public WinDebug()
        {
            VM = new VMDebug();
            InitializeComponent();
            this.DataContext = VM;
            try
            {
                UpdateCOM();

            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                AppManager.Instance.OnTick2 += OnTick2;
                SPCheckManage.Instance.OnLog += SPCheckManage_OnLog;
                SPDevice_ZBJ.Instance.OnLog += SPDevice_ZBJ_OnLog;
                DeviceManager.Instance.DataAnalysisDone += Instance_DataAnalysisDone;
               
                MoniWalkManager.Instance.OnNextStep += MoniWalkManager_OnNextStep;

                _dataReceiveThreadState = 1;
                _dataReceiveThread = new Thread(DataReceive_Thread);
                _dataReceiveThread.Start();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }



        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                e.Cancel = true;
                this.Hide();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        #endregion

        #region ■------------------ 走步机串口控制

        /// <summary>
        /// 断开串口重新检测
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRestart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SPCheckManage.Instance.ClearTask();

                if (SPDevice_ZBJ.Instance.IsStart)
                {
                    SPDevice_ZBJ.Instance.ReStart();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        /// <summary>
        /// 数据分析结束，显示到输出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Instance_DataAnalysisDone(object? sender, DeviceData e)
        {
            try
            {
                ShowLog(e.ToString(), 2);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        #endregion

        #region ■------------------ 数据记录和分析测试

        private void btnStartRecord_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (TrainTaskManager.Instance.IsRun)
                {
                    ShowLog($"【数据记录分析】正在进行训练，无法开始", 1);
                    return;
                }

                if (DeviceDataAnalysisManager.Instance.Start())
                {
                    ShowLog($"【数据记录分析】开始------------------------------------------------------------------------", 4);
                }
                else
                {
                    ShowLog($"【数据记录分析】开始失败，设备未正常通信", 4);
                }
            }
            catch (Exception ex)
            {

                LogHelper.Error(ex);
            }
        }

        private void btnStopRecord_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DeviceDataAnalysisManager.Instance.State != DeviceDataAnalysisManagerStates.Run &&DeviceDataAnalysisManager.Instance.State != DeviceDataAnalysisManagerStates.Pause)
                {
                    ShowLog($"【数据记录分析】未开始记录，操作无效------------------------------------------------------------------------ ", 4);
                    return;
                }
                var result = DeviceDataAnalysisManager.Instance.Stop();
                //string sss = JsonConvert.SerializeObject(a);
                ShowLog($"【数据记录分析】结束------------------------------------------------------------------------ {result}\r\n", 4);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        private void btnPauseRecord_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DeviceDataAnalysisManager.Instance.State == DeviceDataAnalysisManagerStates.Run)
                {
                    DeviceDataAnalysisManager.Instance.Pause();
                    ShowLog($"【数据记录分析】暂停------------------------------------------------------------------------ ", 4);
                }
                else
                {
                    ShowLog($"【数据记录分析】未开始记录，操作无效 ", 4);
                }

            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        private void btnContinueRecord_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DeviceDataAnalysisManager.Instance.State == DeviceDataAnalysisManagerStates.Pause)
                {
                    DeviceDataAnalysisManager.Instance.Continue();
                    ShowLog($"【数据记录分析】继续------------------------------------------------------------------------", 4);
                }
                else
                {
                    ShowLog($"【数据记录分析】未开始记录，操作无效 ", 4);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        #endregion

        #region ■------------------ 主程序-输出

        /// <summary>
        /// 输出消息到调试控制台
        /// </summary>
        /// <param name="info"></param>
        /// <param name="type"></param>
        public void ShowLog(string info, int type)
        {
            if (!VM.ConsoleIsStart)
            {
                return;
            }
            if (!VM.ConsoleIsShowReceive&&type==2)
            {
                return;
            }
            if (!VM.ConsoleIsShowSend && type == 3)
            {
                return;
            }
            try
            {
                Brush brush = null;
                switch (type)
                {
                    case 1:
                        info = DateTime.Now.ToString("HH:mm:ss.fff") + " - " + "系统信息：" + info;
                        brush = Brushes.Black;
                        break;
                    case 2:
                        info = DateTime.Now.ToString("HH:mm:ss.fff") + " - " + "接收数据：" + info;
                        brush = Brushes.Green;
                        break;
                    case 3:
                        info = DateTime.Now.ToString("HH:mm:ss.fff") + " - " + "发送数据：" + info;
                        brush = Brushes.OrangeRed;
                        break;
                    case 4:
                        info = DateTime.Now.ToString("HH:mm:ss.fff") + " - " + "调试信息：" + info;
                        brush = Brushes.Blue;
                        break;
                    case 5:
                        info = DateTime.Now.ToString("HH:mm:ss.fff") + " - " + "MIDI：" + info;
                        brush = Brushes.DarkOrange;
                        break;
                    default:
                        break;
                }

                txtConsole.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (flowdoc.Blocks.Count > 500)
                    {
                        flowdoc.Blocks.Clear();
                    }
                    lock (this)
                    {
                        var p = new Paragraph(); // Paragraph 类似于 html 的 P 标签  
                        var r = new Run(info); // Run 是一个 Inline 的标签  
                        p.Inlines.Add(r);
                        p.Margin = new Thickness(2);
                        p.Foreground = brush;//设置字体颜色  
                        flowdoc.Blocks.Add(p);
                        txtConsole.ScrollToEnd();
                    }
                }));
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        private void BtnClearLog_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                flowdoc.Blocks.Clear();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        private void BtnStopLog_Click(object sender, RoutedEventArgs e)
        {
            VM.ConsoleIsStart = !VM.ConsoleIsStart;
        }

        private void SPCheckManage_OnLog(object sender, string e)
        {
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    ShowLog(e, 1);
                });
            }
            catch (Exception ex)
            {

                LogHelper.Error(ex);
            }
        }

        private void SPDevice_ZBJ_OnLog(object sender, DeviceLog e)
        {
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    ShowLog(e.Message, e.Type);
                });
            }
            catch (Exception ex)
            {

                LogHelper.Error(ex);
            }
        }

        #endregion





        #region ■------------------ 设备模拟器

        /// <summary>
        /// 上一次数据
        /// </summary>
        public DeviceData LastMoniSendData;
        public DateTime LastMoniSendTime=DateTime.MinValue;

        private void OnTick2(object sender, int e)
        {
            try
            {
                VM.PortName = SPDevice_ZBJ.Instance.COM==""?"检测中": SPDevice_ZBJ.Instance.COM;
                bool isSend = false;
                var newData = VM.MD;
 
                if (LastMoniSendData == null 
                    || LastMoniSendData.LeftFoot.IsGround != newData.LeftFoot.IsGround
                    || LastMoniSendData.RightFoot.IsGround != newData.RightFoot.IsGround
                    || LastMoniSendData.Speed != newData.Speed
                    || LastMoniSendData.State_IsDanger != newData.State_IsDanger
                    || LastMoniSendData.State != newData.State)
                {
                    isSend = true;
                }

                if ((DateTime.Now - LastMoniSendTime).TotalSeconds>=1)
                {
                    isSend = true;
                }

                if (isSend)
                {
                    LastMoniSendTime = DateTime.Now;
                    //发送数据：发生事件 马上上传  正常1秒一次
                    if (MONIPort != null && MONIPort.IsOpen)
                    {
                        byte[] data = VM.MD.ConvertToByteArray();
                        ShowLog_MONI($"{MONIPort.PortName} " + PublicMethod.ByteArrayToHexStr(data), 3);
                        MONIPort.Write(data, 0, data.Length);

                        LastMoniSendData = new DeviceData();
                        LastMoniSendData.FromByteArray(data);
                    }
                }

                MoniWalkManager.Instance.WalkTimeMultiplier = VM.WalkTimeMultiplier;

            }
            catch (Exception ex)
            {

                LogHelper.Error(ex);
            }

        }

        public void UpdateCOM()
        {
            try
            {
                string[] names = SerialPort.GetPortNames();

                foreach (string name in names)
                {
                    ComboBoxItem item=new ComboBoxItem();
                    item.Content = name;
             
                    if (!comboxCOM.Items.Contains(item))
                    {
                        comboxCOM.Items.Add(item);
                    }
                }
                if (comboxCOM.Items.Count>0)
                {
                    comboxCOM.SelectedIndex = comboxCOM.Items.Count-1;
                }
                ShowLog_MONI($"更新串口列表  {String.Join(" ",names)}");

            }
            catch (Exception ex)
            {

                LogHelper.Error(ex);
            }
        }

        /// <summary>
        /// 模拟串口
        /// </summary>
        public SerialPort MONIPort;

        private void btnOpenMONICOM_click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (MONIPort == null || !MONIPort.IsOpen)
                {
                    var name = comboxCOM.SelectedItem.ToString().Split(":")[1];
                    //name = comboxCOM.SelectedItem.ToString();

                    if (MONIPort == null)
                    {
                        MONIPort = new SerialPort();
                        MONIPort.PortName = name.Trim();
                        MONIPort.BaudRate = 115200;
                        MONIPort.DataBits = 8;
                        MONIPort.StopBits = StopBits.One;
                        MONIPort.Parity = Parity.None;
                        MONIPort.ReadTimeout = 2000;

                        ShowLog_MONI($"创建串口对象 {MONIPort.PortName}");
                    }
                    if (!MONIPort.IsOpen)
                    {
                        MONIPort.Open();
                    }




                    if (MONIPort.IsOpen)
                    {
                        ShowLog_MONI($"打开串口成功 {MONIPort.PortName}");
                        btnOpenMONICOM.Content = "关闭串口";
                        comboxCOM.IsEnabled = false;
                    }
                }
                else
                {
                    if (MONIPort != null && MONIPort.IsOpen)
                    {
                        MONIPort.Close();
                        ShowLog_MONI($"关闭串口成功 {MONIPort.PortName}");
                        btnOpenMONICOM.Content = "打开串口";
                        comboxCOM.IsEnabled = true;
                        MONIPort = null;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                ShowLog_MONI($" {ex.Message + "\r\n" + ex.StackTrace}", 4);
            }
        }

        /// <summary>
        /// 模拟行走管理器：下一步事件通知
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MoniWalkManager_OnNextStep(object? sender, MoniWalkDeviceData e)
        {
            try
            {
                VM.MD.LeftFoot.Set(e.LeftFoot);
                VM.MD.RightFoot.Set(e.RightFoot);
                ShowLog_MONI($"【模拟行走】{e}");
                OnTick2(null, 0);

            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        private void AutoWalk_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                panelSetT.IsEnabled = false;
                panelSetStepLength.IsEnabled = false;
                panelLeftFoot.IsEnabled = false;
                panelRightFoot.IsEnabled = false;
                panelZX.IsEnabled = false;
                panelShouDong.IsEnabled = false;

                //初始化
                List<int> times = new List<int>();
                times.Add(Convert.ToInt32(txtStepTime1.Text));
                times.Add(Convert.ToInt32(txtStepTime2.Text));
                times.Add(Convert.ToInt32(txtStepTime3.Text));
                times.Add(Convert.ToInt32(txtStepTime4.Text));
                List<int> lengths = new List<int>();
                lengths.Add(Convert.ToInt32(txtStepLength1.Text));
                lengths.Add(Convert.ToInt32(txtStepLength2.Text));
                MoniWalkManager.Instance.SetParam(times, lengths);

                MoniWalkManager.Instance.Auto();

                //MoniWalkManager.Instance.IsAutoWalk = true;
                //MoniWalkManager.Instance.WalkTimeMultiplier = VM.WalkTimeMultiplier;

            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        private void AutoWalk_UnChecked(object sender, RoutedEventArgs e)
        {
            try
            {
                panelSetT.IsEnabled = true;
                panelSetStepLength.IsEnabled = true;
                panelLeftFoot.IsEnabled = true;
                panelRightFoot.IsEnabled = true;
                panelZX.IsEnabled = true;
                panelShouDong.IsEnabled = true;
                MoniWalkManager.Instance.Reset();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        private void btn_ZhanLi_click(object sender, RoutedEventArgs e)
        {
            try
            {
                VM.MD.LeftFoot.IsGround = 1;
                VM.MD.RightFoot.IsGround = 1;
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        private void btn_ManualNext_click(object sender, RoutedEventArgs e)
        {
            try
            {
                //初始化
                List<int> times = new List<int>();
                times.Add(Convert.ToInt32(txtStepTime1.Text));
                times.Add(Convert.ToInt32(txtStepTime2.Text));
                times.Add(Convert.ToInt32(txtStepTime3.Text));
                times.Add(Convert.ToInt32(txtStepTime4.Text));
                List<int> lengths = new List<int>();
                lengths.Add(Convert.ToInt32(txtStepLength1.Text));
                lengths.Add(Convert.ToInt32(txtStepLength2.Text));
                MoniWalkManager.Instance.SetParam(times, lengths);
                MoniWalkManager.Instance.ManualNextStep();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        private void btn_ManualNextGround_click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<int> times = new List<int>();
                times.Add(Convert.ToInt32(txtStepTime1.Text));
                times.Add(Convert.ToInt32(txtStepTime2.Text));
                times.Add(Convert.ToInt32(txtStepTime3.Text));
                times.Add(Convert.ToInt32(txtStepTime4.Text));
                List<int> lengths = new List<int>();
                lengths.Add(Convert.ToInt32(txtStepLength1.Text));
                lengths.Add(Convert.ToInt32(txtStepLength2.Text));
                MoniWalkManager.Instance.SetParam(times, lengths);
                MoniWalkManager.Instance.ManualNextStep(true);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        #region 〓〓〓〓〓〓〓 数据接收

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
            while (this._dataReceiveThreadState == 1)
            {
                try
                {
                    if (MONIPort != null)
                    {
                        if (MONIPort.IsOpen&& MONIPort.BytesToRead > 0)
                        {

                            #region 〓〓〓〓〓〓〓 缓存接收数据-2048字节

                            int len = MONIPort.Read(this.ReceiveBuff, 0, 1024);//从串口缓冲区读取数据，返回实际读取到的字节数
                            if (len > 0)
                            {
                                for (int pos = 0; pos < len; pos++)
                                {
                                    this.ReceiveData[this.RecDataHead] = this.ReceiveBuff[pos];
                                    this.RecDataHead = (this.RecDataHead + 1) % this.ReceiveData.Length;
                                }
                            }

                            #endregion

                            #region 〓〓〓〓〓〓〓 处理缓存数据-处理完

                            int count = (this.RecDataHead - this.RecDataTail + this.ReceiveData.Length) % this.ReceiveData.Length;//头和尾之间的数据为未处理的数据

                            int dataLength = 7;
                            while (count >= dataLength)
                            {
                                if ((this.ReceiveData[this.RecDataTail] == (byte)0x55) &&
                                    (this.ReceiveData[(this.RecDataTail + 1) % this.ReceiveData.Length] == (byte)0x0a))
                                {
                                    byte[] data = new byte[dataLength];
                                    for (int i = 0; i < dataLength; i++)
                                    {
                                        data[i] = this.ReceiveData[(this.RecDataTail + i) % this.ReceiveData.Length];
                                    }

                                    #region CRC16校验

                                    byte[] crcData = new byte[dataLength - 2];
                                    Array.Copy(data, crcData, dataLength - 2);
                                    var crcResult = CRC.CRC16(crcData);
                                    if (crcResult[0] != data[dataLength - 2] || crcResult[1] != data[dataLength - 1])
                                    {
                                        ShowLog_MONI($"【{MONIPort.PortName}收到数据】CRC校验失败 " + PublicMethod.ByteArrayToHexStr(data));
                                        count = count - dataLength;
                                        this.RecDataTail = (this.RecDataTail + dataLength) % this.ReceiveData.Length;
                                        continue;
                                    }

                                    #endregion

                                    //收到正确数据
                                    ReceiveRightDataTime = DateTime.Now;

                                    count = count - dataLength;


                                    //Console.WriteLine(DateTime.Now.ToString());
                                    ShowLog_MONI($"【{MONIPort.PortName}收到数据】 " + PublicMethod.ByteArrayToHexStr(data));

                                    //解析速度 注意高低位
                                    byte[] speedArray = new byte[2];
                                    speedArray[0] = data[4];
                                    speedArray[1] = data[3];
                                    VM.MD.Speed = BitConverter.ToUInt16(speedArray) / 10000f;
                                    //解析电机控制命令
                                    VM.MD.State_IsRun = data[2];

                                    this.RecDataTail = (this.RecDataTail + dataLength) % this.ReceiveData.Length;
                                }
                                else
                                {
                                    count--;
                                    this.RecDataTail = (this.RecDataTail + 1) % this.ReceiveData.Length;
                                }
                            }

                            #endregion
                        }
                    }

                }
                catch (Exception ex)
                {
                    //LogHelper.Error(ex);
                }

                Thread.Sleep(20);
            }
        }

        /// <summary>
        /// 收到正确数据时刻
        /// </summary>
        public DateTime ReceiveRightDataTime { get; set; }

        /// <summary> 
        /// 字节数组转16进制字符串 
        /// </summary> 
        /// <param name="bytes"></param> 
        /// <returns></returns> 
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

        public byte DataCheck(byte[] crcData)
        {
            //byte[] crcData = { 0x5A, 0xD6, 0x01, 0xC1, 0x5B, 0x19, 0xD1, 0xC3, 0x53, 0xF8, 0xC8, 0xFD, 0x08, 0xC0, 0x80, 0x01, 0xA5 };
            byte result = 0;
            for (int i = 0; i < crcData.Length; i++)
            {
                result += crcData[i];
            }

            //result = (byte)~result;
            return (byte)result;
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


        /// <summary>
        /// 设备模拟器-输出
        /// </summary>
        /// <param name="info"></param>
        /// <param name="type"></param>
        public void ShowLog_MONI(string info, int type = 1)
        {
            try
            {
                if (!VM.MoniOuputIsShowSend && type == 3)
                {
                    return;
                }
                if (!VM.MoniOuputIsShowReceive && type == 2)
                {
                    return;
                }
                Brush brush = null;
                switch (type)
                {
                    case 1:
                        info = DateTime.Now.ToString("HH:mm:ss.fff") + " - " + "系统：" + info;
                        brush = Brushes.Black;
                        break;
                    case 2:
                        info = DateTime.Now.ToString("HH:mm:ss.fff") + " - " + "接收数据：" + info;
                        brush = Brushes.Green;
                        break;
                    case 3:
                        info = DateTime.Now.ToString("HH:mm:ss.fff") + " - " + "发送数据：" + info;
                        brush = Brushes.OrangeRed;
                        break;
                    case 4:
                        info = DateTime.Now.ToString("HH:mm:ss.fff") + " - " + "异常信息：" + info + "\r\n请稍后重试";
                        brush = Brushes.Red;
                        break;
                    default:
                        break;
                }

                txtConsole_moni.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (flowdoc_moni.Blocks.Count > 400)
                    {
                        flowdoc_moni.Blocks.Clear();
                    }
                    lock (this)
                    {
                        var p = new Paragraph(); // Paragraph 类似于 html 的 P 标签  
                        var r = new Run(info); // Run 是一个 Inline 的标签  
                        p.Inlines.Add(r);
                        p.Margin = new Thickness(2);
                        p.Foreground = brush;//设置字体颜色  
                        flowdoc_moni.Blocks.Add(p);
                        txtConsole_moni.ScrollToEnd();
                    }
                }));
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        #endregion

    }


    /// <summary>
    /// 界面ViewModel
    /// </summary>
    public class VMDebug : NotifyPropertyClass
    {
        #region ■------------------ 程序

        private bool _console_IsStart = true;
        public bool ConsoleIsStart
        {
            get { return _console_IsStart; }
            set
            {
                _console_IsStart = value;
                NotifyPropertyChanged("ConsoleIsStart");
            }
        }

        private bool _console_IsShowReceive = true;

        public bool ConsoleIsShowReceive
        {
            get { return _console_IsShowReceive; }
            set
            {
                _console_IsShowReceive = value;
                NotifyPropertyChanged("ConsoleIsShowReceive");
            }
        }

        private bool _console_IsShowSend = false;

        public bool ConsoleIsShowSend
        {
            get { return _console_IsShowSend; }
            set
            {
                _console_IsShowSend = value;
                NotifyPropertyChanged("ConsoleIsShowSend");
            }
        }


        private string _portName;

        /// <summary>
        /// 当前连接的串口名称
        /// </summary>
        public string PortName
        {
            get { return _portName; }
            set { _portName = value; NotifyPropertyChanged("PortName"); }
        }


        #endregion

        #region ■------------------ 设备模拟器

        /// <summary>
        /// 模拟数据
        /// </summary>
        public DeviceData MD { get; set; } = new DeviceData();

        private bool _isMoNiWalk;
        /// <summary>
        /// 是否模拟行走
        /// </summary>
        public bool IsMoniWalk
        {
            get { return _isMoNiWalk; }
            set { _isMoNiWalk = value; NotifyPropertyChanged("IsMoniWalk"); }
        }

        private float _WalkTimeMultiplier = 1;
        /// <summary>
        /// 行走时间倍数
        /// </summary>
        public float WalkTimeMultiplier
        {
            get { return _WalkTimeMultiplier; }
            set
            {
                if (value > 12) value = 12;
                if (value < 0.5) value = 0.5f;
                _WalkTimeMultiplier = value;
                NotifyPropertyChanged("WalkTimeMultiplier");
            }
        }

        private bool _MoniOuputIsShowSend = true;
        /// <summary>
        /// 模拟输出是否输出发送
        /// </summary>
        public bool MoniOuputIsShowSend
        {
            get { return _MoniOuputIsShowSend; }
            set
            {
                _MoniOuputIsShowSend = value;
                NotifyPropertyChanged("MoniOuputIsShowSend");
            }
        }

        private bool _MoniOuputIsShowReceive = true;
        /// <summary>
        /// 模拟输出是否输出发送
        /// </summary>
        public bool MoniOuputIsShowReceive
        {
            get { return _MoniOuputIsShowReceive; }
            set
            {
                _MoniOuputIsShowReceive = value;
                NotifyPropertyChanged("MoniOuputIsShowReceive");
            }
        }

        #endregion

    }
}
