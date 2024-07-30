using HYT.MidiManager.Properties;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Resources;
using System.Windows;
using System.Xml.Linq;

namespace HYT.MidiManager
{
    /// <summary>
    /// MIDI控制器
    /// </summary>
    public class MidiControl
    {
        #region ■------------------ 单例
        private MidiControl()
        {

        }

        public readonly static MidiControl Instance = new MidiControl();

        #endregion

        #region ■------------------ 字段

        /// <summary>
        /// 是否全部音乐加载完成
        /// </summary>
        public bool IsLoadDone
        {
            get
            {
                lock (_lockerListMusicInfo)
                {
                    return MidiMusicList.Count(o => o.LoadState != MidiMusicLoadStates.LoadDone) == 0;
                }
            }
        }

        public MidiControlStates MidiControlState = MidiControlStates.None;

        /// <summary>
        /// 当前音乐
        /// </summary>
        private MidiMusic CurrentMidiMusic;


        /// <summary>
        /// 音乐小节
        /// </summary>
        private MidiMusic YYXJMidiMusic_L;
        /// <summary>
        /// 音乐小节
        /// </summary>
        private MidiMusic YYXJMidiMusic_R;
        /// <summary>
        /// 音乐小节-左
        /// </summary>
        private List<MidiEvent> midiEventLeft;
        /// <summary>
        /// 音乐小节-右
        /// </summary>
        private List<MidiEvent> midiEventRight;
        /// <summary>
        /// 当前音乐小节
        /// </summary>
        private List<MidiEvent> _currentMidiEvent;

        /// <summary>
        /// 音乐列表
        /// </summary>
        private List<MidiMusic> MidiMusicList = new List<MidiMusic>();



        private readonly object _lockerListMusicInfo = new object();

        /// <summary>
        /// 设备
        /// </summary>
        private OutputDevice outDevice;
        private int outDeviceID = 0;

        /// <summary>
        /// 当前时间节点
        /// </summary>
        double _currentTick = 0;
        long _lastTick = 0;
        /// <summary>
        /// 是否循环播放
        /// </summary>
        bool isLoop = false;
        /// <summary>
        /// 已完成音轨
        /// </summary>
        int endCount = 0;

        /// <summary>
        /// 播放类型 0音乐 1节拍器 2音乐小节  -1信号控制节拍器 -2信号控制音乐小节
        /// </summary>
        int isPlayType = 0;

        /// <summary>
        /// 当前节拍器-左边
        /// </summary>
        bool this_metronome_left = true;
        /// <summary>
        /// 更改速度间隔-l
        /// </summary>
        float ChangeInterval_l = 1;
        /// <summary>
        /// 更改速度间隔-r
        /// </summary>
        float ChangeInterval_r = 2f;
        /// <summary>
        /// 节拍器/音乐小节更改计时
        /// </summary>
        double _metronomeTime = 0;

        /// <summary>
        /// 音乐小节初始时长
        /// </summary>
        float originTime = 2f;


        /// <summary>
        /// /// 上一个播放信号 时间节点
        /// </summary>
        double _lastPlayTick_ = 0;

        #endregion


        #region ■------------------ 初始化

        /// <summary>
        /// 需要正确释放
        /// </summary>
        public void Dispose()
        {
            if (outDevice != null && !outDevice.IsDisposed)
            {
                outDevice.Close();
                outDevice.Dispose();
            }
        }

        /// <summary>
        /// 初始化设备和播放计时器
        /// </summary>
        public void Init()
        {
            MidiControlState = MidiControlStates.Init;

            //设备
            if (OutputDevice.DeviceCount == 0)
            {
                //MessageBox.Show("No MIDI output devices available.", "Error!");
                throw new Exception("MIDI设备初始化失败");
            }
            else
            {
                if (outDevice == null)
                {
                    //outDevice.Close();
                    outDevice = new OutputDevice(outDeviceID);
                }
            }

            //StartTimer();
            OnLog($"【MIDI控制器】初始化完成...");
        }

        #endregion

        #region ■------------------ 加载

