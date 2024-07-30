using CL.Common;
using CoreAudioApi;
using KCL;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HYT.APP.WPF.UIControls
{
    /// <summary>
    /// VolumeControl.xaml 的交互逻辑
    /// </summary>
    /// <summary>
    /// Volume.xaml 的交互逻辑
    /// </summary>
    public partial class VolumeControl : UserControl
    {
        public VolumeControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                slider.Value = VolumeController.Instance.Volumn;
                sliderValue.Text = slider.Value.ToString("f0");
                slider.ValueChanged -= Slider_ValueChanged;
                slider.ValueChanged += Slider_ValueChanged;
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }



        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                var value = (int)e.NewValue;
                if (value < 0) value = 0;
                if (value > 100) value = 100;
                VolumeController.Instance.SetVolumn(value);
                sliderValue.Text = value.ToString("f0");
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        private void GridMain_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            e.Handled = true;
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (Convert.ToBoolean(e.NewValue) == true)
                {
                    slider.Value = VolumeController.Instance.Volumn;
                    sliderValue.Text = slider.Value.ToString("f0");
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }
    }

    /// <summary>
    /// 系统音量控制器
    /// </summary>
    public class VolumeController
    {
        MMDeviceEnumerator _deviceCollections;
        MMDevice _device;
        bool _isInitSuccess;

        #region ■------------------ 单例

        private VolumeController()
        {

        }

        public readonly static VolumeController Instance = new VolumeController();

        #endregion

        public void Init()
        {
            try
            {
                _deviceCollections = new MMDeviceEnumerator();
                _device = _deviceCollections.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia);//当前选中设备
                _volumn = _device.AudioEndpointVolume.MasterVolumeLevelScalar * 100;
                _isInitSuccess = true;
            }
            catch (Exception ex)
            {
                LogHelper.Info("音量控制组件初始化失败，" + ex.Message);
            }
        }

        private double _volumn;
        public double Volumn
        {
            get
            {
                if (_device == null) return -1;
                _volumn = _device.AudioEndpointVolume.MasterVolumeLevelScalar * 100;
                return _volumn;

            }
            private set
            {
                if (_device == null) return;
                _device.AudioEndpointVolume.MasterVolumeLevelScalar = value > 1 ? 1 : (float)value;
                _volumn = _device.AudioEndpointVolume.MasterVolumeLevelScalar * 100;
            }
        }

        #region ■------------------ 外部接口

        /// <summary>
        /// 设置音量
        /// </summary>
        /// <param name="value"></param>
        public void SetVolumn(int value)
        {
            try
            {
                BrowserManager.Instance.Browser.Dispatcher.Invoke(() => {
                    if (_isInitSuccess)
                    {
                        Volumn = (double)value / 100d;
                    }
                });

            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        public int GetVolumn()
        {
            BrowserManager.Instance.Browser.Dispatcher.Invoke(() => {
                var a = Volumn;
            });
            return (int)Math.Floor(_volumn);
        }

        #endregion
    }
}
