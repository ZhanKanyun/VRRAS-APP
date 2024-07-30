using CL.Common;
using KCL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HYT.APP.WPF.Manager
{
    public class KeyBoardManager
    {
        private KeyBoardManager() { }
        public static readonly KeyBoardManager Instance = new KeyBoardManager();

        [DllImport("user32.dll")]
        static extern int SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        private const int WM_ACTIVATE = 0x0006;
        private readonly IntPtr WA_ACTIVE = new IntPtr(1);
        private const int SC_RESTORE = 0xF120;
        #region ■------------------ 屏幕键盘

        Process _processScreenKey;

        /// <summary>
        /// 显示屏幕键盘
        /// </summary>
        /// <returns></returns>
        public int Show()
        {
            try
            {
                if (_processScreenKey == null)
                {
                    //创建启动对象
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.FileName = "C:\\windows\\system32\\osk.exe";
                    startInfo.UseShellExecute = true;
       
                    _processScreenKey = Process.Start(startInfo);

                    //_processInputPanel = Process.Start(@"C:\windows\system32\osk.exe");
                    BrowserManager.Instance.Browser.Focus();
                }
                else
                {
                    if (_processScreenKey.HasExited)//已经通过关闭按钮退出 进程已经退出 重开
                    {
                        _processScreenKey = null;

                        ProcessStartInfo startInfo = new ProcessStartInfo();
                        startInfo.FileName = "C:\\windows\\system32\\osk.exe";
                        startInfo.UseShellExecute = true;
                        _processScreenKey = Process.Start(startInfo);

                        BrowserManager.Instance.Browser.Focus();
                    }
                    else
                    {
                        //ShowWindow(_processScreenKey.MainWindowHandle, 5);
                        SendMessage(_processScreenKey.MainWindowHandle, WM_SYSCOMMAND, (IntPtr)SC_RESTORE, IntPtr.Zero);
                    }
                }

                return 1;
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return 255;
            }
        }

        /// <summary>
        /// 隐藏屏幕键盘
        /// </summary>
        public void Hide()
        {
            try
            {
                if (_processScreenKey != null)
                {
                    if (!_processScreenKey.HasExited)
                    {
                        _processScreenKey.Kill();
                    }
                    _processScreenKey = null;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        #endregion


        #region ■------------------ 触摸键盘


        private const Int32 WM_SYSCOMMAND = 274;
        private const UInt32 SC_CLOSE = 61536;
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool PostMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool PostMessage(IntPtr hWnd, int Msg, uint wParam, uint lParam);
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int RegisterWindowMessage(string lpString);

        Process _processInputPanel;
        //显示屏幕键盘
        public int ShowTouchInput()
        {
            try
            {
                if (_processScreenKey == null)
                {
                    dynamic file = "C:\\Program Files\\Common Files\\microsoft shared\\ink\\TabTip.exe";
                    if (!System.IO.File.Exists(file))
                        return -1;

                    //创建启动对象
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.FileName = "C:\\Program Files\\Common Files\\microsoft shared\\ink\\TabTip.exe";
                    startInfo.UseShellExecute = true;
                    _processScreenKey = Process.Start(startInfo);

                    //_processInputPanel = Process.Start(@"C:\windows\system32\osk.exe");
                    BrowserManager.Instance.Browser.Focus();
                }
                else
                {
                    if (_processScreenKey.HasExited)//已经通过关闭按钮退出 进程已经退出 重开
                    {
                        _processScreenKey = null;

                        ProcessStartInfo startInfo = new ProcessStartInfo();
                        startInfo.FileName = "C:\\Program Files\\Common Files\\microsoft shared\\ink\\TabTip.exe";
                        startInfo.UseShellExecute = true;
                        _processScreenKey = Process.Start(startInfo);

                        BrowserManager.Instance.Browser.Focus();
                    }
                }

                return 1;

            }
            catch (Exception)
            {
                return 255;
            }
        }

        //隐藏屏幕键盘
        public void HideTouchInput()
        {
            try
            {
                IntPtr TouchhWnd = new IntPtr(0);
                TouchhWnd = FindWindow("IPTip_Main_Window", null);
                if (TouchhWnd == IntPtr.Zero)
                    return;
                PostMessage(TouchhWnd, WM_SYSCOMMAND, SC_CLOSE, 0);
            }
            catch (Exception ex)
            {

            }
            try
            {
                if (_processScreenKey != null)
                {
                    if (!_processScreenKey.HasExited)
                    {
                        _processScreenKey.Kill();
                    }
                    _processScreenKey = null;
                }
            }
            catch (Exception ex)
            {

            }

        }


        #endregion

    }
}
