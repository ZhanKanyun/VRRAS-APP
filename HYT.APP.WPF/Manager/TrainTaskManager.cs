using CL.Common;
using KT.TCP;
using Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Threading;
using System.Windows;
using HYT.APP.WPF;
using HYT.MidiManager;
using System.Linq;
using System.Threading.Tasks;
using KT.TCP.Train;
using HYT.APP.WPF.Device;

namespace KCL
{
    /// <summary>
    /// 训练管理器：单训练版本
    /// </summary>
    public class TrainTaskManager
    {
        #region ■------------------ TCP通信

        /// <summary>
        /// 训练客户端连接处理：发送启动信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TCPServer_OnClientConnect(object sender, IntPtr e)
        {
            try
            {
                if (CurrentTask == null) return;
                if (CurrentTask.State == TrainTaskStates.Loading)//正在加载训练的时候
                {
                    //读取训练启动信息，发送到游戏
                    CurrentTask.StartInfo.UserInfo = JsonConvert.SerializeObject(AppManager.Instance.CurrentPatient);

                    //查询曲库信息和训练记录
                    TrainMusicLibrary trainMusicLibrary = AppManager.Instance.GetTrainMusicLibrary(CurrentTask.StartInfo.id);
                    CurrentTask.StartInfo.MusicLibrary = JsonConvert.SerializeObject(trainMusicLibrary);

                    //发送训练启动信息
                    TCPServer.Instance.SendClient(new TrainMsg(TrainMsgTypes.S2C_TrainStartInfo)
                    {
                        Data = JsonConvert.SerializeObject(CurrentTask.StartInfo)
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        public tb_Music Music_GetByID(string id)
        {
            try
            {
                var music = DBHelper.Instance.GetOneByID<tb_Music>(id);
                return music;

            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return null;
            }
        }
        /// <summary>
        /// 上一次收到的加载进度
        /// </summary>
        private string _lastLoadProgress = "";

        /// <summary>
        /// 客户端消息处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TCPServer_OnMsgProcess(object sender, TrainMsg e)
        {
            try
            {
                if (e.Type != TrainMsgTypes.C2S_LoadProgress && e.Type != TrainMsgTypes.C2S_PlayMIDI)
                {
                    LogHelper.Info("【收到消息】" + e.ToString());
                }
                if (CurrentTask == null) return;

                switch (e.Type)
                {
                    case TrainMsgTypes.C2S_LoadProgress://训练启动进度
                        {
                            //if (CurrentTask.State == TrainTaskStates.Loading)
                            //{
                            //    if (e.Data != _lastLoadProgress)
                            //    {
                            //        _lastLoadProgress = e.Data;
                            //        WindowTrainUI.Dispatcher.Invoke(() =>
                            //        {
                            //            WindowTrainUI.SetPregress(Convert.ToDouble(e.Data));
                            //        });
                            //    }

                            //}
                        }
                        break;
                    case TrainMsgTypes.C2S_LoadDone://训练启动成功
                        {
                            CurrentTask.State = TrainTaskStates.LoadDown;
                            WindowTrainUI.SetPregress(1);
                        }
                        break;
                    case TrainMsgTypes.C2S_LoadError://训练启动失败，可以告知原因
                        {
                            if (CurrentTask.State == TrainTaskStates.Loading)
                            {
                                LogHelper.Info("【加载训练】加载异常，错误信息：" + e.Data);
                                CurrentTask.State = TrainTaskStates.LoadError;
                            }
                        }
                        break;
                    case TrainMsgTypes.C2S_ShowPause://显示暂停
                        {
                            DeviceDataAnalysisManager.Instance.Pause();

                        }
                        break;
                    case TrainMsgTypes.C2S_HidePause://隐藏暂停
                        {
                            DeviceDataAnalysisManager.Instance.Continue();

                        }
                        break;
                    case TrainMsgTypes.C2S_Done://训练完成
                        {
                            //关闭进程
                            if (CurrentTask.State == TrainTaskStates.Run || CurrentTask.State == TrainTaskStates.RoundDone)
                            {
                                //打开评测
                                if (e.D1 == 1)
                                {
                                    BrowserManager.Instance.ExecuteJSAsync($"API_CSharp.opentrainreportdetail('{web_train_report_id}')");
                                }

                                if (CurrentTask != null)
                                {
                                    //关闭训练
                                    CurrentTask.State = TrainTaskStates.Done;
                                    CurrentTask.Kill();
                                }

                               
                            }
                        }
                        break;
                    case TrainMsgTypes.C2S_StartRecord://开始记录数据
                        {
                            DeviceDataAnalysisManager.Instance.Start();
                        }
                        break;
                    case TrainMsgTypes.C2S_RoundDone://回合结束，保存记录
                        {
                            //关闭进程
                            if (CurrentTask.State == TrainTaskStates.Run)
                            {
                                CurrentTask.State = TrainTaskStates.RoundDone;
                                //发送训练简报给训练 = 设备数据记录和分析
                                try
                                {
                                    var record = DeviceDataAnalysisManager.Instance.Stop();

                                    GaitRecordUnity gaitRecordUnity = new GaitRecordUnity(record);

                                    TCPServer.Instance.SendClient(new TrainMsg(TrainMsgTypes.S2C_TrainResult)
                                    {
                                        Data = JsonConvert.SerializeObject(gaitRecordUnity)
                                    });
                                }
                                catch (Exception ex)
                                {
                                    LogHelper.Error(ex);
                                }

                                if (e.D2 == 1)//是否记录
                                {

                                }
                                else
                                {
                                    tb_LogTrain tr_data = new tb_LogTrain();

                                    tr_data.ID = AppManager.Instance.CreateID("TL");
                                    while (DBHelper.Instance.GetCountWhere<tb_LogTrain>("ID=@id", new { id = tr_data.ID }) > 0)
                                    {
                                        tr_data.ID = AppManager.Instance.CreateID("TL");
                                    }

                                    //TODO
                                    tr_data.StartTime = CurrentTask.StartTime;
                                    tr_data.EndTime = DateTime.Now;

                                    MsgData_RoundDone msgData_RoundDone = JsonConvert.DeserializeObject<MsgData_RoundDone>(e.Data);

                                    tr_data.PatientID = AppManager.Instance.CurrentPatient.ID;

                                    tr_data.TrainID = CurrentTask.StartInfo.id;
                                    tr_data.TrainType = Convert.ToInt32(tr_data.TrainID[0].ToString());
                                    tr_data.TrainName = CurrentTask.StartInfo.name;
                                    tr_data.SetInfo = JsonConvert.SerializeObject(CurrentTask.StartInfo);
                                    tr_data.SetDifficulty = CurrentTask.StartInfo.set_Difficulty;
                                    tr_data.SetTime = CurrentTask.StartInfo.set_Time;
                                    tr_data.Data = e.Data;
                                    tr_data.Record = JsonConvert.SerializeObject(DeviceDataAnalysisManager.Instance.CurrentGaitRecord);
                                    tr_data.MusicID = msgData_RoundDone.MusicID;
                                    tr_data.Score = msgData_RoundDone.Score;
                                    tr_data.AccuracyRate = msgData_RoundDone.AccuracyRate;
                                    DBHelper.Instance.Insert<tb_LogTrain>(tr_data);

                                    web_train_report_id = tr_data.ID;

                                    BrowserManager.Instance.ExecuteJSAsync($"API_CSharp.updateTrainRecord()");

                                    #region 〓〓〓〓〓〓〓 更新患者数据  

                                    try
                                    {
                                        if (AppManager.Instance.CurrentPatient != null)
                                        {
                                            //患者最新步态记录 仅训练不包含游戏
                                            if (tr_data.TrainType == 1)
                                            {
                                                Mode_patientNewData patientNewData = new Mode_patientNewData()
                                                {
                                                    symm = DeviceDataAnalysisManager.Instance.CurrentGaitRecord.Symm_Average,
                                                    speed = DeviceDataAnalysisManager.Instance.CurrentGaitRecord.Speed_Average,
                                                    steplenght = DeviceDataAnalysisManager.Instance.CurrentGaitRecord.StepLength_Average,
                                                    rhythm = DeviceDataAnalysisManager.Instance.CurrentGaitRecord.Rhythm_Average
                                                };
                                                AppManager.Instance.CurrentPatient.Data = JsonConvert.SerializeObject(patientNewData);
                                            }

                                            AppManager.Instance.CurrentPatient.Gold += msgData_RoundDone.GetGold;

                                            DBHelper.Instance.Update<tb_Patient>(AppManager.Instance.CurrentPatient);

                                            BrowserManager.Instance.ExecuteJSAsync($"API_CSharp.updatacurrentPatient()");

                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        LogHelper.Error(ex);
                                    }

                                    #endregion

                                    #region 〓〓〓〓〓〓〓 查询排行榜发送给训练：分数计算包含难度系数，排行榜不区分难度

                                    try
                                    {
                                        //查询排行榜发送给训练：分数计算包含难度系数，排行榜不区分难度
                                        var ranks = DBHelper.Instance.GetWhereOrderTop<tb_LogTrain>(o => o.TrainID == CurrentTask.StartInfo.id && o.MusicID == msgData_RoundDone.MusicID, 10, "Score desc,StartTime desc");
                                        //Parallel.ForEach(ranks,item=> {});
                                        List<tb_LogTrainUnity> tb_LogTrainUnitys = new List<tb_LogTrainUnity>();
                                        foreach (var item in ranks)
                                        {
                                            var pa = DBHelper.Instance.GetOneByID<tb_Patient>(item.PatientID);
                                            item.PatientName = pa == null ? item.PatientID : pa.Name;
                                            tb_LogTrainUnitys.Add(new tb_LogTrainUnity()
                                            {
                                                PatientID = item.PatientID,
                                                PatientName = item.PatientName,
                                                Score = item.Score,
                                                AccuracyRate = item.AccuracyRate,
                                                TrainID = item.TrainID,
                                                StartTime = item.StartTime
                                            });
                                        }

                                        var rank = 0;//当前患者排第几名
                                        try
                                        {
                                            var list = DBHelper.Instance.WhereOrderSelect<tb_LogTrain, tb_LogTrainSimple>(o => o.TrainID == CurrentTask.StartInfo.id && o.MusicID == msgData_RoundDone.MusicID, "Score desc,StartTime desc", o => new tb_LogTrainSimple { PatientID = o.PatientID, Score = o.Score });
                                            var my = list.Where(o => o.Score == msgData_RoundDone.Score && o.PatientID == AppManager.Instance.CurrentPatient.ID).ToList()[0];
                                            rank = list.IndexOf(my);
                                        }
                                        catch (Exception ex)
                                        {
                                            LogHelper.Error(ex);
                                        }

                                        TCPServer.Instance.SendClient(new TrainMsg(TrainMsgTypes.S2C_Rank)
                                        {
                                            Data = JsonConvert.SerializeObject(tb_LogTrainUnitys),
                                            D1 = rank + 1,//当前患者排第几名
                                            D2 = msgData_RoundDone.Score
                                        });
                                    }
                                    catch (Exception ex)
                                    {

                                        LogHelper.Error(ex);
                                    }

                                    #endregion


                                }
                            }
                        }
                        break;
                    case TrainMsgTypes.C2S_DeviceControl://设备控制
                        {
                            switch (e.D1)
                            {
                                case 1://启动并设置速度
                                    {
                                        DeviceManager.Instance.SetSpeed(e.D3);
                                    }
                                    break;
                                case 2://关闭
                                    {
                                        DeviceManager.Instance.Stop();
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    case TrainMsgTypes.C2S_PlayMIDI://指定音乐ID（节拍器为0，音乐小节为1）、BPM
                        {
                            if (e.D1 != 4 && e.D1 != 5)
                            {
                                LogHelper.Info("【收到消息】" + e.ToString());
                            }
                            //e.D1     信号：1自动播放  2继续  3设置播放类型为信号控制  4信号（播放左边）  5信号（播放右边）
                            //e.Data   播放类型：音乐ID  节拍器  音乐小节

                            //e.D2     音乐播放节拍
                            //e.D3     节拍器/音乐小节间隔时间 左边
                            //e.D4     节拍器/音乐小节间隔时间 右边

                            string _control = e.Data;
                            switch (e.D1)
                            {
                                case 1://设置播放类型，主程序自动播放
                                    {
                                        if (_control == "节拍器") MidiControl.Instance.PlayMusic(e.D3, e.D4);
                                        else if (_control == "音乐小节") MidiControl.Instance.PlayMusicbeat(e.D3, e.D4);
                                        else MidiControl.Instance.PlayMusic(_control, e.D2);
                                    }
                                    break;
                                case 2://"继续"
                                    {
                                        MidiControl.Instance.ContinueMusic();
                                    }
                                    break;
                                case 3://设置播放类型，等待信号控制
                                    {
                                        if (_control == "节拍器") MidiControl.Instance.PlayType(-1);
                                        else if (_control == "音乐小节") MidiControl.Instance.PlayType(-2, e.D3, e.D4);
                                    }
                                    break;
                                case 4://"播放控制信号" //播放左边
                                    {
                                        MidiControl.Instance.leftPlay();
                                    }
                                    break;
                                case 5://"播放控制信号"  //播放右边
                                    {
                                        MidiControl.Instance.rightPlay();
                                    }
                                    break;
                            }

                            #region 信号控制更改前
                            ////e.Data   音乐ID  节拍器为  "节拍器"  音乐小节为"音乐小节"
                            ////e.D1     播放为1  继续为2 
                            ////e.D2     节拍
                            ////e.D3     节拍器间隔时间 左边间隔时间
                            ////e.D4     节拍器间隔时间 右边间隔时间

                            //var musicid = e.Data;
                            ////AppManager.Instance.Window.Dispatcher.Invoke(() => {
                            //if (e.D1 == 1)
                            //{
                            //    if (musicid == "节拍器")
                            //    {
                            //        MidiControl.Instance.PlayMusic(e.D3, e.D4);
                            //    }
                            //    else if (musicid == "音乐小节")
                            //    {
                            //        MidiControl.Instance.PlayMusicbeat(e.D3, e.D4);
                            //    }
                            //    else
                            //    {
                            //        //if (musicid == "") musicid = "2e5b0b95-419c-467b-85a7-4a23d1b4c376";
                            //        MidiControl.Instance.PlayMusic(musicid, e.D2);
                            //    }
                            //}
                            //else
                            //{
                            //    MidiControl.Instance.ContinueMusic();
                            //}

                            #endregion

                        }
                        break;
                    case TrainMsgTypes.C2S_PauseMIDI://暂停MIDI音乐
                        {
                            MidiControl.Instance.PauseMusic();
                        }
                        break;
                    case TrainMsgTypes.C2S_StopMIDI://停止MIDI音乐
                        {
                            MidiControl.Instance.StopMusic();
                        }
                        break;
                    case TrainMsgTypes.C2S_SetMIDISpeed:// 设置MIDI音乐速度
                        {
                            //e.D2     速度为1  节拍为2  3设置节拍器时间
                            //e.D3     设置 速度/节拍/ 左边间隔时间
                            //e.D4     右边间隔时间
                            if (e.D2 == 1)
                            {
                                MidiControl.Instance.SetMusicSpeed(e.D3);
                            }
                            else if (e.D2 == 2)
                            {
                                MidiControl.Instance.SetMusicBeat(e.D3);
                            }
                            else
                            {
                                MidiControl.Instance.SetBeatTime(e.D3, e.D4);
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
        /// 服务器管理组件日志
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TCPServer_OnLog(object sender, TCPLog e)
        {
            try
            {
                switch (e.LogType)
                {
                    case TCPLogTypes.Info:
                        LogHelper.Info(e.Log);
                        break;
                    case TCPLogTypes.Error:
                        LogHelper.Error(e.Log);
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

        private void TCPServer_OnMsgSend(object sender, TrainMsg e)
        {
            try
            {
                if (e.Type == TrainMsgTypes.S2C_Controll||e.Type == TrainMsgTypes.S2C_DeviceData) return; //太频繁
                LogHelper.Info("【发送消息】" + e.ToString());
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }

        }

        #endregion

        #region ■------------------ 字段相关

        /// <summary>
        /// 当前任务
        /// </summary>
        public TrainTask CurrentTask { get; private set; }

        /// <summary>
        /// 主程序窗口 打开网页的窗口
        /// </summary>
        public Window WindowMain;
        /// <summary>
        /// 训练窗口，开始训练任务时打开，完成训练任务后关闭
        /// </summary>
        public WinTrain WindowTrain;
        /// <summary>
        /// 训练UI窗口 盖在训练UI上面
        /// </summary>
        public WinTrainUI WindowTrainUI;

        /// <summary>
        /// 是否超时检测
        /// </summary>
        public bool IsLoadTimeoutCheck { get; set; }

        public bool IsRun
        {
            get { return CurrentTask != null && CurrentTask.State != TrainTaskStates.Done && CurrentTask.State != TrainTaskStates.RoundDone; }
        }


        /// <summary>
        /// 保存的训练记录ID-便于传给web查看训练详情
        /// </summary>
        public string web_train_report_id;

        #endregion

        #region ■------------------ 单例

        private TrainTaskManager()
        {
            //StartTimer();
            AppManager.Instance.OnTick5 += OnTick5;
            TCPServer.Instance.OnLog += TCPServer_OnLog;
            TCPServer.Instance.OnMsgProcess += TCPServer_OnMsgProcess;
            TCPServer.Instance.OnClientConnect += TCPServer_OnClientConnect;
            TCPServer.Instance.OnMsgSend += TCPServer_OnMsgSend;
            //TCPServer.Instance.IsLogEvent = true;
            //TCPServer.Instance.IsLogData_Send = true;
            TCPServer.Instance.Start("127.0.0.1", Convert.ToUInt16(ConfigManager.Instance.SSetting.TCPPort), 20480);//PublicMethod.GetLocalIp()
        }

        public readonly static TrainTaskManager Instance = new TrainTaskManager();

        #endregion

        #region ■------------------ 定时器：后台线程刷新  如果有问题，可以切换为 计时器

        private void OnTick5(object sender, int e)
        {
            var nowTick = DateTime.Now.Ticks;
            var passTick = nowTick - _lastTick;
            _lastTick = nowTick;

            if (!TCPServer.Instance.IsStarted) return;//服务器未启动

            if (CurrentTask != null)//没有正在训练的任务，进入下一个训练
            {
                switch (CurrentTask.State)
                {
                    case TrainTaskStates.Wait:
                        {
                            //准备加载
                            CurrentTask.State = TrainTaskStates.LoadPrepare;
                            LogHelper.Info($"启动训练 {CurrentTask.StartInfo.exePath}");
                            CurrentTask.OnExit += CurrentTask_OnExit;
                            CurrentTask.OpenUnity();
                            _lastTick = DateTime.Now.Ticks;

                        }
                        break;
                    case TrainTaskStates.Loading://训练加载中
                        {
                            if (IsLoadTimeoutCheck)
                            {
                                CurrentTask.LoadTime += passTick;
                                if (CurrentTask.LoadTime > TimeSpan.FromMinutes(2).Ticks)//两分钟未收到加载完成信息
                                {
                                    CurrentTask.State = TrainTaskStates.LoadTimeout;
                                }
                            }
                        }
                        break;
                    case TrainTaskStates.LoadDown:
                        {
                            if (MidiControl.Instance.IsLoadDone)
                            {
                                WindowTrainUI.Dispatcher.Invoke(() =>
                                {

                                    if (CurrentTask.State == TrainTaskStates.LoadDown)
                                    {
                                        WindowTrainUI.SetPregress(1d);
                                        WindowTrainUI.HideLoad();

                                        CurrentTask.State = TrainTaskStates.Run;
                                        CurrentTask.StartTime = DateTime.Now;

                                        TCPServer.Instance.SendClient(new TrainMsg(TrainMsgTypes.S2C_StartGame));

                                        TCPServer.Instance.SendClient(new TrainMsg(TrainMsgTypes.S2C_DeviceState)
                                        {
                                            D2 = SPDevice_ZBJ.Instance.IsCommunication ? 1 : 0,
                                        });

                                    }
                                });
                            }
                        }
                        break;
                    case TrainTaskStates.Run:
                        {
                            WindowTrainUI.UpdateDeviceState(DeviceManager.Instance.IsCommunication);
                            if (DeviceManager.Instance.IsCommunication)
                            {
                                CurrentTask.Active();
                            }
                        }
                        break;
                    case TrainTaskStates.Done://训练完成
                        {
                            Stop();
                        }
                        break;
                    case TrainTaskStates.Cancel://训练取消
                        {
                            Stop();
                        }
                        break;
                    case TrainTaskStates.LoadError://训练加载错误 自动下一个
                        {
                            Stop();
                        }
                        break;
                    case TrainTaskStates.LoadTimeout://训练加载错误 自动下一个
                        {
                            Stop();
                        }
                        break;
                    default:
                        break;
                }
            }

            if (WindowTrainUI != null)
            {
                WindowTrainUI.Dispatcher.Invoke(() =>
                {
                    WindowTrainUI.UpdateServerState();
                });
            }
        }

        /// <summary>
        /// 训练任务计时器
        /// </summary>
        public DispatcherTimer _timer;

        private long _lastTick;

        #endregion

        #region ■------------------ 外部接口

        /// <summary>
        /// 是否已经初始化
        /// </summary>
        public bool IsInit { get; private set; }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="win"></param>
        public void Init(MainWindow win)
        {
            try
            {
                if (!IsInit)
                {
                    WindowMain = win;

                    //创建训练窗口 不再销毁 只隐藏
                    WindowTrain = new WinTrain();
                    WindowTrain.Owner = WindowMain;
                    WindowTrain.Left = 0;
                    WindowTrain.Top = 0;
                    WindowTrain.WindowState = WindowState.Maximized;
                    WindowTrain.WindowStyle = WindowStyle.None;
                    WindowTrain.ResizeMode = ResizeMode.NoResize;
                    WindowTrain.ShowInTaskbar = false;
                    WindowTrain.Hide();
                    IsInit = true;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                LogHelper.Info("训练任务管理器初始化失败，无法开启训练");
            }
        }

        public bool Start(TrainStartInfo item)
        {
            try
            {
                ProcessHelper.KillTrainProcess();

                item.ParentProcessID = Process.GetCurrentProcess().Id;
                if (!System.IO.Path.IsPathRooted(item.exePath))
                {
                    item.exePath = AppDomain.CurrentDomain.BaseDirectory + item.exePath;//Train\CPTrain110001.exe 转化为绝对路径
                }
                TrainTask task = new TrainTask();
                task.Index = item.Index;
                task.StartInfo = item;
                CurrentTask = task;
                LogHelper.Info($"TrainTaskManager.Start，添加训练= {item.ToString()}");

                _lastTick = DateTime.Now.Ticks;
                WindowTrain.Dispatcher.Invoke(() =>
                {
                    WindowTrain.Show();
                });
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return false;
            }
        }

        /// <summary>
        /// 取消任务、关闭Unity进程、隐藏WindowTrain、停止MIDI 
        /// </summary>
        public void Stop()
        {
            try
            {
                if (CurrentTask != null)
                {
                    CurrentTask.Kill();
                    CurrentTask = null;//TrainTaskManager.Stop
                }

                ProcessHelper.KillTrainProcess();
                WindowTrain.Dispatcher.Invoke(() =>
                {
                    if (WindowTrain != null)
                    {
                        WindowTrain.Hide();
                    }
                });
                MidiControl.Instance.StopMusic();//停止音乐

            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        public void ActiveCurrentTask()
        {
            try
            {
                if (CurrentTask != null && CurrentTask.State == TrainTaskStates.Run)
                {
                    CurrentTask.Active(true);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        #endregion

        #region ■------------------ 训练事件通知

        /// <summary>
        /// 训练任务退出通知
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CurrentTask_OnExit(object sender, TrainEventArgs e)
        {
            try
            {
                if (WindowTrainUI != null)
                {
                    WindowTrainUI.Dispatcher.Invoke(() =>
                    {
                        WindowTrainUI.Close();
                    });
                }
                MidiControl.Instance.StopMusic();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        #endregion

    }

    /// <summary>
    /// 训练任务管理器状态
    /// </summary>
    public enum TrainTaskManagerStates
    {
        /// <summary>
        /// 休息状态 可以进行添加任务
        /// </summary>
        Rest,
        /// <summary>
        /// 全部完成，等待训练任务
        /// </summary>
        Done,
        /// <summary>
        /// 运行状态 正在训练或者有训练任务在等待
        /// </summary>
        Run,
    }

    /// <summary>
    /// 训练任务
    /// </summary>
    public class TrainTask
    {
        /// <summary>
        /// 训练任务顺序编号
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 加载计时
        /// </summary>
        public long LoadTime { get; set; }

        private TrainTaskStates _state = TrainTaskStates.Wait;

        /// <summary>
        /// 训练任务状态
        /// </summary>
        public TrainTaskStates State
        {
            get { return _state; }
            set
            {
                if (_state != value)
                {
                    _state = value;
                    switch (_state)
                    {
                        case TrainTaskStates.LoadTimeout://加载超时
                            Kill(2);
                            OnLoadResult?.Invoke(this, new TrainEventArgs() { Code = 2 });
                            break;
                        case TrainTaskStates.LoadError://加载错误
                            Kill(2);
                            OnLoadResult?.Invoke(this, null);
                            break;
                        case TrainTaskStates.Done://完成
                            Kill(0);
                            break;
                        case TrainTaskStates.Cancel://取消训练
                            Kill(1);
                            break;
                        default:
                            break;
                    }
                }
            }
        }


        /// <summary>
        /// 训练启动信息
        /// </summary>
        public TrainStartInfo StartInfo { get; set; }

        /// <summary>
        /// 启动时间
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 训练数据
        /// </summary>
        public MsgData_RoundDone TrainData { get; set; } = new MsgData_RoundDone();

        #region ■------------------ 启动

        /// <summary>
        /// 训练进程
        /// </summary>
        public Process TrainProcess;

        /// <summary>
        /// 启动进程
        /// </summary>
        /// <param name="startInfo"></param>
        public void OpenUnity()
        {
            //杀死残留进程
            ProcessHelper.KillTrainProcess();

            if (State == TrainTaskStates.LoadPrepare)
            {
                State = TrainTaskStates.Loading;
                //打开训练进程
                IntPtr thisHandle = IntPtr.Zero;
                TrainTaskManager.Instance.WindowTrain.Dispatcher.Invoke(() =>
                {
                    thisHandle = (new WindowInteropHelper(TrainTaskManager.Instance.WindowTrain)).Handle;
                });

                try
                {
                    TrainProcess = new Process();
                    TrainProcess.StartInfo.FileName = StartInfo.exePath;
                    TrainProcess.StartInfo.Arguments = "-parentHWND " + ((int)thisHandle).ToString() + " UnityMain " + TCPServer.Instance.CurrentPort;
                    TrainProcess.StartInfo.UseShellExecute = false;
                    TrainProcess.StartInfo.CreateNoWindow = true;
                    TrainProcess.EnableRaisingEvents = true;
                    TrainProcess.Exited += TrainProcess_Exited;

                    if (TrainProcess.Start())//训练进程打开成功
                    {
                        Console.WriteLine($"打开训练进程成功 {StartInfo.exePath}");
                        LogHelper.Info("【加载训练】启动训练进程，进入加载状态，超时检测");
                        //CurrentTask.State = TrainStates.Loading;
                        LoadTime = 0;

                        //添加训练UI
                        LogHelper.Info("【加载训练】显示UI窗口");
                        TrainTaskManager.Instance.WindowTrain.Dispatcher.Invoke(() =>
                        {
                            TrainTaskManager.Instance.WindowTrainUI = new WinTrainUI();
                            TrainTaskManager.Instance.WindowTrainUI.Owner = TrainTaskManager.Instance.WindowTrain;

                            TrainTaskManager.Instance.WindowTrainUI.LoadLoadPage(this.StartInfo.id);
                            TrainTaskManager.Instance.WindowTrainUI.Show();
                        });

                    }
                    else//训练进程启动失败
                    {
                        Debug.WriteLine($"打开训练进程失败 {StartInfo.exePath}");
                        State = TrainTaskStates.LoadError;
                    }
                }
                catch (Exception ex)
                {
                    State = TrainTaskStates.LoadError;
                    LogHelper.Error(ex);
                }
            }
        }

        /// <summary>
        /// 训练进程退出监听
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TrainProcess_Exited(object sender, EventArgs e)
        {
            try
            {
                OnExit?.Invoke(this, null);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        /// <summary>
        /// 进程退出代码 0-训练完成由任务管理器正常关闭 1-训练过程中手动点击退出 2-训练过程中进程异常退出
        /// </summary>
        public int ExitCode { get; set; } = -1;

        /// <summary>
        /// 杀掉游戏进程
        /// </summary>
        /// <param name="isManual"></param>
        public void Kill(int exitCode = 0)
        {
            try
            {
                if (ExitCode == -1)
                {
                    ExitCode = exitCode;
                    if (TrainProcess != null)
                    {
                        TrainProcess.Kill();
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        #endregion

        #region ■------------------ 事件通知

        /// <summary>
        /// 训练加载结果 0-加载成功 1-加载异常 2-加载超时 3-进程打开失败
        /// </summary>
        public event EventHandler<TrainEventArgs> OnLoadResult;

        /// <summary>
        /// 训练退出结果 0-训练完成 1-点击退出 2-训练异常退出
        /// </summary>
        public event EventHandler<TrainEventArgs> OnExit;

        #endregion


        [DllImport("user32.dll", EntryPoint = "FindWindowEx")]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);
        [DllImport("user32.dll")]
        static extern int SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
        private const int WM_ACTIVATE = 0x0006;
        private readonly IntPtr WA_ACTIVE = new IntPtr(1);

        private int _activeInterval = 20;
        /// <summary>
        /// 激活Unity进程，避免掉帧
        /// </summary>
        /// <param name="isForce"></param>
        public void Active(bool isForce = false)
        {
            try
            {
                if (!isForce)
                {
                    if (_activeInterval > 0)
                    {
                        _activeInterval--;
                        return;
                    }
                }
                _activeInterval = 20;

                if (TrainProcess != null)
                {
                    TrainTaskManager.Instance.WindowTrain.Dispatcher.Invoke(new Action(() =>
                    {
                        var thisHandle = new WindowInteropHelper(TrainTaskManager.Instance.WindowTrain).Handle;
                        var handle = FindWindowEx(thisHandle, IntPtr.Zero, null, TrainProcess.ProcessName);

                        SendMessage(handle, WM_ACTIVATE, WA_ACTIVE, IntPtr.Zero);
                    }));

                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }
    }

    public class TrainEventArgs
    {
        /// <summary>
        /// 代码
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        public string Msg { get; set; }
    }

    /// <summary>
    /// 训练任务状态
    /// </summary>
    public enum TrainTaskStates
    {
        /// <summary>
        /// 等待前面的训练任务完成
        /// </summary>
        Wait,
        /// <summary>
        /// 准备加载
        /// </summary>
        LoadPrepare,
        /// <summary>
        /// 加载中
        /// </summary>
        Loading,
        /// <summary>
        /// 加载超时
        /// </summary>
        LoadTimeout,
        /// <summary>
        /// 加载错误
        /// </summary>
        LoadError,
        /// <summary>
        /// 训练进行中
        /// </summary>
        Run,
        /// <summary>
        /// 回合结束
        /// </summary>
        RoundDone,
        /// <summary>
        /// 训练完成
        /// </summary>
        Done,
        /// <summary>
        /// 主动退出 或者 进程异常退出 ，训练未完成
        /// </summary>
        Cancel,
        /// <summary>
        /// 训练帮助视频播放
        /// </summary>
        HelpVideo,

        /// <summary>
        /// 训练加载完成,等待音乐加载完成
        /// </summary>
        LoadDown,
    }


    class Mode_patientNewData
    {
        public float symm { get; set; }
        public float speed { get; set; }
        public float steplenght { get; set; }
        public float rhythm { get; set; }

    };
}