        /// <summary>
        /// 添加音乐，自动检测加载
        /// </summary>
        /// <param name="music"></param>
        public void AddMusic(MidiMusic music)
        {
            lock (_lockerListMusicInfo)
            {
                if (MidiMusicList.Count(o => o.Name == music.Name && o.ID == music.ID) <= 0)
                {
                    OnLog($"【MIDI控制器】1.添加音乐 - 线程{Thread.CurrentThread.ManagedThreadId} 音乐={music}  ");
                    MidiMusicList.Add(music);
                }
            }
        }

        /// <summary>
        /// 添加音乐，自动检测加载
        /// </summary>
        /// <param name="musics"></param>
        public void AddMusics(List<MidiMusic> musics)
        {
            lock (_lockerListMusicInfo)
            {
                foreach (var music in musics)
                {
                    if (MidiMusicList.Count(o => o.Name == music.Name) <= 0)
                    {
                        OnLog($"【MIDI控制器】1.添加音乐 - 线程{Thread.CurrentThread.ManagedThreadId} 音乐={music}");
                        MidiMusicList.Add(music);
                    }
                }
            }
        }

        /// <summary>
        /// 音乐加载检测
        /// </summary>
        public void LoadUpdate(int pass = 0)
        {
            lock (_lockerListMusicInfo)
            {
                string noDones = "";
                foreach (var music in MidiMusicList)
                {
                    if (music.LoadState == MidiMusicLoadStates.None || music.LoadState == MidiMusicLoadStates.LoadError)
                    {
                        noDones += music.Name + "|";
                        music.LoadState = MidiMusicLoadStates.Loading;
                        OnLog($"【MIDI控制器】2.为音乐创建加载线程 - 线程 {Thread.CurrentThread.ManagedThreadId} 音乐={music} ");
                        music.LoadTask = Task.Run(() =>
                        {
                            ThreadLoadMusic(music);
                        });
                        break;//线程安全问题，不能一次性启动
                    }
                    else if (music.LoadState == MidiMusicLoadStates.Loading)
                    {
                        noDones += music.Name + "|";
                        music.LoadTime += pass;
                        if (music.LoadTime > 120 * 1000)//2分钟
                        {
                            music.LoadState = MidiMusicLoadStates.LoadError;
                            OnLog($"【MIDI控制器】5.加载超时 - 线程 {Thread.CurrentThread.ManagedThreadId} 音乐={music} ");
                        }
                    }
                }

                if (noDones != "")
                {
                    OnLog($"【MIDI控制器】加载中 = {noDones}");
                }

                if (YYXJMidiMusic_L == null || YYXJMidiMusic_L.LoadState == MidiMusicLoadStates.None || YYXJMidiMusic_L.LoadState == MidiMusicLoadStates.LoadError)
                {
                    YYXJMidiMusic_L = new MidiMusic()
                    {
                        ID = "音乐小节左",
                        Name = "水边的阿迪丽娜",
                        MusicBeat = 60,
                        LoadState = MidiMusicLoadStates.Loading
                    };

                    YYXJMidiMusic_L.LoadTask = Task.Run(() =>
                    {
                        LoadMusic(YYXJMidiMusic_L);
                    });
                }
                if (YYXJMidiMusic_R == null || YYXJMidiMusic_R.LoadState == MidiMusicLoadStates.None || YYXJMidiMusic_R.LoadState == MidiMusicLoadStates.LoadError)
                {
                    YYXJMidiMusic_R = new MidiMusic()
                    {
                        ID = "音乐小节右",
                        Name = "秋日私语",
                        MusicBeat = 60,
                        LoadState = MidiMusicLoadStates.Loading
                    };

                    YYXJMidiMusic_R.LoadTask = Task.Run(() =>
                    {
                        LoadMusic(YYXJMidiMusic_R);
                    });
                }
            }
        }

