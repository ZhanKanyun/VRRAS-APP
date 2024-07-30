using CL.Common;
using HYT.MidiManager;
using KCL;
using Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Windows;

namespace HYT.APP.WPF
{
    public partial class MainWindow : Window
    {
        #region ■------------------ 构造加载

        public MainWindow()
        {
            InitializeComponent();
            try
            {
                //程序设置
                Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;

                //启动主计时器
                AppManager.Instance.StartTick1();
                AppManager.Instance.StartTick2();
                AppManager.Instance.StartTick3();
                AppManager.Instance.StartTick4();
                AppManager.Instance.StartTick5();

                AppManager.Instance.OnTick5 += OnTick5;
                //加载配置
                ConfigManager.Instance.Load();
                LogHelper.Info("Load Config Success");

                //检测数据库
                if (DBHelper.Instance.Init(SqlSugar.DbType.Sqlite, "DataSource=" + AppDomain.CurrentDomain.BaseDirectory + @"Content\DK.db"))
                {
                    LogHelper.Info("DB CONN Success");
                    //DBHelper.Instance.CreateClassFile();
                    LogHelper.Info($"Browser Init Start APPPath={ConfigManager.Instance.SSetting.APPPath} AppTestPath={ConfigManager.Instance.DSetting.AppTestPath}");
                    //渲染Web页面
                    BrowserManager.Instance.InitBrowser(wv2, ConfigManager.Instance.SSetting.APPPath, ConfigManager.Instance.DSetting.AppTestPath, () =>
                    {
                        DeviceManager.Instance.Start();
                        LogHelper.Info("Browser Init Success");
                        AppManager.Instance.AddLogOperate("软件启动", "", DateTime.Now.ToString());

                        #region 〓〓〓〓〓〓〓 MIDI初始化和加载

                        MidiControl.Instance.Init();

                        var db_musics = DBHelper.Instance.GetWhere<tb_Music>(o => o.IsValid);
                        List<MidiMusic> musics = new List<MidiMusic>();
                        foreach (var musicItem in db_musics)
                        {
                            if (musicItem != null)
                            {
                                musics.Add(new MidiMusic()
                                {
                                    ID = musicItem.ID,
                                    Name = musicItem.Name,
                                    MusicBeat = musicItem.BPM,
                                    //MusicPath = musicItem.FilePath,
                                    FileData = musicItem.FileData
                                });
                            }
                        }
                        MidiControl.Instance.AddMusics(musics);
                        LogHelper.Info("MIDI Init Success");

                        #endregion

                    });
                }
                else
                {
                    MessageBox.Show("数据库连接失败，应用程序退出");
                    Application.Current.Shutdown();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                LogHelper.Info("应用程序初始化失败，程序自动退出！");
                Application.Current.Shutdown();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                AppManager.Instance.Win_Debug = new WinDebug();
                AppManager.Instance.Win_MIDI = new WinMIDI();
                AppManager.Instance.Win_MIDI.Hide();

                Left = 0;
                Top = 0;
                TrainTaskManager.Instance.Init(this);
                AppManager.Instance.Window = this;

            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        #endregion

        #region ■------------------ 打开调试窗口 F10-MIDI调试   F11-设备调试   F12-Web调试

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                if (ConfigManager.Instance.SSetting != null && ConfigManager.Instance.SSetting.IsDebug)
                {
                    if (e.Key == System.Windows.Input.Key.F11)
                    {
                        AppManager.Instance.Win_Debug.Topmost = true;
                        AppManager.Instance.Win_Debug.WindowState = WindowState.Normal;
                        AppManager.Instance.Win_Debug.Show();
                    }
                    if (e.Key == System.Windows.Input.Key.F10)
                    {
                        BrowserManager.Instance.ExecuteJSAsync($"API_CSharp.updatacurrentPatient()");
                        AppManager.Instance.Win_MIDI.Topmost = true;
                        AppManager.Instance.Win_MIDI.WindowState = WindowState.Normal;
                        AppManager.Instance.Win_MIDI.Show();
                    }
                }
                else
                {
                    //BrowserManager.Instance.Browser.CoreWebView2.Settings.
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        #endregion


        private void OnTick5(object sender, int e)
        {
            try
            {
                if (DeviceManager.Instance.IsJJTZ)
                {
                    panelJJTZ.Visibility = Visibility.Visible;
                    wv2.Visibility = Visibility.Collapsed;

                    LogHelper.Info($"【紧急停止】");
                    //停止训练
                    if (TrainTaskManager.Instance.IsRun)
                    {
                        TrainTaskManager.Instance.Stop();
                    }
                    //停止评估
                    BrowserManager.Instance.ExecuteJSAsync("API_CSharp.stopAssess()");
                }
                else
                {
                    panelJJTZ.Visibility = Visibility.Collapsed;
                    wv2.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {

                LogHelper.Error(ex);
            }
          
        }

        /// <summary>
        /// 页面缩放 高度自适应
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {
                var bili = gridWrap.ActualWidth / gridWrap.ActualHeight;
                var newHeight = 1920d / bili;
                //if (newHeight < 1080) newHeight = 1080;
                gridMain.Height = newHeight;

                BrowserManager.Instance.ExecuteJSAsync($"API_CSharp.setHeight("+ newHeight + ")");
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

    }
}
