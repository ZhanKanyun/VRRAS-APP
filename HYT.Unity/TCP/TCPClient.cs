using HPSocket;
using HPSocket.Tcp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace KT.TCP
{
    public class TCPClient
    {
        #region ■------------------ 单例

        private TCPClient()
        {
            Binding();
            NeedAckMsgManager.Instance.OnNeedSend -= Instance_OnNeedSend;
            NeedAckMsgManager.Instance.OnNeedSend += Instance_OnNeedSend;
        }

        private void Instance_OnNeedSend(object sender, TCPNeedAckMsg e)
        {
            SendServer(e.Msg);
        }

        public static readonly TCPClient Instance = new TCPClient();

        public TrainStartInfo TrainInfo { get; set; }

        #endregion

        #region ■------------------ 客户端

        /// <summary>
        /// 封包, 做粘包用
        /// </summary>
        private readonly List<byte> _packetData = new List<byte>();

        /// <summary>
        /// 最大封包长度
        /// </summary>
        private const int MaxPacketSize = 10240;

        /// <summary>
        /// 是否输出服务器事件日志
        /// </summary>
        public bool IsLogEvent { get; set; } = false;

        public bool IsLogData_Send { get; set; } = false;
        public bool IsLogData_Receive { get; set; } = false;

        /// <summary>
        /// 客户端对象
        /// </summary>
        public ITcpClient _tcpClient = new TcpClient();

        private void Binding()
        {
            _tcpClient.OnPrepareConnect += OnPrepareConnect;
            _tcpClient.OnConnect += OnConnect;
            _tcpClient.OnReceive += OnReceive;
            _tcpClient.OnClose += OnClose;
            _tcpClient.OnSend += OnSend;
        }

        private HandleResult OnPrepareConnect(IClient sender, IntPtr socket)
        {
            AddLog_Event($"OnPrepareConnect({sender.Address}:{sender.Port}), socket handle: {socket}, hp-socket version: {sender.Version}");
            return HandleResult.Ok;
        }

        private HandleResult OnConnect(IClient sender)
        {
            OnConnectSuccess?.Invoke(null, null);
            AddLog_Event("OnConnect()");
            _packetData.Clear();
            return HandleResult.Ok;
        }

        private HandleResult OnReceive(IClient sender, byte[] data)
        {

            string info = ByteArrayToHexStr(data);
            if (IsLogData_Receive)
            {
                AddLog_Data($"OnReceive, data length: {data.Length}  data= {info}");
            }

            _packetData.AddRange(data);

            // 总长度小于包头
            if (_packetData.Count < sizeof(int))
            {
                return HandleResult.Ok;
            }

            HandleResult result;
            const int headerLength = sizeof(int);
            do
            {
                // 取头部字节得到包头
                var packetHeader = _packetData.GetRange(0, headerLength).ToArray();

                // 两端字节序要保持一致
                // 如果当前环境不是小端字节序
                if (!BitConverter.IsLittleEndian)
                {
                    // 翻转字节数组, 变为小端字节序
                    Array.Reverse(packetHeader);
                }

                // 得到包头指向的数据长度
                var dataLength = BitConverter.ToInt32(packetHeader, 0);

                // 完整的包长度(含包头和完整数据的大小)
                var fullLength = dataLength + headerLength;

                if (dataLength < 0 || fullLength > MaxPacketSize)
                {
                    result = HandleResult.Error;
                    break;
                }

                // 如果来的数据小于一个完整的包
                if (_packetData.Count < fullLength)
                {
                    // 下次数据到达处理
                    result = HandleResult.Ignore;
                    break;
                }

                // 是不是一个完整的包(包长 = 实际数据长度 + 包头长度)
                if (_packetData.Count == fullLength)
                {
                    // 得到完整包并处理
                    var fullData = _packetData.GetRange(headerLength, dataLength).ToArray();
                    result = OnProcessFullPacket(fullData);
                    // 清空缓存
                    _packetData.Clear();
                    break;
                }

                // 如果来的数据比一个完整的包长
                if (_packetData.Count > fullLength)
                {
                    // 先得到完整包并处理
                    var fullData = _packetData.GetRange(headerLength, dataLength).ToArray();
                    result = OnProcessFullPacket(fullData);
                    if (result == HandleResult.Error)
                    {
                        break;
                    }
                    // 再移除已经读了的数据
                    _packetData.RemoveRange(0, fullLength);

                    // 剩余的数据下个循环处理
                }

            } while (true);


            return result;
        }

        private HandleResult OnClose(IClient sender, SocketOperation socketOperation, int errorCode)
        {
            OnConnectClose?.Invoke(null, null);
            _packetData.Clear();
            AddLog_Event($"OnClose(), socket operation: {socketOperation}, error code: {errorCode}");
            AddLog_Info("与服务器断开连接");
            return HandleResult.Ok;
        }

        private HandleResult OnSend(IClient sender, byte[] data)
        {
            string info = ByteArrayToHexStr(data);
            if (IsLogData_Send)
            {
                AddLog_Data($"OnSend, data length: {data.Length}  data = {info}");
            }
            return HandleResult.Ok;
        }



        /// <summary>
        /// 是否自动重连 如果是手动停止就不自动重连了
        /// </summary>
        private bool IsAutoReStart = true;

        /// <summary>
        /// 停止客户端
        /// </summary>
        private async void Stop(bool isAutoReStart = false)
        {
            try
            {

                // 等待服务停止
                await _tcpClient.StopAsync();

                IsAutoReStart = isAutoReStart;

            }
            catch (Exception ex)
            {
                AddLog_Error(ex);
            }
        }

        /// <summary>
        /// 消息处理
        /// </summary>
        public event EventHandler<TrainMsg> OnMsgProcess;
        /// <summary>
        /// 消息发送成功
        /// </summary>
        public event EventHandler<TrainMsg> OnMsgSend;

        /// <summary>
        /// 日志记录
        /// </summary>
        public event EventHandler<TCPLog> OnLog;

        /// <summary>
        /// 连接成功
        /// </summary>
        public event EventHandler OnConnectSuccess;

        /// <summary>
        /// 断开连接
        /// </summary>
        public event EventHandler OnConnectClose;

        #endregion

        #region ■------------------ 消息处理

        /// <summary>
        /// 处理消息次数
        /// </summary>
        public int ProcessFullPacketCount { get; private set; }

        private HandleResult OnProcessFullPacket(byte[] data)
        {
            ProcessFullPacketCount++;
            if (ProcessFullPacketCount > 9999999) ProcessFullPacketCount = 0;

            // 这里来的都是完整的包
            var packet = JsonConvert.DeserializeObject<Packet>(Encoding.UTF8.GetString(data));
            var result = HandleResult.Ok;
            switch (packet.Type)
            {
                case PacketType.TrainMsg:
                    {
                        try
                        {
                            var msg = JsonConvert.DeserializeObject<TrainMsg>(packet.Data);

                            if (msg.Type == TrainMsgTypes.S2C_TrainStartInfo)
                            {
                                TrainInfo = JsonConvert.DeserializeObject<TrainStartInfo>(msg.Data);
                            }

                            if (msg.Type == TrainMsgTypes.ALL_ACK)//对方确认接收
                            {
                                NeedAckMsgManager.Instance.ACK(msg.ID);
                            }
                            else//告诉对方确认接收
                            {
                                if (!string.IsNullOrEmpty(msg.ID) && msg.ID != "1")
                                {
                                    //恢复需要确认收到的消息
                                    SendServer(new TrainMsg()
                                    {
                                        ID = msg.ID,
                                        Type = TrainMsgTypes.ALL_ACK
                                    });
                                }
                            }
                            LastMsg = msg.ToString();
                            OnMsgProcess?.Invoke(null, msg);//训练自己处理
                        }
                        catch (Exception ex)
                        {
                            AddLog_Error(ex);
                        }
                    }
                    break;
                case PacketType.Echo: // 回显是个字符串显示操作
                    {
                        AddLog_Event($"OnProcessFullPacket(), type: {packet.Type}, content: {packet.Data}");
                        break;
                    }
                case PacketType.Time: // 获取客户端时间依然是个字符串操作^_^
                    {
                        AddLog_Event($"OnProcessFullPacket(), type: {packet.Type}, time: {packet.Data}");
                        break;
                    }
                default:
                    result = HandleResult.Error;

                    break;
            }
            return result;
        }

        public string LastMsg = "";
        public string LastSend = "";
        #endregion

        #region ■------------------ 发送消息

        /// <summary>
        /// 发送包头
        /// <para>固定包头网络字节序</para>
        /// </summary>
        /// <param name="dataLength">实际数据长度</param>
        /// <returns></returns>
        private bool SendPacketHeader(int dataLength)
        {
            // 包头转字节数组
            var packetHeaderBytes = BitConverter.GetBytes(dataLength);

            // 两端字节序要保持一致
            // 如果当前环境不是小端字节序
            if (!BitConverter.IsLittleEndian)
            {
                // 翻转字节数组, 变为小端字节序
                Array.Reverse(packetHeaderBytes);
            }

            return _tcpClient.Send(packetHeaderBytes, packetHeaderBytes.Length);
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="type"></param>
        /// <param name="data"></param>
        private void SendPacketType(PacketType type, string data)
        {
            if (!_tcpClient.HasStarted)
            {
                return;
            }

            // 组织封包, 取得要发送的数据
            var packet = new Packet
            {
                Type = type,
                Data = data,
            };

            var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(packet));

            // 发送包头, 发送失败断开连接
            if (!SendPacketHeader(bytes.Length))
            {
                _tcpClient.Stop();
                return;
            }

            // 发送实际数据, 发送失败断开连接
            if (!_tcpClient.Send(bytes, bytes.Length))
            {
                _tcpClient.Stop();
            }
        }

        /// <summary>
        /// 发送消息给客户端 单客户端模式
        /// </summary>
        public void SendServer(TrainMsg msg)
        {
            try
            {
                if (!_tcpClient.HasStarted)
                {
                    AddLog_Info("发送消息失败，未连接服务器");
                    return;
                }

                string data = JsonConvert.SerializeObject(msg);
                // 组织packet为一个json
                var json = JsonConvert.SerializeObject(new Packet
                {
                    Type = PacketType.TrainMsg,
                    Data = data,
                });
                // json转字节数组
                var bytes = Encoding.UTF8.GetBytes(json);
                msg.S = bytes.Length;
                // 先发包头
                if (!SendPacketHeader(bytes.Length))
                {
                    return;
                }

                // 再发实际数据
                if (!_tcpClient.Send(bytes, bytes.Length))
                {
                    return;
                }
                else
                {
                    LastSend = msg.ToString();
                    OnMsgSend?.Invoke(null, msg);
                }
            }
            catch (Exception ex)
            {
                AddLog_Error(ex);
            }
        }

        /// <summary>
        /// 发送并且需要接收端确认收到并回调
        /// </summary>
        /// <param name="msg">消息</param>
        /// <param name="callback">回调 无参数 无返回值</param>
        /// <param name="count">发送次数，只有当确认类型为Count相关的才有效</param>
        /// <param name="ackType">确认类型</param>
        public void SendServerCallback(TrainMsg msg, Action callback, int count = 10, NeedAckMsgTypes ackType = NeedAckMsgTypes.Infinite)
        {
            try
            {
                if (string.IsNullOrEmpty(msg.ID))
                {
                    msg.ID = Guid.NewGuid().ToString();
                }
                TCPNeedAckMsg ack = new TCPNeedAckMsg();
                ack.NeedAckMsgType = ackType;
                ack.CountNeed = count;
                ack.Callback = callback;
                ack.Msg = msg;
                NeedAckMsgManager.Instance.Add(ack);
            }
            catch (Exception ex)
            {
                AddLog_Error(ex);
            }
        }

        #endregion

        #region ■------------------ 外部接口

        /// <summary>
        /// 客户端初始化
        /// </summary>
        /// <param name="ip">IP地址</param>
        /// <param name="port">端口</param>
        /// <param name="socketBufferSize">通信数据缓存区大小</param>
        public void Start(string ip, ushort port, uint socketBufferSize = 6144)
        {
            AddLog_Info($"客户端组件启动，连接 {ip}:{port}");
            _tcpClient.Address = ip;// "127.0.0.1";
            _tcpClient.Port = port;// 8888;
            _tcpClient.SocketBufferSize = socketBufferSize;
            _tcpClient.Async = true;

            StartTimer();
        }

        #endregion

        #region ■------------------ 内部接口

        public string LastLog = "";

        private void AddLog_Error(Exception ex)
        {
            TCPLog temp = new TCPLog()
            {
                type = typeof(TcpClient),
                LogType = TCPLogTypes.Error,
                Log = ex.Message + "/r/n" + ex.StackTrace
            };
            OnLog?.Invoke(null, temp);
        }

        private void AddLog_Info(string log)
        {
            LastLog = log;
            TCPLog temp = new TCPLog()
            {
                type = typeof(TcpClient),
                LogType = TCPLogTypes.Info,
                Log = log
            };
            OnLog?.Invoke(null, temp);
        }

        private void AddLog_Event(string log)
        {
            if (!IsLogEvent)
            {
                return;
            }
            LastLog = log;
            TCPLog temp = new TCPLog()
            {
                type = typeof(TcpClient),
                LogType = TCPLogTypes.Info,
                Log = log
            };
            OnLog?.Invoke(null, temp);
        }

        private void AddLog_Data(string log)
        {
            LastLog = log;
            TCPLog temp = new TCPLog()
            {
                type = typeof(TcpClient),
                LogType = TCPLogTypes.Info,
                Log = log
            };
            OnLog?.Invoke(null, temp);
        }

        /// <summary> 
        /// 字节数组转16进制字符串 
        /// </summary> 
        /// <param name="bytes"></param> 
        /// <returns></returns> 
        private static string ByteArrayToHexStr(byte[] bytes)
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

        #region ■------------------ 计时器：连接服务器控制 重发机制

        private System.Timers.Timer _timerServer = new System.Timers.Timer(1000);

        private void StartTimer()
        {
            _timerServer.Elapsed += _timerServer_Tick;
            _timerServer.Start();
        }

        /// <summary>
        /// 客户端重连倒计时
        /// </summary>
        private long _countDownReStart;

        /// <summary>
        /// 客户端计时器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _timerServer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (!_tcpClient.IsConnected)
                {
                    if (_countDownReStart > 0)
                    {
                        _countDownReStart--;
                    }
                    else
                    {
                        //连接服务器
                        if (!_tcpClient.Connect())
                        {
                            _countDownReStart = 10;//倒计时多少秒再连接
                            AddLog_Info($"连接服务器失败，error code: {_tcpClient.ErrorCode}, error message: {_tcpClient.ErrorMessage}");
                        }
                        else
                        {
                            AddLog_Info($"连接服务器成功，{_tcpClient.Address}:{_tcpClient.Port}");
                        }
                    }
                }

                //检测父进程退出 此进程退出
                try
                {
                    if (TrainInfo != null && TrainInfo.ParentProcessID != -1)
                    {
                        Process process = Process.GetProcessById(TrainInfo.ParentProcessID);
                        if (process != null)
                        {
                            if (process.HasExited)
                            {
                                AddLog_Info("父进程已退出，关闭训练");
                                Process.GetCurrentProcess().Kill();
                            }
                        }
                        else
                        {
                            AddLog_Info("父进程已不在，关闭训练");
                            Process.GetCurrentProcess().Kill();
                        }
                    }
                }
                catch (Exception ex)
                {
                    AddLog_Error(ex);
                }

                //重发机制
                NeedAckMsgManager.Instance.Update();
            }
            catch (Exception ex)
            {
                AddLog_Error(ex);
            }
        }

        #endregion


        #region ■------------------ 重发机制

        public List<TCPNeedAckMsg> NeedAckMsgs = new List<TCPNeedAckMsg>();

        #endregion


        //public string State
        //{
        //    get
        //    {
        //        return $"ClientState : connID = {_tcpClient.ConnectionId}  State = {_tcpClient.State}  IsConnected = {_tcpClient.IsConnected}  HasStarted = {_tcpClient.HasStarted}";
        //    }
        //}

        public string State
        {
            get
            {
                switch (_tcpClient.State)
                {
                    case ServiceState.Starting:
                        return $"C - A - {_tcpClient.Port} ";
                    case ServiceState.Started:
                        return $"C - B - {_tcpClient.Port} ";
                    case ServiceState.Stopping:
                        return $"C - C - {_tcpClient.Port} ";
                    case ServiceState.Stopped:
                        return $"C - D - {_tcpClient.Port} ";
                    default:
                        return $"C - E - {_tcpClient.State} ";
                }
            }
        }
    }
}