        /// <summary>
        /// 加载音乐
        /// </summary>
        /// <param name="music"></param>
        private void ThreadLoadMusic(MidiMusic music)
        {
            try
            {
                if (music.LoadState == MidiMusicLoadStates.Loading)
                {
                    OnLog($"【MIDI控制器】3.加载线程运行 - 线程{Thread.CurrentThread.ManagedThreadId} 线程池={Thread.CurrentThread.IsThreadPoolThread}  音乐={music} ");

                    if (music.FileData != null && music.FileData.Length > 0)//文件流加载
                    {
                        music.MidiFileInfo.Music = music;
                        music.MidiFileInfo.Load(music.FileData);

                        music.MidiEventList.Clear();
                        //获取所有音轨的Midievent
                        for (int trackIndex = 0; trackIndex < music.MidiFileInfo.Tracks.Count; trackIndex++)
                        {
                            Track track = music.MidiFileInfo.Tracks[trackIndex];

                            for (int i = 0; i < track.Count; i++)
                            {
                                music.MidiEventList.Add(track.GetMidiEvent(i));
                            }
                        }
                    }
                    else
                    {
                        if (File.Exists(music.MusicPath))//文件加载
                        {
                            music.MidiFileInfo.Load(music.MusicPath);
                            music.MidiEventList.Clear();
                            //获取所有音轨的Midievent
                            for (int trackIndex = 0; trackIndex < music.MidiFileInfo.Tracks.Count; trackIndex++)
                            {
                                Track track = music.MidiFileInfo.Tracks[trackIndex];

                                for (int i = 0; i < track.Count; i++)
                                {
                                    music.MidiEventList.Add(track.GetMidiEvent(i));
                                }
                            }
                        }
                    }
                    music.LoadState = MidiMusicLoadStates.LoadDone;
                    OnLog($"【MIDI控制器】5.加载完成 - 线程{Thread.CurrentThread.ManagedThreadId}  音乐={music}  ");
                }
            }
            catch (Exception ex)
            {
                OnLog(ex.Message + "," + ex.StackTrace);
                music.Clear();
                music.LoadState = MidiMusicLoadStates.LoadError;
                //LogHelper.Error(this, ex);
            }
        }

        /// <summary>
        /// 加载音乐小节
        /// </summary>
        /// <param name="music"></param>
        private void LoadMusic(MidiMusic music)
        {
            try
            {
                if (music.LoadState == MidiMusicLoadStates.Loading)
                {
                    Assembly _assembly = Assembly.GetExecutingAssembly();
                    string _namespace = MethodBase.GetCurrentMethod().DeclaringType.Namespace;

                    string resourceName = _namespace + ".Resources." + music.Name + ".mid";
                    Stream stream = _assembly.GetManifestResourceStream(resourceName);

                    if (stream != null && stream.Length > 0)//文件流加载
                    {
                        music.MidiFileInfo.Music = music;
                        music.MidiFileInfo.Load(stream);

                        music.MidiEventList.Clear();
                        //获取所有音轨的Midievent
                        for (int trackIndex = 0; trackIndex < music.MidiFileInfo.Tracks.Count; trackIndex++)
                        {
                            Track track = music.MidiFileInfo.Tracks[trackIndex];

                            for (int i = 0; i < track.Count; i++)
                            {
                                music.MidiEventList.Add(track.GetMidiEvent(i));
                            }
                        }
                        music.LoadState = MidiMusicLoadStates.LoadDone;
                    }
                    else
                    {
                        music.LoadState = MidiMusicLoadStates.LoadError;
                    }
                }
            }
            catch (Exception ex)
            {
                OnLog(ex.Message + "," + ex.StackTrace);
                music.Clear();
                music.LoadState = MidiMusicLoadStates.LoadError;
            }
        }

        public MidiMusic GetMidiMusicByID(string id)
        {
            lock (_lockerListMusicInfo)
            {
                if (MidiMusicList.Count(o => o.ID == id) > 0)
                {
                    return MidiMusicList.First(o => o.ID == id);
                }
            }
            return null;
        }

        public bool MusicIsLoad(string id)
        {
            lock (_lockerListMusicInfo)
            {
                if (MidiMusicList.Count(o => o.ID == id) > 0)
                {
                    var a = MidiMusicList.First(o => o.ID == id);
                    return a.LoadState == MidiMusicLoadStates.LoadDone;
                }
            }
            return false;
        }

        #endregion

        #region---信号控制节拍器、音乐小节播放

