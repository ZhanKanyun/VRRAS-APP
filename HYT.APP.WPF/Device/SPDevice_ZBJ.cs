using CL.Common;
using CL.Common.Data;
using System;
using System.IO.Ports;
using System.Threading;

namespace HYT.APP.WPF.Device
{
    /// <summary>
    /// 走步机
    /// 负责通信自动连接、状态维护、数据接收和发送
    /// </summary>
    public class SPDevice_ZBJ:SPDeviceBase
    {
        #region ■------------------ 单例

        private SPDevice_ZBJ()
        {
            _timerMain.Elapsed += _timerMain_Elapsed;
        }
        public readonly static SPDevice_ZBJ Instance = new SPDevice_ZBJ();
        #endregion

        #region ■------------------ 计时器

        private System.Timers.Timer _timerMain = new System.Timers.Timer(200);

        private void _timerMain_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                //Console.WriteLine(DeviceState);
                switch (DeviceState)
                {
                    case DeviceStates.CheckStart:
                        {
                            if (COM == "")//检测串口
                            {
                                SPCheckManage.Instance.StartTask(new SPCheckTask()
                                {
                                    ID = "走步机",
                                    DataFormat = new byte[] { 0x55, 0x0A, 0x00, 0x00, 0x00, 0x00, 0x00,0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },//new byte[] { 0x5A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xA5 },
                                    Callback = (com) =>
                                    {
                                        COM = com;
                                        DeviceState = DeviceStates.Connecting;
                                        ShowLog(new DeviceLog("检测到串口" + com));
                                    }
                                });
                                DeviceState = DeviceStates.CheckWait;
                            }
                        }
                        break;
                    case DeviceStates.CheckWait:
                        {
                            //ShowLog(new DeviceLog(SPCheckManage.Instance.LogLast + SPCheckManage.Instance.LastData));
                        }
                        break;
                    case DeviceStates.Connecting:
                        {
                            if (SPort == null)
                            {
                                SPort = new SerialPort();
                                SPort.PortName = COM;
                                SPort.BaudRate = BaudRate;
                                SPort.DataBits = 8;
                                SPort.StopBits = StopBits.One;
                                SPort.Parity = Parity.None;
                                SPort.ReadTimeout = 2000;
                            }

                            try
                            {
                                if (SPort != null && !SPort.IsOpen)
                                {
                                    SPort.Open();
                                    if (SPort.IsOpen)
                                    {
                                        ShowLog($"{NameCOM} 打开成功");
                                        ReceiveRightDataTime = DateTime.Now;
                                        DeviceState = DeviceStates.Run;
                                        ConnectSuccess(COM);
                              
                                    }
                                    else
                                    {
                                        ShowLog($"{NameCOM} 打开失败");
                                    }
                                }

                            }
                            catch (Exception ex)
                            {

                            }
                        }
                        break;
                    case DeviceStates.Run:
                        {
                            //超时未收到数据：拔掉、关掉、
                            // 未超时重新开启? 可以连上
                            // 未超时重新插上？需要等待超时
                            if ((DateTime.Now - ReceiveRightDataTime) > TimeSpan.FromSeconds(3))
                            {
                                ShowLog($"{NameCOM}工作状态  {3} 秒未收到数据，断开重连");
                                ReStart();
                            }
                        }
                        break;

                    default:
                        break;
                }

                Thread.Sleep(10);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        #endregion

        #region ■------------------ 外部接口

        public override void Start(int baudRate)
        {
            try
            {
                base.Start(baudRate);
                if (DeviceState == DeviceStates.None)
                {
                    if (!_timerMain.Enabled)
                    {
                        _timerMain.Start();
                    }
                    _dataReceiveThreadState = 1;
                    _dataReceiveThread = new Thread(DataReceive_Thread);
                    _dataReceiveThread.Start();
                    DeviceState = DeviceStates.CheckStart;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        public bool IsStart
        {
            get
            {
                return _timerMain.Enabled;
            }
        }

        public override void Stop()
        {
            try
            {
                if (DeviceState != DeviceStates.None)
                {
                    base.Stop();
                    if (_timerMain.Enabled)
                    {
                        _timerMain.Stop();
                    }
                    _dataReceiveThreadState = 0;
                    _dataReceiveThread = null;
                    DeviceState = DeviceStates.None;
                    COM = "";
                    if (SPort != null)
                    {
                        SPort.Close();
                        SPort = null;
                    }

                    ReceiveBuff = new byte[2048];
                    ReceiveData = new byte[2048];
                    RecDataHead = 0;
                    RecDataTail = 0;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
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
            while (this._dataReceiveThreadState == 1)
            {
                try
                {
                    if (SPort != null)
                    {
                        if ( SPort.IsOpen&& SPort.BytesToRead > 0 )
                        {

                            #region 〓〓〓〓〓〓〓 缓存接收数据-2048字节

                            int len = SPort.Read(this.ReceiveBuff, 0, 1024);//从串口缓冲区读取数据，返回实际读取到的字节数
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

                            int dataLength = 14;
                            while (count >= dataLength)
                            {
                                if ((this.ReceiveData[this.RecDataTail] == (byte)0x55) &&
                                    (this.ReceiveData[(this.RecDataTail + 1) % this.ReceiveData.Length] == (byte)0x0a) )
                                {
                                    byte[] data = new byte[dataLength];
                                    for (int i = 0; i < dataLength; i++)
                                    {
                                        data[i] = this.ReceiveData[(this.RecDataTail + i) % this.ReceiveData.Length];
                                    }

                                    #region CRC16校验

                                    byte[] crcData= new byte[dataLength-2];
                                    Array.Copy(data,crcData, dataLength-2);
                                    var crcResult = CRC.CRC16(crcData);
                                    if (crcResult[0]!= data[dataLength - 2] || crcResult[1] != data[dataLength-1])
                                    {
                                        ShowLog(new DeviceLog(2, $"【{NameCOM}收到数据】CRC校验失败 " + PublicMethod.ByteArrayToHexStr(data)));
                                        count = count - dataLength;
                                        this.RecDataTail = (this.RecDataTail + dataLength) % this.ReceiveData.Length;
                                        continue;
                                    }

                                    #endregion

                                    //收到正确数据
                                    ReceiveRightDataTime = DateTime.Now;
                         
                                    count = count - dataLength;

                                    OnDataReceive(data, DeviceDataTypes.Work_Data);
                                    //Console.WriteLine(DateTime.Now.ToString());
                                    ShowLog(new DeviceLog(2, $"【{NameCOM}收到数据】 " + PublicMethod.ByteArrayToHexStr(data)));

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

                Thread.Sleep(10);
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

        #region ■------------------ 数据到达事件通知

        /// <summary>
        /// 串口数据到达
        /// </summary>
        public event EventHandler<DeviceDataEventArgs> OnDataReceiveEvent;

        public void OnDataReceive(byte[] data, DeviceDataTypes type)
        {
            try
            {
                DeviceDataEventArgs e = new DeviceDataEventArgs();
                e.Data = data;
                e.COM = COM;
                e.Type = type;
                OnDataReceiveEvent?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        #endregion
    }
}
