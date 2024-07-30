using HPSocket;
using HPSocket.Tcp;
using HPSocket.Thread;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KT.TCP
{
    public class TCPServer
    {
        #region ■------------------ 单例

        private TCPServer()
        {
            Binding();
            NeedAckMsgManager.Instance.OnNeedSend -= Instance_OnNeedSend;
            NeedAckMsgManager.Instance.OnNeedSend += Instance_OnNeedSend;
        }

        public static readonly TCPServer Instance = new TCPServer();

        private void Instance_OnNeedSend(object sender, TCPNeedAckMsg e)
        {
            SendClient(e.Msg);
        }

        #endregion

        #region ■------------------ 服务器

        /// <summary>
        /// 初始端口 启动管理组件时设置的
        /// </summary>
        public ushort InitPort { get; private set; }

        /// <summary>
        /// 当前服务器使用的端口
        /// </summary>
        public ushort CurrentPort
        {
            get
            {
                return _tcpServer.Port;
            }
        }

        /// <summary>
        /// 线程池
        /// </summary>
        private readonly ThreadPool _threadPool = new ThreadPool();

        /// <summary>
        /// 线程池回调函数
        /// </summary>
        private TaskProcEx _taskTaskProc;

        /// <summary>
        /// 最大封包长度 4096
        /// </summary>
        private const int MaxPacketSize = 10240;

        public bool IsStarted
        {
            get { return _tcpServer.HasStarted; }
        }

        /// <summary>
        /// 服务器对象
        /// </summary>
        private ITcpServer _tcpServer = new TcpServer();

        /// <summary>
        /// 暂时只用最后连接的客户端
        /// </summary>
        private IntPtr _connId = IntPtr.Zero;

        /// <summary>
        /// 是否输出服务器事件日志
        /// </summary>
        public bool IsLogEvent { get; set; } = false;

        public bool IsLogData_Send { get; set; } = false;
        public bool IsLogData_Receive { get; set; } = false;

        private void Binding()
        {
            _tcpServer.OnPrepareListen += TCPServer_OnPrepareListen;//准备监听事件
            _tcpServer.OnAccept += TCPServer_OnAccept;//连接到达事件
            _tcpServer.OnSend += TCPServer_OnSend;//数据包发送事件
            _tcpServer.OnReceive += TCPServer_OnReceive;//数据到达事件
            _tcpServer.OnClose += TCPServer_OnClose;//连接关闭事件
            _tcpServer.OnShutdown += TCPServer_OnShutdown;//服务器关闭事件
            _tcpServer.OnHandShake += TCPServer_OnHandShake;//握手成功事件

            _taskTaskProc = TaskTaskProc;
        }

        private HandleResult TCPServer_OnPrepareListen(IServer sender, IntPtr listen)
        {
            AddLog_Event($"OnPrepareListen({sender.Address}:{sender.Port}), listen: {listen}, hp-socket version: {sender.Version}");
            return HandleResult.Ok;
        }

        private HandleResult TCPServer_OnAccept(IServer sender, IntPtr connId, IntPtr client)
        {
            // 记住最新连接，断开之前的连接 单客户端
            if (_connId != IntPtr.Zero)
            {
                _tcpServer.Disconnect(_connId);
            }
            _connId = connId;

            // 获取客户端地址
            if (!sender.GetRemoteAddress(connId, out var ip, out var port))
            {
                return HandleResult.Error;
            }

            AddLog_Event($"OnAccept({connId}), ip: {ip}, port: {port}");
            AddLog_Info($"【服务器】客户端连接 ({connId}), ip: {ip}, port: {port}");
            // 设置附加数据, 用来做粘包处理
            sender.SetExtra(connId, new ClientInfo
            {
                ConnId = connId,
                PacketData = new List<byte>(),
            });
            OnClientConnect?.Invoke(null, connId);
            return HandleResult.Ok;
        }

        private HandleResult TCPServer_OnHandShake(IServer sender, IntPtr connId)
        {
            AddLog_Event($"OnHandShake");
            return HandleResult.Ok;
        }

        private HandleResult TCPServer_OnShutdown(IServer sender)
        {
            AddLog_Event($"OnShutdown({sender.Address}:{sender.Port})");
            AddLog_Info($"【服务器】服务器关闭 {sender.Address}:{sender.Port}");
            return HandleResult.Ok;
        }

        private HandleResult TCPServer_OnClose(IServer sender, IntPtr connId, SocketOperation socketOperation, int errorCode)
        {
            AddLog_Event($"OnClose({connId}), socket operation: {socketOperation}, error code: {errorCode}");
            AddLog_Info($"【服务器】客户端断开连接({connId}) error code: {errorCode}");
            if (_connId == connId) _connId = IntPtr.Zero;

            var client = sender.GetExtra<ClientInfo>(connId);
            if (client != null)
            {
                sender.RemoveExtra(connId);
                client.PacketData.Clear();
                return HandleResult.Error;
            }

            return HandleResult.Ok;
        }

        private HandleResult TCPServer_OnReceive(IServer sender, IntPtr connId, byte[] data)
        {
            try
            {
                string info = ByteArrayToHexStr(data);
                if (IsLogData_Receive)
                {
                    AddLog_Data($"OnReceive({connId}), data length: {data.Length}  data = {info}");
                }


                var client = sender.GetExtra<ClientInfo>(connId);
                if (client == null)
                {
                    return HandleResult.Error;
                }

                client.PacketData.AddRange(data);

                // 总长度小于包头
                if (client.PacketData.Count < sizeof(int))
                {
                    return HandleResult.Ok;
                }

                HandleResult result;
                const int headerLength = sizeof(int);
                do
                {
                    // 取头部字节得到包头
                    var packetHeader = client.PacketData.GetRange(0, headerLength).ToArray();

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

                    //限制包长度
                    if (dataLength < 0 || fullLength > MaxPacketSize)
                    {
                        result = HandleResult.Error;
                        break;
                    }

                    // 如果来的数据小于一个完整的包
                    if (client.PacketData.Count < fullLength)
                    {
                        // 下次数据到达处理
                        result = HandleResult.Ignore;
                        break;
                    }

                    // 是不是一个完整的包(包长 = 实际数据长度 + 包头长度)
                    if (client.PacketData.Count == fullLength)
                    {
                        // 得到完整包并处理
                        var fullData = client.PacketData.GetRange(headerLength, dataLength).ToArray();
                        result = OnProcessFullPacket(sender, client, fullData);

                        // 清空缓存
                        client.PacketData.Clear();
                        break;
                    }

                    // 如果来的数据比一个完整的包长
                    if (client.PacketData.Count > fullLength)
                    {
                        // 先得到完整包并处理
                        var fullData = client.PacketData.GetRange(headerLength, dataLength).ToArray();
                        result = OnProcessFullPacket(sender, client, fullData);
                        if (result == HandleResult.Error)
                        {
                            break;
                        }
                        // 再移除已经读了的数据
                        client.PacketData.RemoveRange(0, fullLength);

                        // 剩余的数据下个循环处理
                    }

                } while (true);


                return result;
            }
            catch (Exception)
            {
                return HandleResult.Error;
            }
        }

        private HandleResult TCPServer_OnSend(IServer sender, IntPtr connId, byte[] data)
        {
            if (IsLogData_Send)
            {
                AddLog_Data($"OnSend, data length: {data.Length}  data = {ByteArrayToHexStr(data)}");
            }
            return HandleResult.Ok;
        }

        /// <summary>
        /// 线程池任务回调函数
        /// </summary>
        /// <param name="obj">任务参数</param>
        private void TaskTaskProc(object obj)
        {
            if (!(obj is TaskInfo taskInfo))
            {
                return;
            }

            // 如果连接已经断开了(可能被踢了)
            // 它的任务就不做了(根据自己业务需求来, 也许你的任务就是要完成每个连接的所有任务, 每个包都要处理, 不管连接断开与否, 就不要写这个判断, 但是你回发包的时候要判断是否有效连接)
            if (!_tcpServer.IsConnected(taskInfo.Client.ConnId))
            {
                return;
            }

            // 在这里处理耗时任务逻辑

            switch (taskInfo.Packet.Type)
            {
                case PacketType.Time:
                    {
                        // 模拟耗时操作
                        System.Threading.Thread.Sleep(6000);

                        // 组织packet为一个json
                        var json = JsonConvert.SerializeObject(new Packet
                        {
                            Type = PacketType.Time,
                            Data = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        });

                        // json转字节数组
                        var bytes = Encoding.UTF8.GetBytes(json);

                        // 先发包头
                        if (!SendPacketHeader(_tcpServer, taskInfo.Client.ConnId, bytes.Length))
                        {
                            // 发送失败断开连接
                            _tcpServer.Disconnect(taskInfo.Client.ConnId);
                            break;
                        }

                        // 再发实际数据
                        if (!_tcpServer.Send(taskInfo.Client.ConnId, bytes, bytes.Length))
                        {
                            _tcpServer.Disconnect(taskInfo.Client.ConnId);
                        }

                        // 至此完成当前任务
                        break;
                    }
            }
        }



        /// <summary>
        /// 是否自动重连 如果是手动停止就不自动重连了
        /// </summary>
        private bool IsAutoReStart = true;

        /// <summary>
        /// 停止服务器
        /// </summary>
        private async void Stop(bool isAutoReStart = false)
        {
            try
            {
                // 停止并等待线程池任务全部完成
                await _threadPool.StopAsync();

                // 等待服务停止
                await _tcpServer.StopAsync();

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
        /// 客户端连接
        /// </summary>
        public event EventHandler<IntPtr> OnClientConnect;

        #endregion

        #region ■------------------ 消息处理

        /// <summary>
        /// 处理消息次数
        /// </summary>
        public int ProcessFullPacketCount { get; private set; }

        private HandleResult OnProcessFullPacket(IServer sender, ClientInfo client, byte[] data)
        {
            ProcessFullPacketCount++;
            if (ProcessFullPacketCount > 9999999) ProcessFullPacketCount = 0;

            // 这里来的都是完整的包, 但是这里不做耗时操作, 仅把数据放入队列
            var packet = JsonConvert.DeserializeObject<Packet>(Encoding.UTF8.GetString(data));

            var result = HandleResult.Ok;
            switch (packet.Type)
            {
                case PacketType.TrainMsg:
                    {
                        try
                        {
                            var msg = JsonConvert.DeserializeObject<TrainMsg>(packet.Data);

                            if (msg.Type == TrainMsgTypes.ALL_ACK)//对方确认接收
                            {
                                NeedAckMsgManager.Instance.ACK(msg.ID);
                            }
                            else//告诉对方确认接收
                            {
                                if (!string.IsNullOrEmpty(msg.ID) && msg.ID != "1")
                                {
                                    //恢复需要确认收到的消息
                                    SendClient(new TrainMsg(TrainMsgTypes.ALL_ACK)
                                    {
                                        ID = msg.ID
                                    });
                                }
                            }
                            OnMsgProcess?.Invoke(null, msg);//训练自己处理 
                        }
                        catch (Exception ex)
                        {
                            AddLog_Error(ex);
                        }
                    }
                    break;
                case PacketType.Echo: // 假如回显是一个非耗时操作, 在这处理
                    {
                        // 组织packet为一个json
                        var json = JsonConvert.SerializeObject(new Packet
                        {
                            Type = packet.Type,
                            Data = packet.Data,
                        });

                        // json转字节数组
                        var bytes = Encoding.UTF8.GetBytes(json);

                        // 先发包头
                        if (!SendPacketHeader(sender, client.ConnId, bytes.Length))
                        {
                            result = HandleResult.Error;
                            break;
                        }

                        // 再发实际数据
                        if (!sender.Send(client.ConnId, bytes, bytes.Length))
                        {
                            result = HandleResult.Error;
                        }

                        // 至此完成回显
                        break;
                    }
                case PacketType.Time: // 假如获取服务器时间是耗时操作, 将该操作放入队列
                    {
                        // 向线程池提交任务
                        if (!_threadPool.Submit(_taskTaskProc, new TaskInfo
                        {
                            Client = client,
                            Packet = packet,
                        }))
                        {
                            result = HandleResult.Error;
                        }

                        break;
                    }
                default:
                    result = HandleResult.Error;
                    break;
            }
            return result;
        }

        #endregion

        #region ■------------------ 发送消息

        /// <summary>
        /// 发送包头
        /// <para>固定包头网络字节序</para>
        /// </summary>
        /// <param name="sender">服务器组件</param>
        /// <param name="connId">连接id</param>
        /// <param name="dataLength">实际数据长度</param>
        /// <returns></returns>
        private bool SendPacketHeader(IServer sender, IntPtr connId, int dataLength)
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

            return sender.Send(connId, packetHeaderBytes, packetHeaderBytes.Length);
        }

        /// <summary>
        /// 发送消息给客户端 单客户端模式
        /// </summary>
        public bool SendClient(TrainMsg msg)
        {
            try
            {
                if (_connId != IntPtr.Zero)
                {
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
                    if (!SendPacketHeader(_tcpServer, _connId, bytes.Length))
                    {
                        return false;
                    }

                    // 再发实际数据
                    if (!_tcpServer.Send(_connId, bytes, bytes.Length))
                    {
                        return false;
                    }
                    OnMsgSend?.Invoke(null, msg);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                AddLog_Error(ex);
                return false;
            }
        }

        /// <summary>
        /// 发送并且需要接收端确认收到并回调
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public bool SendClientCallback(TrainMsg msg, Action callback, int count = 10, NeedAckMsgTypes ackType = NeedAckMsgTypes.Infinite)
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

                if (_connId != IntPtr.Zero)
                {
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
                    if (!SendPacketHeader(_tcpServer, _connId, bytes.Length))
                    {
                        return false;
                    }

                    // 再发实际数据
                    if (!_tcpServer.Send(_connId, bytes, bytes.Length))
                    {
                        return false;
                    }
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                AddLog_Error(ex);
                return false;
            }
        }

        #endregion

        #region ■------------------ 外部接口

        /// <summary>
        /// 服务器初始化
        /// </summary>
        /// <param name="ip">服务器地址</param>
        /// <param name="port">服务器初始端口，如果被占用就递增</param>
        /// <param name="socketBufferSize"></param>
        public void Start(string ip, ushort port, uint socketBufferSize = 6144)
        {
            AddLog_Info($"【服务器】管理器启动");
            StartTimer();

            if (!IsStarted)//服务器已经在运行
            {
                InitPort = port;
                _tcpServer.Address = ip;// "127.0.0.1";
                _tcpServer.Port = port;// 8888;
                _tcpServer.SocketBufferSize = socketBufferSize;
            }
        }

        public void Stop()
        {
            try
            {
                if (_timerServer != null)
                {
                    _timerServer.Stop();
                    _timerServer.Dispose();
                }

            }
            catch (Exception ex)
            {
                AddLog_Error(ex);
            }

            try
            {
                if (_tcpServer != null)
                {
                    _tcpServer.Stop();
                    _tcpServer.Dispose();
                }
            }
            catch (Exception ex)
            {
                AddLog_Error(ex);
            }
        }

        #endregion

        #region ■------------------ 内部接口

        private void AddLog_Error(Exception ex)
        {
            TCPLog temp = new TCPLog()
            {
                type = typeof(TCPServer),
                LogType = TCPLogTypes.Error,
                Log = ex.Message + "/r/n" + ex.StackTrace
            };
            OnLog?.Invoke(null, temp);
        }

        private void AddLog_Info(string log)
        {
            Console.WriteLine(log);
            TCPLog temp = new TCPLog()
            {
                type = typeof(TCPServer),
                LogType = TCPLogTypes.Info,
                Log = log
            };
            OnLog?.Invoke(null, temp);
        }

        private void AddLog_Event(string log)
        {
            if (!IsLogEvent) return;
            Console.WriteLine(log);
            TCPLog temp = new TCPLog()
            {
                type = typeof(TCPServer),
                LogType = TCPLogTypes.Info,
                Log = log
            };
            OnLog?.Invoke(null, temp);
        }

        private void AddLog_Data(string log)
        {
            Console.WriteLine(log);
            TCPLog temp = new TCPLog()
            {
                type = typeof(TCPServer),
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

        #region ■------------------ 计时器：服务器启动管理（自动）

        private System.Timers.Timer _timerServer = new System.Timers.Timer(1000);

        /// <summary>
        /// 启动计时器
        /// </summary>
        private void StartTimer()
        {
            if (!_timerServer.Enabled)
            {
                _timerServer.Elapsed -= _timerServer_Tick;
                _timerServer.Elapsed += _timerServer_Tick;
                _timerServer.Start();
            }
        }

        /// <summary>
        /// 服务器启动倒计时  首次不倒计时
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
                if (!_tcpServer.HasStarted)
                {
                    if (_countDownReStart > 0)
                    {
                        _countDownReStart--;
                        AddLog_Info($"【服务器】检测到服务器未启动，启动倒计时 {_countDownReStart} 秒");
                    }
                    else
                    {
                        AddLog_Info($"【服务器】开始启动服务器 {_tcpServer.Address}:{_tcpServer.Port}");

                        // 2个线程处理耗时操作, 作为相对耗时的任务, 可根据业务需求多开线程处理
                        if (!_threadPool.Start(2, RejectedPolicy.WaitFor))
                        {
                            _countDownReStart = 10;
                            AddLog_Info($"【服务器】线程池启动失败, error code: {_threadPool.SysErrorCode}");
                            return;
                        }

                        // 启动服务
                        if (!_tcpServer.Start())
                        {
                            _countDownReStart = 10;
                            AddLog_Info($"【服务器】服务器启动失败 {_tcpServer.Address}:{_tcpServer.Port}，error code: {_tcpServer.ErrorCode}, error message: {_tcpServer.ErrorMessage} ,{_countDownReStart} 秒后重启");

                            //如果端口被占用
                            if (_tcpServer.ErrorCode == SocketError.SocketBind)
                            {
                                if (_tcpServer.Port - InitPort < 50)
                                {
                                    _tcpServer.Port += 1;
                                }
                            }
                            return;
                        }
                        AddLog_Info($"【服务器】服务器启动成功 {_tcpServer.Address}:{_tcpServer.Port}");
                    }
                }
                else
                {
                    NeedAckMsgManager.Instance.Update();
                }
            }
            catch (Exception ex)
            {
                AddLog_Error(ex);
            }
        }

        public string State
        {
            get
            {
                switch (_tcpServer.State)
                {
                    case ServiceState.Starting:
                        return $"S - STG - P{_tcpServer.Port} ";
                    case ServiceState.Started:
                        ushort port = 0;
                        try
                        {
                            if (_connId != IntPtr.Zero) _tcpServer.GetRemoteAddress(_connId, out var ip, out port);
                        }
                        catch (Exception)
                        {
                        }
                        return $"S - STD - P{_tcpServer.Port} - C - {_connId} - P{port} ";
                    case ServiceState.Stopping:
                        return $"S - SPG - P{_tcpServer.Port} ";
                    case ServiceState.Stopped:
                        return $"S - SPD - P{_tcpServer.Port} ";
                    default:
                        return $"S - None - P{_tcpServer.State} ";
                }
            }
        }

        #endregion

        #region ■------------------ 重发机制

        public List<TCPNeedAckMsg> NeedAckMsgs = new List<TCPNeedAckMsg>();

        #endregion
    }
}
