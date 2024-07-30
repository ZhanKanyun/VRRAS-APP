using CL.Common;
using HYT.APP.WPF;
using HYT.APP.WPF.Device;
using HYT.MidiManager;
using KT.TCP;
using Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace KCL
{
    public class AppManager
    {
        #region ■------------------ 单例

        private AppManager() { }
        public readonly static AppManager Instance = new AppManager();

        #endregion

        public MainWindow Window;
        public WinDebug Win_Debug;
        /// <summary>
        /// MIDI测试窗口
        /// </summary>
        public WinMIDI Win_MIDI = null;

        /// <summary>
        /// 当前用户
        /// </summary>
        public tb_Patient CurrentPatient;

        /// <summary>
        /// 当前网页正在试听的音乐
        /// </summary>
        public tb_Music CurrentWebTestMusic;

        #region ■------------------ 主计时器1：Task 间隔10毫秒

        /// <summary>
        /// 程序主计时器Tick事件 参数为间隔毫秒数
        /// </summary>
        public EventHandler<int> OnTick;

        CancellationTokenSource _tokenSource = new CancellationTokenSource();
        Task _timerTask;

        /// <summary>
        /// 主计时器最后一次Tick 时间
        /// </summary>
        public DateTime _lastTick = DateTime.MinValue;

        /// <summary>
        /// 主计时器开始运行
        /// </summary>
        public void StartTick1()
        {
            _lastTick = DateTime.Now;
            //_timerMain.Interval = TimeSpan.FromMilliseconds(16);

            _timerTask = Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    var now = DateTime.Now;
                    var pass = (int)((now - _lastTick).TotalMilliseconds);
                    _lastTick = now;
                    OnTick?.Invoke(this, pass);
                    if (_tokenSource.IsCancellationRequested)
                    {
                        return;
                    }
                    Thread.Sleep(10);
                }
            }, _tokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);


            //_timerMain.Elapsed += (sender, e) => {
            //    var now = DateTime.Now;
            //    var pass = (int)((now - _lastTick).TotalMilliseconds);
            //    _lastTick = now;
            //    OnTick?.Invoke(_timerMain, pass);
            //};
            //_timerMain.Start();
        }

        public void StopTick1()
        {
            _tokenSource.Cancel();
        }

        #endregion

        #region ■------------------ 主计时器2：Task 间隔10毫秒  稳住时间  设备模拟器在用

        /// <summary>
        /// 程序主计时器Tick事件 参数为间隔毫秒数
        /// </summary>
        public EventHandler<int> OnTick2;

        CancellationTokenSource _tokenSource2 = new CancellationTokenSource();
        Task _timerTask2;

        /// <summary>
        /// 主计时器最后一次Tick 时间
        /// </summary>
        public DateTime _lastTick2 = DateTime.MinValue;

        public Queue<double> _passRecord = new Queue<double>();

        /// <summary>
        /// 主计时器开始运行
        /// </summary>
        public void StartTick2()
        {
            _lastTick2 = DateTime.Now;

            _timerTask2 = Task.Factory.StartNew(() =>
            {
                Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;
                while (true)
                {
                    var now = DateTime.Now;
                    var pass = ((now - _lastTick2).TotalMilliseconds);
                    //Debug.WriteLine($"Tick2 pass = { pass}  Thread（id ={ Thread.CurrentThread.ManagedThreadId} { Thread.CurrentThread.IsBackground}  { Thread.CurrentThread.IsThreadPoolThread}）");
                    _lastTick2 = now;

                    Task.Run(() =>
                    {
                        OnTick2?.Invoke(this, (int)pass);
                    });

                    if (_tokenSource2.IsCancellationRequested)
                    {
                        Thread.CurrentThread.Priority = ThreadPriority.Normal;
                        return;
                    }

                    //稳住时间
                    if (isJumpPass)
                    {
                        isJumpPass = false;
                        if (pass >= 9)
                        {
                            _passRecord.Enqueue(pass);
                        }
                    }
                    else
                    {
                        _passRecord.Enqueue(pass);
                    }

                    if (_passRecord.Count >= 2)
                    {
                        int sleep = (int)(Math.Floor(30d - _passRecord.Sum()));
                        if (sleep <= 0) sleep = 1;

                        Thread.Sleep(sleep);

                        _passRecord.Clear();
                        isJumpPass = true;
                    }
                    else
                    {
                        Thread.Sleep(8);
                    }

                }
            }, _tokenSource2.Token);//TaskCreationOptions.LongRunning,TaskScheduler.Default

        }

        private bool isJumpPass = false;

        public void StopTick2()
        {
            _tokenSource2.Cancel();
        }

        #endregion

        #region ■------------------ 主计时器3：Task 间隔25毫秒  WinTrainUI 加载动画在用

        /// <summary>
        /// 程序主计时器Tick事件 参数为间隔毫秒数
        /// </summary>
        public EventHandler<int> OnTick3;

        CancellationTokenSource _tokenSource3 = new CancellationTokenSource();
        Task _timerTask3;

        /// <summary>
        /// 主计时器最后一次Tick 时间
        /// </summary>
        public DateTime _lastTick3 = DateTime.MinValue;

        /// <summary>
        /// 主计时器开始运行
        /// </summary>
        public void StartTick3()
        {
            _lastTick3 = DateTime.Now;

            _timerTask3 = Task.Factory.StartNew(() =>
            {
                Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;
                while (true)
                {
                    var now = DateTime.Now;
                    var pass = ((now - _lastTick3).TotalMilliseconds);
                    _lastTick3 = now;

                    Task.Run(() =>
                    {
                        OnTick3?.Invoke(this, (int)pass);
                    });

                    if (_tokenSource3.IsCancellationRequested)
                    {
                        Thread.CurrentThread.Priority = ThreadPriority.Normal;
                        return;
                    }

                    Thread.Sleep(25);
                }
            }, _tokenSource3.Token);//TaskCreationOptions.LongRunning,TaskScheduler.Default

        }

        public void StopTick3()
        {
            _tokenSource3.Cancel();
        }

        #endregion

        #region ■------------------ 主计时器4：Task 间隔10毫秒  稳住时间  MIDI播放器用

        /// <summary>
        /// 程序主计时器Tick事件 参数为间隔毫秒数
        /// </summary>
        public EventHandler<int> OnTick4;

        CancellationTokenSource _tokenSource4 = new CancellationTokenSource();
        Task _timerTask4;

        /// <summary>
        /// 主计时器最后一次Tick 时间
        /// </summary>
        public DateTime _lastTick4 = DateTime.MinValue;

        public Queue<double> _passRecord4 = new Queue<double>();

        /// <summary>
        /// 主计时器开始运行
        /// </summary>
        public void StartTick4()
        {
            _lastTick4 = DateTime.Now;

            _timerTask4 = Task.Factory.StartNew(() =>
            {
                Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;
                while (true)
                {
                    var now = DateTime.Now;
                    var pass = ((now - _lastTick4).TotalMilliseconds);
                    _lastTick4 = now;

                    MidiControl.Instance.PlayerUpdate();

                    Task.Run(() =>
                    {
                        OnTick4?.Invoke(this, (int)pass);
                    });

                    if (_tokenSource4.IsCancellationRequested)
                    {
                        Thread.CurrentThread.Priority = ThreadPriority.Normal;
                        return;
                    }

                    //稳住时间
                    if (isJumpPass4)
                    {
                        isJumpPass4 = false;
                        if (pass >= 9)
                        {
                            _passRecord4.Enqueue(pass);
                        }
                    }
                    else
                    {
                        _passRecord4.Enqueue(pass);
                    }

                    if (_passRecord4.Count >= 2)
                    {
                        int sleep = (int)(Math.Floor(30d - _passRecord4.Sum()));
                        if (sleep <= 0) sleep = 1;

                        Thread.Sleep(sleep);

                        _passRecord4.Clear();
                        isJumpPass4 = true;
                    }
                    else
                    {
                        Thread.Sleep(8);
                    }

                }
            }, _tokenSource4.Token);//TaskCreationOptions.LongRunning,TaskScheduler.Default

        }

        private bool isJumpPass4 = false;

        public void StopTick4()
        {
            _tokenSource4.Cancel();
        }

        #endregion

        #region ■------------------ 主计时器5：DispatcherTimer 间隔16毫秒  UI计时器 不跨线程  间隔很不稳定  训练任务管理器再用

        /// <summary>
        /// 程序主计时器Tick事件 参数为间隔毫秒数
        /// </summary>
        public EventHandler<int> OnTick5;

        DispatcherTimer _timerTask5 = new DispatcherTimer();

        public DateTime _timerTask5_last;

        /// <summary>
        /// 主计时器开始运行
        /// </summary>
        public void StartTick5()
        {
            _timerTask5.Interval = TimeSpan.FromMilliseconds(16);
            _timerTask5.Tick += (sender, o) =>
            {
                var now = DateTime.Now;
                var pass = (_timerTask5_last - now).TotalMilliseconds;
                OnTick5?.Invoke(this, 16);
                MidiControl.Instance.LoadUpdate((int)pass);
                //Debug.WriteLine($"【计时器】{DateTime.Now.ToString("HH:mm:ss.fff")}  - 间隔 {(_timerTask5_last - now).TotalMilliseconds}");
                _timerTask5_last = now;
            };
            _timerTask5.Start();
        }

        public void StopTick5()
        {
            _timerTask5.Stop();
        }

        #endregion

        /// <summary>
        /// 获取训练的曲库及训练记录
        /// </summary>
        /// <param name="train"></param>
        /// <returns></returns>
        public TrainMusicLibrary GetTrainMusicLibrary(string trainid)
        {
            try
            {
                TrainMusicLibrary trainMusicLibrary = new TrainMusicLibrary();
                trainMusicLibrary.TrainID = trainid;
                List<tb_TrainMusic> db_trainMusics = new List<tb_TrainMusic>();
                switch (CurrentPatient.DiseaseType)
                {
                    case "脑卒中":
                        {
                            db_trainMusics.Add(new tb_TrainMusic() { MusicID = "节拍器", MusicName = "节拍器" });
                            db_trainMusics.Add(new tb_TrainMusic() { MusicID = "音乐小节", MusicName = "音乐小节" });
                        }
                        break;
                    case "帕金森":
                        {
                            db_trainMusics = DBHelper.Instance.GetWhere<tb_TrainMusic>(o => o.TrainID == trainid);
                        }
                        break;
                    default:
                        break;
                }
               

                foreach (var item in db_trainMusics)
                {
                    tb_Music db_music = DBHelper.Instance.GetOneByID<tb_Music>(item.MusicID);
                    if (db_music == null && item.MusicID != "节拍器" && item.MusicID != "音乐小节")
                    {
                        continue;
                    } 
                    MusicLibrary_Music music = new MusicLibrary_Music();
                    music.MusicID = item.MusicID;
                    music.MusicName = db_music == null ? item.MusicName : db_music.Name;
                    music.BPM = db_music == null ? 0 : db_music.BPM;

                    //指定用户指定训练指定歌曲的训练记录
                    List<tb_LogTrain> logs = DBHelper.Instance.GetWhere<tb_LogTrain>(o => o.PatientID == CurrentPatient.ID && o.TrainID == trainid && o.MusicID == item.MusicID);
                    //根据难度分组，取出每个难度的最高记录
                    var groups = logs.GroupBy(o => o.SetDifficulty);
                    foreach (var group in groups)
                    {
                        MusicLibrary_Record record = new MusicLibrary_Record();
                        record.Difficulty = group.Key;

                        var maxScore = group.MaxBy(o => o.Score);
                        if (maxScore != null && maxScore.Score > 0)
                        {
                            record.Score = (int)maxScore.Score;
                            record.ZQL = (int)maxScore.AccuracyRate;
                        }
                        music.Records.Add(record);
                    }
                    //music.ZQL = 0;
                    //music.Score = 0;

                    //tb_LogTrain log = DBHelper.Instance.GetWhereFirst<tb_LogTrain>(o => o.TrainID == trainid && o.SetDifficulty == difficulty.ID && o.MusicID == item.MusicID); ;
                    //if (log != null)
                    //{
                    //    music.ZQL = log.AccuracyRate;
                    //    music.Score = log.Score == null ? 0 : (int)log.Score;
                    //}

                    trainMusicLibrary.Musics.Add(music);
                }

                return trainMusicLibrary;
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return null;
            }
        }

        public string CreateID(string key)
        {
            return key + DateTime.Now.ToString("yyyMMddHHmmssffffff");
        }

        /// <summary>
        /// 输出消息到调试控制台 
        /// </summary>
        /// <param name="msg"></param>
        public void Output(string msg, int type = 4)
        {
            try
            {
                if (Win_Debug != null)
                {
                    Win_Debug.ShowLog(msg, type);
                }
            }
            catch (Exception ex)
            {

                LogHelper.Error(ex);
            }
        }

        #region ■------------------ 操作日志


        /// <summary>
        /// 添加操作日志
        /// </summary>
        public void AddLogOperate(string type, string dataid, string old)
        {
            try
            {
                tb_LogOperate log = new tb_LogOperate();
                log.Time = DateTime.Now;
                log.Type = type;
                log.DataID = dataid;
                log.Old = old;
                log.ID = CreateID("OL");
                DBHelper.Instance.Insert<tb_LogOperate>(log);
            }
            catch (Exception ex)
            {

                LogHelper.Error(ex);
            }
        }


        #endregion

        public void Exit()
        {
            try
            {
                LogHelper.Info("〓〓〓〓〓〓 App Exit 开始停止线程、释放资源");
                try
                {
                    MidiControl.Instance.Dispose();
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                }
                try
                {
                    App_JSAPI.Instance.KeyBoard_Hide();
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                }
                try
                {
                    //关闭设备通信
                    SPCheckManage.Instance.Stop();
                    SPDevice_ZBJ.Instance.Stop();

                    //关闭计时器
                    AppManager.Instance.StopTick1();
                    AppManager.Instance.StopTick2();
                    AppManager.Instance.StopTick3();
                    AppManager.Instance.StopTick4();
                    AppManager.Instance.StopTick5();

                    TrainTaskManager.Instance.Stop();
                    
                    MoniWalkManager.Instance.Stop();

                    TCPServer.Instance.Stop();

                    //关闭串口
                    if (AppManager.Instance.Win_Debug != null)
                    {
                        if (AppManager.Instance.Win_Debug.MONIPort != null && AppManager.Instance.Win_Debug.MONIPort.IsOpen)
                        {
                            AppManager.Instance.Win_Debug.MONIPort.Close();
                        }
                    }

                    //关闭试听音乐
                    if (AppManager.Instance.CurrentWebTestMusic != null)
                    {
                        MidiControl.Instance.StopMusic();
                    }
                    AppManager.Instance.AddLogOperate("软件退出", "", DateTime.Now.ToString());

                    LogHelper.Info("〓〓〓〓〓〓 App Exit 结束");
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }
    }
}