        /// <summary>
        /// 设置播放类型为信号控制
        /// </summary>
        /// <param name="metronome">播放类型 -1信号控制节拍器 -2信号控制音乐小节</param>
        public void PlayType(int metronome, float beatTime_l = 0f, float beatTime_r = 0f)
        {
            // if (MidiControlState == MidiControlStates.Playing) return;
            try
            {
                if (outDevice != null)
                {
                    outDevice.Close();
                }
                outDevice = new OutputDevice(outDeviceID);

                isPlayType = metronome;

                if (isPlayType == -2)
                {
                    ChangeInterval_l = beatTime_l;
                    ChangeInterval_r = beatTime_r;

                    CurrentMidiMusic = YYXJMidiMusic_L;
                    if (CurrentMidiMusic == null) CurrentMidiMusic = MidiMusicList[0];
                    midiEventLeft = new List<MidiEvent>(YYXJMidiMusic_L.MidiEventList.Skip(0).Take(26));
                    midiEventRight = new List<MidiEvent>(YYXJMidiMusic_R.MidiEventList.Skip(0).Take(26));
                    foreach (var item in midiEventLeft)
                    {
                        item.IsDone = false;
                    }

                    foreach (var item in midiEventRight)
                    {
                        item.IsDone = false;
                    }
                    SetMusicSpeed(originTime / ChangeInterval_l);
                    _lastTick = DateTime.Now.Ticks;
                    _currentTick = 0;
                }

                MidiControlState = MidiControlStates.Playing;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        /// <summary>
        /// 信号通知控制播放 左边 音乐小节/节拍器
        /// </summary>
        public void leftPlay()
        {
            _lastPlayTick_ = _currentTick;

            //正常音乐
            if (isPlayType != -1 && isPlayType != -2) return;

            //节拍器
            if (isPlayType == -1)
            {
                PlayLeft();
                return;
            }
            //音乐小节
            foreach (var item in midiEventLeft)
            {
                item.IsDone = false;
            }
            _currentMidiEvent = midiEventLeft;

            SetMusicSpeed(originTime / ChangeInterval_l);
            _currentTick = midiEventLeft[0].AbsoluteTicks;
        }

        /// <summary>
        /// 信号通知控制播放 左边 音乐小节/节拍器
        /// </summary>
        public void rightPlay()
        {
            _lastPlayTick_ = _currentTick;
            //正常音乐
            if (isPlayType != -1 && isPlayType != -2) return;
            //节拍器
            if (isPlayType == -1)
            {
                PlayRight();
                return;
            }
            //音乐小节
            foreach (var item in midiEventRight)
            {
                item.IsDone = false;
            }
            _currentMidiEvent = midiEventRight;

            SetMusicSpeed(originTime / ChangeInterval_r);
            _currentTick = midiEventRight[0].AbsoluteTicks;
        }

        #endregion

        #region ■------------------ 播放

        /// <summary>
        /// 音乐播放计时器
        /// </summary>
        public void PlayerUpdate()
        {
            long nowTick = DateTime.Now.Ticks;
            long passTick = nowTick - _lastTick;

            //OnLog($"【MIDI】线程ID={Thread.CurrentThread.ManagedThreadId}  线程池={Thread.CurrentThread.IsThreadPoolThread} _timerMain_Tick  经过 = {passTick/10000d} 毫秒");

            _lastTick = nowTick;

            try
            {
                switch (MidiControlState)
                {
                    case MidiControlStates.None:
                        break;
                    case MidiControlStates.Init://初始化             
                        break;
                    case MidiControlStates.Playing://播放中
                        {
                            double passSeconds = (double)passTick / 1000d / 1000d / 10d;
                            _metronomeTime += passSeconds;
                            //节拍器
                            if (isPlayType == 1)
                            {
                                if (this_metronome_left)
                                {
                                    if (_metronomeTime >= ChangeInterval_l)
                                    {
                                        _metronomeTime = 0;
                                        this_metronome_left = false;

                                        PlayRight();
                                    }
                                }
                                else
                                {
                                    if (_metronomeTime >= ChangeInterval_r)
                                    {
                                        _metronomeTime = 0;
                                        this_metronome_left = true;

                                        PlayLeft();
                                    }
                                }

                                return;
                            }

                            if (isPlayType == -1) return;

                            //经过的微秒数 1秒=10000000Ticks
                            double passus = (double)passTick / 10d;
                            //经过的MIDI Tick数
                            double pass_midiTick = passus / CurrentMidiMusic.MidiFileInfo.PerTickMicrosecond;
                            _currentTick += pass_midiTick;

                            //信号控制-音乐小节
                            if (isPlayType == -2)
                            {
                                foreach (var even in _currentMidiEvent)
                                {
                                    if (even.IsDone) continue;

                                    if (even.AbsoluteTicks <= _currentTick)
                                    {
                                        OnLog($"【MIDI播放】{even}");
                                        switch (even.MidiMessage.MessageType)
                                        {
                                            case MessageType.Channel:
                                                outDevice.Send((ChannelMessage)even.MidiMessage);
                                                break;
                                            case MessageType.SystemExclusive:
                                                outDevice.Send((SysExMessage)even.MidiMessage);
                                                break;
                                            case MessageType.SystemCommon:
                                                outDevice.Send((SysCommonMessage)even.MidiMessage);
                                                break;
                                            case MessageType.SystemRealtime:
                                                outDevice.Send((SysRealtimeMessage)even.MidiMessage);
                                                break;
                                            case MessageType.Meta:
                                                break;
                                            default:
                                                break;
                                        }
                                        even.IsDone = true;
                                    }
                                }

                                return;
                            }

                            //音乐小节
                            if (isPlayType == 2)
                            {
                                foreach (var even in _currentMidiEvent)
                                {
                                    if (even.IsDone) continue;

                                    if (even.AbsoluteTicks <= _currentTick)
                                    {
                                        OnLog($"【MIDI播放】{even}");
                                        switch (even.MidiMessage.MessageType)
                                        {
                                            case MessageType.Channel:
                                                outDevice.Send((ChannelMessage)even.MidiMessage);
                                                break;
                                            case MessageType.SystemExclusive:
                                                outDevice.Send((SysExMessage)even.MidiMessage);
                                                break;
                                            case MessageType.SystemCommon:
                                                outDevice.Send((SysCommonMessage)even.MidiMessage);
                                                break;
                                            case MessageType.SystemRealtime:
                                                outDevice.Send((SysRealtimeMessage)even.MidiMessage);
                                                break;
                                            case MessageType.Meta:
                                                break;
                                            default:
                                                break;
                                        }
                                        even.IsDone = true;
                                    }
                                }

                                if (this_metronome_left)
                                {
                                    if (_metronomeTime > ChangeInterval_l)
                                    {
                                        foreach (var item in midiEventRight)
                                        {
                                            item.IsDone = false;
                                        }

                                        SetMusicSpeed(originTime / ChangeInterval_r);
                                        _currentTick = midiEventRight[0].AbsoluteTicks;
                                        _currentMidiEvent = midiEventRight;

                                        _metronomeTime = 0;
                                        this_metronome_left = false;
                                    }
                                }
                                else
                                {
                                    if (_metronomeTime >= ChangeInterval_r)
                                    {
                                        foreach (var item in midiEventLeft)
                                        {
                                            item.IsDone = false;
                                        }

                                        SetMusicSpeed(originTime / ChangeInterval_l);
                                        _currentTick = midiEventLeft[0].AbsoluteTicks;
                                        _currentMidiEvent = midiEventLeft;

                                        _metronomeTime = 0;
                                        this_metronome_left = true;
                                    }
                                }

                                return;
                            }

                            //正常音乐
                            foreach (var even in CurrentMidiMusic.MidiEventList)
                            {
                                if (even.IsDone) continue;
                                if (_metronomeTime >= 2)
                                {
                                    var aa = 0;
                                }

                                if (even.AbsoluteTicks <= _currentTick)
                                {
                                    OnLog($"【MIDI播放】{even}");
                                    switch (even.MidiMessage.MessageType)
                                    {
                                        case MessageType.Channel:
                                            outDevice.Send((ChannelMessage)even.MidiMessage);
                                            break;
                                        case MessageType.SystemExclusive:
                                            outDevice.Send((SysExMessage)even.MidiMessage);
                                            break;
                                        case MessageType.SystemCommon:
                                            outDevice.Send((SysCommonMessage)even.MidiMessage);
                                            break;
                                        case MessageType.SystemRealtime:
                                            outDevice.Send((SysRealtimeMessage)even.MidiMessage);
                                            break;
                                        case MessageType.Meta:
                                            break;
                                        default:
                                            break;
                                    }
                                    even.IsDone = true;

                                    endCount++;
                                }
                            }
                            //歌曲完成
                            if (endCount >= CurrentMidiMusic.MidiEventList.Count)
                            {
                                if (isLoop)
                                {
                                    OnLog($"【MIDI播放】循环播放 {CurrentMidiMusic.Name}  Division={CurrentMidiMusic.MidiFileInfo.Division}   音轨数={CurrentMidiMusic.MidiFileInfo.Tracks.Count}  消息数={CurrentMidiMusic.MidiEventList.Count}");
                                    endCount = 0;
                                    _lastTick = DateTime.Now.Ticks;
                                    _currentTick = 0;

                                    foreach (var item in CurrentMidiMusic.MidiEventList)
                                    {
                                        item.IsDone = false;
                                    }
                                    //LoopMusic();
                                }
                                else
                                {
                                    StopMusic();
                                }
                            }
                        }
                        break;
                    case MidiControlStates.Pause:
                        break;
                    case MidiControlStates.Finish://播放结束
                        {
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                OnLog(ex.Message + "," + ex.StackTrace);
                //LogHelper.Error(this, ex);
            }
        }

        /// <summary>
        /// 播放音乐
        /// </summary>
        /// <param name="musicID">音乐ID</param>
        /// <param name="beat">播放节拍</param>
        /// <param name="loop">循环播放 默认=true</param>
        public bool PlayMusic(string musicID, float beat, bool loop = true)
        {

            //if (MidiLoadState != MidiLoadStates.Loaded) return;
            //if (MidiControlState == MidiControlStates.Playing) return;

            //try
            //{

            if (outDevice != null)
            {
                // outDevice.Reset();
                outDevice.Close();
            }
            outDevice = new OutputDevice(outDeviceID);

            isPlayType = 0;

            if (MidiMusicList.Count(o => o.ID == musicID) <= 0)
            {
                return false;
            }

            CurrentMidiMusic = MidiMusicList.Find(o => o.ID == musicID);

            if (CurrentMidiMusic.LoadState != MidiMusicLoadStates.LoadDone) return false; ;

            if (CurrentMidiMusic == null)
            {
                CurrentMidiMusic = MidiMusicList[0];
                if (CurrentMidiMusic.LoadState != MidiMusicLoadStates.LoadDone) return false; ;
            }
            OnLog($"【MIDI播放】开始播放 {CurrentMidiMusic.Name}  Division={CurrentMidiMusic.MidiFileInfo.Division}   音轨数={CurrentMidiMusic.MidiFileInfo.Tracks.Count}  消息数={CurrentMidiMusic.MidiEventList.Count}");
            SetMusicBeat(beat);

            foreach (var item in CurrentMidiMusic.MidiEventList)
            {
                item.IsDone = false;
            }

            _lastTick = DateTime.Now.Ticks;
            _currentTick = 0;
            endCount = 0;
            isLoop = loop;

            MidiControlState = MidiControlStates.Playing;
            //StartTimer();
            return true;
            //}
            //catch (Exception ex)
            //{
            //    LogHelper.Error(this, ex);
            //    //Debug.WriteLine(ex);
            //    return false;

            //}
        }

        /// <summary>
        /// 节拍器播放
        /// </summary>
        /// <param name="beatTime_l">左边间隔时间</param>
        /// <param name="beatTime_r">右边间隔时间</param>
        public void PlayMusic(float beatTime_l = 1f, float beatTime_r = 1f)
        {
            // if (MidiControlState == MidiControlStates.Playing) return;
            //try
            //{
            if (outDevice != null)
            {
                // outDevice.Reset();
                outDevice.Close();
            }
            outDevice = new OutputDevice(outDeviceID);

            isPlayType = 1;

            _lastTick = DateTime.Now.Ticks;
            _currentTick = 0;

            ChangeInterval_l = beatTime_l;
            ChangeInterval_r = beatTime_r;

            //立刻播放左边一声
            _metronomeTime = 0;
            this_metronome_left = true;
            PlayLeft();

            MidiControlState = MidiControlStates.Playing;
            //StartTimer();

            //}
            //catch (Exception ex)
            //{
            //    Debug.WriteLine(ex);
            //    //LogHelper.Error(this, ex);
            //}
        }

        /// <summary>
        /// 音乐小节播放
        /// </summary>
        /// <param name="beatTime_l">左边间隔时间</param>
        /// <param name="beatTime_r">右边间隔时间</param>
        public void PlayMusicbeat(float beatTime_l = 1f, float beatTime_r = 1f)
        {
            // if (MidiControlState == MidiControlStates.Playing) return;
            try
            {
                if (outDevice != null)
                {
                    // outDevice.Reset();
                    outDevice.Close();
                }
                outDevice = new OutputDevice(outDeviceID);

                isPlayType = 2;

                CurrentMidiMusic = YYXJMidiMusic_L;
                if (CurrentMidiMusic == null) CurrentMidiMusic = MidiMusicList[0];
                if (YYXJMidiMusic_L == null) YYXJMidiMusic_L = MidiMusicList[0];
                if (YYXJMidiMusic_R == null) YYXJMidiMusic_R = MidiMusicList[0];

                midiEventLeft = new List<MidiEvent>(YYXJMidiMusic_L.MidiEventList.Skip(0).Take(26));
                midiEventRight = new List<MidiEvent>(YYXJMidiMusic_R.MidiEventList.Skip(0).Take(26));
                //从左边开始
                _metronomeTime = 0;
                this_metronome_left = true;
                _currentMidiEvent = midiEventLeft;

                _lastTick = DateTime.Now.Ticks;
                _currentTick = 0;

                ChangeInterval_l = beatTime_l;
                ChangeInterval_r = beatTime_r;

                SetMusicSpeed(originTime / ChangeInterval_l);

                foreach (var item in midiEventLeft)
                {
                    item.IsDone = false;
                }

                foreach (var item in midiEventRight)
                {
                    item.IsDone = false;
                }


                MidiControlState = MidiControlStates.Playing;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                //LogHelper.Error(this, ex);
            }
        }

        /// <summary>
        /// 停止播放/停止计时器
        /// </summary>
        public void StopMusic()
        {
            try
            {
                MidiControlState = MidiControlStates.Finish;
                //StopTimer();

                if (outDevice != null)
                {
                    // outDevice.Reset();
                    outDevice.Close();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                //LogHelper.Error(this, ex);
            }
        }

        /// <summary>
        /// 暂停
        /// </summary>
        public void PauseMusic()
        {
            if (MidiControlState != MidiControlStates.Playing) return;
            MidiControlState = MidiControlStates.Pause;
        }

        /// <summary>
        /// 继续
        /// </summary>
        public void ContinueMusic()
        {
            if (MidiControlState != MidiControlStates.Pause) return;
            MidiControlState = MidiControlStates.Playing;

            _currentTick = _lastPlayTick_;
            MusicRollback(_currentTick);
        }

        /// <summary>
        /// 节拍器播放声音
        /// </summary>
        /// <param name="key">音高(音调)</param>
        /// <param name="volume">音量</param>
        /// <param name="chenel">通道</param>
        /// <returns></returns>
        private void ShortPlay(uint key, uint volume, uint chenel)
        {
            uint val = 144 + key * 256 + volume * 65536 + chenel;

            outDevice.PlaySend((int)val);
        }

        /// <summary>
        /// 左边节拍播放
        /// </summary>
        public void PlayRight()
        {
            ShortPlay(Convert.ToUInt32(70), Convert.ToUInt32(100), Convert.ToUInt32(0));
        }
        /// <summary>
        /// 右边节拍播放
        /// </summary>
        public void PlayLeft()
        {
            ShortPlay(Convert.ToUInt32(60), Convert.ToUInt32(100), Convert.ToUInt32(0));
        }
        #endregion

        #region 设置播放速度  音乐回滚
        /// <summary>
        /// 设置播放速度
        /// </summary>
        /// <param name="speed"></param>
        public void SetMusicSpeed(float speed)
        {
            try
            {
                speed = (float)(Math.Round(speed * 100f) / 100f);

                double lastT = SpeedManager.Instance.Speed / speed;
                _currentTick = Math.Ceiling(_currentTick * lastT);
                SpeedManager.Instance.Speed = speed;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                //LogHelper.Error(this, ex);
            }
        }

        /// <summary>
        /// 设置播放节拍（根据节拍调整速度）
        /// </summary>
        /// <param name="beat">节拍</param>
        public void SetMusicBeat(float beat)
        {
            try
            {
                if (CurrentMidiMusic == null) return;

                float newspeed = (float)(Math.Round(beat / CurrentMidiMusic.MusicBeat * 100f) / 100f);

                SetMusicSpeed(newspeed);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                //LogHelper.Error(this, ex);
            }
        }

        /// <summary>
        /// 设置节拍器/音乐小节播放间隔时间
        /// </summary>
        /// <param name="beatTime_l"></param>
        /// <param name="beatTime_r"></param>
        public void SetBeatTime(float beatTime_l, float beatTime_r)
        {
            try
            {
                ChangeInterval_l = beatTime_l;
                ChangeInterval_r = beatTime_r;

                //if (isPlayType == 1)
                //{
                //    //立刻播放左边一声
                //    _metronomeTime = 0;
                //    this_metronome_left = true;
                //    PlayLeft();
                //}
                //else
                //{
                //    //从左边开始播放
                //    _currentMidiEvent = midiEventLeft;
                //    foreach (var item in _currentMidiEvent)
                //    {
                //        item.IsDone = false;
                //    }

                //    _currentTick = midiEventLeft[0].AbsoluteTicks;
                //    _metronomeTime = 0;
                //    this_metronome_left = true;
                //}
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                //LogHelper.Error(this, ex);
            }
        }

        /// <summary>
        /// 音乐回滚
        /// </summary>
        /// <param name="time">回滚时间点</param>
        void MusicRollback(double time)
        {

            if (CurrentMidiMusic != null)
            {
                foreach (var even in CurrentMidiMusic.MidiEventList)
                {
                    if (even.AbsoluteTicks >= time)
                    {
                        even.IsDone = false;
                    }
                }
            }
        }
        #endregion


        //#region ■------------------ 计时器：音乐播放

        //public System.Timers.Timer _timerMain = new System.Timers.Timer(20);

        ///// <summary>
        ///// 启动计时器
        ///// </summary>
        //private void StartTimer()
        //{
        //    if (!_timerMain.Enabled)
        //    {
        //        _timerMain.Elapsed += _timerMain_Tick;
        //        _timerMain.Start();
        //    }
        //}

        ///// <summary>
        ///// 停止计时器
        ///// </summary>
        //private void StopTimer()
        //{
        //    if (_timerMain != null)
        //    {
        //        _timerMain.Stop();
        //    }
        //}

        ///// <summary>
        ///// 客户端计时器
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void _timerMain_Tick(object sender, EventArgs e)
        //{
        //    //LoadUpdate();

        //}



        //#endregion

        #region ■------------------ 事件

        public event EventHandler<string> Log;

        public void OnLog(string msg)
        {

            Log?.Invoke(null, msg);
        }

        #endregion
    }

    public class MidiMusic
    {
        public void Clear()
        {
            ID = "";
            Name = "";
            MusicPath = "";
            MusicBeat = 0;
            FileData = null;
            MidiFileInfo = new MidiFile();
            MidiEventList.Clear();
            LoadTime = 0;
        }
        public string ID { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// 路径
        /// </summary>
        public string MusicPath { get; set; }
        /// <summary>
        /// 节拍
        /// </summary>
        public float MusicBeat { get; set; }
        public byte[] FileData { get; set; }

        /// <summary>
        /// 音乐文件
        /// </summary>
        public MidiFile MidiFileInfo { get; set; } = new MidiFile();
        /// <summary>
        /// 音轨事件
        /// </summary>
        public List<MidiEvent> MidiEventList { get; set; } = new List<MidiEvent>();

        /// <summary>
        /// 加载状态
        /// </summary>
        public MidiMusicLoadStates LoadState { get; set; } = MidiMusicLoadStates.None;

        /// <summary>
        /// 加载计时 毫秒
        /// </summary>
        public int LoadTime { get; set; }

        /// <summary>
        /// 加载线程
        /// </summary>
        public Task LoadTask;


        public override string ToString()
        {
            return $"ID={ID} Name={Name} 加载状态={LoadState}";
        }
    }

    /// <summary>
    /// MIDI音乐加载状态
    /// </summary>
    public enum MidiMusicLoadStates
    {
        None = 0,
        Loading,
        LoadDone,
        LoadError
    }


    public enum MidiControlStates
    {
        None = 0,
        Init,
        Playing,
        Pause,
        Finish
    }
}

