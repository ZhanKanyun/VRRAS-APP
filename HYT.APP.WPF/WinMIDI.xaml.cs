using CL.Common;
using HYT.MidiManager;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Interop;
using System.Windows.Media;

namespace HYT.APP.WPF
{
    /// <summary>
    /// WinMIDI.xaml 的交互逻辑
    /// </summary>
    public partial class WinMIDI : Window
    {
        public WinMIDI()
        {
            InitializeComponent();
            try
            {
                MidiControl.Instance.Log += MidiControl_Log;
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

        /// <summary>
        /// 输出消息到调试控制台
        /// </summary>
        /// <param name="info"></param>
        /// <param name="type"></param>
        public void Output(string info, int type)
        {
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
                Debug.WriteLine($"Log: {info}");
                if (!info.Contains("播放"))
                {
                    LogHelper.Info(info);
                }
              
                txtMIDIOutput.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (flowdocmidi.Blocks.Count > 1000)
                    {
                        flowdocmidi.Blocks.Clear();
                    }
                    lock (this)
                    {
                        var p = new Paragraph(); // Paragraph 类似于 html 的 P 标签  
                        var r = new Run(info); // Run 是一个 Inline 的标签  
                        p.Inlines.Add(r);
                        p.Margin = new Thickness(2);
                        p.Foreground = brush;//设置字体颜色  
                        flowdocmidi.Blocks.Add(p);
                        txtMIDIOutput.ScrollToEnd();
                    }
                }));
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        private void MidiControl_Log(object? sender, string e)
        {
            try
            {
                Output(e, 1);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }
    }
}
