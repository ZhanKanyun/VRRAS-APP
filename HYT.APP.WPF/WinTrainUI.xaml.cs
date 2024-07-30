using CL.Common;
using HYT.APP.WPF.Device;
using KCL;
using KT.TCP;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace HYT.APP.WPF
{
    /// <summary>
    /// WinTrainUI.xaml 的交互逻辑
    /// </summary>
    public partial class WinTrainUI : Window
    {
        #region ■------------------ 构造加载

        public WinTrainUI()
        {
            InitializeComponent();

            try
            {
                AppManager.Instance.OnTick3 += OnTick3;

                
               
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
                //if (ConfigManager.Instance.SSetting.IsDebug)
                //{
                    gridMain.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                //}
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        #endregion

        #region ■------------------ 加载面板

        private void OnTick3(object sender, int e)
        {
            try
            {
                //TODO   模拟进度条
                if (_lastProgress<1)
                {
                    _lastProgress += 0.008f;
                }
                if (_lastProgress>1)
                {
                    _lastProgress = 1;
                }
                SetPregress(_lastProgress);

            }
            catch (Exception ex)
            {

                LogHelper.Error(ex);
            }

        }

        /// <summary>
        /// 打开进程先加载加载页面所需的资源，快速显示
        /// </summary>
        /// <param name="resource"></param>
        public void LoadLoadPage(string trainID)
        {
            try
            {
                loadText.Text = "训练加载中...0%";
                var assemblyName = System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name.Replace(".exe", "").Replace(".dll","");//单文件编译模式获取不到
                assemblyName = "SWBT.APP";
                LogHelper.Info("训练加载："+assemblyName);
                //_loadRenImages.Add(new BitmapImage(new Uri(@"pack://application:,,,/" + assemblyName + ";Component/Content/Resource/Image/加载动画/跑步_000" + i.ToString().PadLeft(2, '0') + ".png", UriKind.Absolute)));
                gridLoad.Visibility = Visibility.Visible;
                gridLoad.Background = new ImageBrush(new BitmapImage(new Uri(@"pack://application:,,,/" + assemblyName + ";Component/Content/Resource/Image/"+ trainID + "BG.png")));
                imgProgress.Source = new BitmapImage(new Uri(@"pack://application:,,,/" + assemblyName + ";Component/Content/Resource/Image/" + trainID + "Progress.png"));
                imgProgress.Width = 0;
                _lastProgress = 0;
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        public void HideLoad()
        {
            try
            {
                gridLoad.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        double _lastProgress;
        public void SetPregress(double progress)
        {
            var progressPercent = Math.Floor(progress * 100);
            loadText.Dispatcher.Invoke(() =>{
                loadText.Text = "训练加载中..." + progressPercent.ToString("f0") + "%";
                imgProgress.Width = 1700d * progress;
            });
        }

        #endregion

        #region ■------------------ 暂停功能

        #endregion

        #region ■------------------ UI


        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }

        }

       


    
        #endregion

        #region ■------------------ 刷新

        public void UpdateServerState()
        {
            try
            {
                if (ConfigManager.Instance.SSetting.IsDebug)
                {
                    lblServerState.Text = TCPServer.Instance.State;
                }
                else
                {
                    lblServerState.Text = "";
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        /// <summary>
        /// 设备断开提示
        /// </summary>
        /// <param name="isShow"></param>
        public void UpdateDeviceState(bool isShow)
        {
            gridDeviceDisconnect.Visibility = isShow ? Visibility.Collapsed : Visibility.Visible;
        }

        #endregion

        private void btnExitTrain_click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (TrainTaskManager.Instance.CurrentTask == null) return;
                TrainTaskManager.Instance.CurrentTask.State = TrainTaskStates.Cancel;
            }
            catch (Exception ex)
            {

                LogHelper.Error(ex);
            }
        }


    }
}
