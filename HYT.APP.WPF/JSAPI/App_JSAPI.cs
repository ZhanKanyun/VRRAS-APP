using CL.Common;
using HYT.APP.WPF.Manager;
using HYT.MidiManager;
using KT.TCP;
using Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Path = System.IO.Path;

namespace KCL
{
    [ComVisible(true)]
    public class App_JSAPI
    {
        private App_JSAPI() { }
        public static readonly App_JSAPI Instance = new App_JSAPI();

        /// <summary>
        /// 获取数据缓存：当前用户
        /// 网页刷新，不影响后台数据
        /// </summary>
        /// <returns></returns>
        public string JS_GetDataCache()
        {
            try
            {
                //上次登录用户更新
                //if (AppManager.Instance.CurrentPatient == null)
                //{
                //    var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Content\lastPA.bin");
                //    if (File.Exists(path))
                //    {
                //        using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                //        {
                //            byte[] buffer = new byte[fs.Length];
                //            fs.Read(buffer, 0, (int)fs.Length);

                //            string id = Encoding.UTF8.GetString(buffer);


                            

                //            tb_Patient pa = DBHelper.Instance.GetOneByID<tb_Patient>(id);
                //            if (pa != null)
                //            {
                //                AppManager.Instance.CurrentPatient = pa;
                //            }
                //            else
                //            {

                //            }
                //        }
                //    }
                //}


                return JSAPIResponse.Success(new { patient = AppManager.Instance.CurrentPatient ,testMusic=AppManager.Instance.CurrentWebTestMusic}).ToJson();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return JSAPIResponse.Exception(ex).ToJson();
            }
        }

        #region ■------------------ 当前用户（用户）

        /// <summary>
        /// 获取当前用户 
        /// </summary>
        /// <returns></returns>
        public string JS_GetCurrentPatient()
        {
            try
            {
                if (AppManager.Instance.CurrentPatient==null)
                {
                    try
                    {
                        //上次登录用户更新
                        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Content\lastPA.bin");
                        if (File.Exists(path))
                        {
                            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                            {
                                byte[] buffer=new byte[fs.Length];
                                fs.Read(buffer,0, (int)fs.Length);

                                string id= Encoding.UTF8.GetString(buffer,0,buffer.Length);
                                tb_Patient pa = DBHelper.Instance.GetOneByID<tb_Patient>(id);
                                if (pa != null)
                                {
                                    AppManager.Instance.CurrentPatient = pa;
                                }
                                else
                                {
                                    
                                }
                            }
                        }
                       
                    }
                    catch (Exception ex)
                    {

                    }
                }
               
                return JSAPIResponse.Success(AppManager.Instance.CurrentPatient).ToJson();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return JSAPIResponse.Exception(ex).ToJson();
            }
        }

        /// <summary>
        /// 切换当前用户
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string JS_ChangeCurrentPatient(string id)
        {
            try
            {
                tb_Patient pa=DBHelper.Instance.GetOneByID<tb_Patient>(id);
                if (pa == null)
                {
                    return JSAPIResponse.Error("不存在").ToJson();
                }
                AppManager.Instance.CurrentPatient = pa;

                //修改最后使用时间
                var d_now = DateTime.Now;
                pa.LastLoginTime = new DateTime(d_now.Year, d_now.Month, d_now.Day, d_now.Hour, d_now.Minute, d_now.Second);
                DBHelper.Instance.UpdateColumns<tb_Patient>(pa, "LastLoginTime");

                try
                {
                    //上次登录用户更新
                    var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Content\lastPA.bin");
                    using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                    {
                        byte[] buffer = Encoding.UTF8.GetBytes(id);

                        fs.Write(buffer, 0, buffer.Length);
                    }
                }
                catch (Exception ex)
                {

                }

                return JSAPIResponse.Success(pa,"切换成功").ToJson();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return JSAPIResponse.Exception(ex).ToJson();
            }
        }

        #endregion

        #region ■------------------ 其它

        ///// <summary>
        ///// 浏览文件夹
        ///// </summary>
        ///// <returns></returns>
        //public string JS_BrowserFolder()
        //{
        //    try
        //    {
        //        string path = "";
        //        if (AppManager.Instance.Window != null)
        //        {
        //            //AppManager.Instance.Window.Dispatcher.Invoke(() => {
        //            //    FolderBrowserDialog dialog = new FolderBrowserDialog();
        //            //    dialog.RootFolder = Environment.SpecialFolder.Desktop;
        //            //    dialog.Description = "请选择文件路径";
        //            //    if (dialog.ShowDialog() == DialogResult.OK)
        //            //    {
        //            //        //BrowserManager.Instance.Browser.Visible = true;
        //            //        path = dialog.SelectedPath;
        //            //    }
        //            //});
        //        }
        //        if (path != "")
        //        {
        //            //return DB_JSAPI.Instance.Setting_SetFileSavePath(path);
        //        }
        //        else
        //        {
        //            return JSAPIResponse.Error() .ToJson();
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.Error(ex);
        //        return JSAPIResponse.Exception(ex).ToJson();
        //    }
        //}

        /// <summary>
        /// Web 消息通知
        /// </summary>
        /// <param name="message"></param>
        /// <param name="type"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public string Notification(string message, string type, int duration)
        {
            try
            {
                BrowserManager.Instance.ExecuteJSAsync($"API_CSharp.notification('{message}','{type}',{duration})");
                return JSAPIResponse.Success().ToJson();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return JSAPIResponse.Exception(ex).ToJson();
            }
        }

        /// <summary>
        /// 打印 未使用
        /// </summary>
        /// <returns></returns>
        public string Print()
        {
            try
            {
                //BrowserManager.Instance.Browser.Print();
                return JSAPIResponse.Success().ToJson();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return JSAPIResponse.Exception(ex).ToJson();
            }
        }

        /// <summary>
        /// 退出系统
        /// </summary>
        /// <returns></returns>
        public string Exit()
        {
            try
            {
                //System.Windows.Application.Current.Dispatcher.Invoke(() => {
                //    System.Windows.Application.Current.Shutdown();
                //});
                AppManager.Instance.Exit();
                System.Environment.Exit(0);//不会触发 APP.OnExit

                return JSAPIResponse.Success().ToJson();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return JSAPIResponse.Exception(ex).ToJson();
            }
        }

        #endregion

        #region ■------------------ 训练

        public string StartGame(string train_startInfo)
        {
            try
            {
                try
                {
                    KeyBoard_Hide();
                }
                catch (Exception ex)
                {
      
                }

                if (!MidiControl.Instance.IsLoadDone)
                {
                    return JSAPIResponse.Error("音乐文件加载中，请稍后...").ToJson();
                }

                //TODO 设备检测
                if (!TCPServer.Instance.IsStarted)
                {
                    ProcessHelper.KillTrainProcess();
                    return JSAPIResponse.Error("通信服务器未运行，请稍后重试").ToJson();
                }

                TrainStartInfo info = JsonConvert.DeserializeObject<TrainStartInfo>(train_startInfo);

                if (AppManager.Instance.CurrentPatient.DiseaseType== "帕金森")
                {
                    var db_trainMusics = DBHelper.Instance.GetWhere<tb_TrainMusic>(o => o.TrainID == info.id);
                    if (db_trainMusics.Count <= 0)
                    {
                        return JSAPIResponse.Error("请先为该训练或游戏设置音乐").ToJson();
                    }
                }
              

                if (info == null)
                {
                    return JSAPIResponse.Error("训练启动信息为空").ToJson();
                }

                if (!TrainTaskManager.Instance.IsInit)
                {
                    return JSAPIResponse.Error("训练组件未初始化").ToJson();
                }

                if (TrainTaskManager.Instance.CurrentTask != null)
                {
                    return JSAPIResponse.Error("训练任务未完成").ToJson();
                }

                TrainTaskManager.Instance.Start(info);

                ConfigManager.Instance.Load();

                //App_JSAPI.Instance.Notification("训练启动成功","success",2000);
                return JSAPIResponse.Success("启动成功").ToJson();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return JSAPIResponse.Exception(ex).ToJson();
            }
        }

        #endregion

        #region ■------------------  跑步机

        public string DeviceSetSpeed(float val)
        {
            try
            {                
                DeviceManager.Instance.SetSpeed(val);
                return JSAPIResponse.Success().ToJson();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return JSAPIResponse.Exception(ex).ToJson();
            }
        }
        public string DeviceStop( )
        {
            try
            {                
                DeviceManager.Instance.Stop();
                return JSAPIResponse.Success().ToJson();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return JSAPIResponse.Exception(ex).ToJson();
            }
        }
        #endregion

        #region ■------------------  虚拟键盘

        public string KeyBoard_Show()
        {
            try
            {
                if (ConfigManager.Instance.SSetting.KeyBoardType == 2)
                {
                    KeyBoardManager.Instance.Show();
                }
                else if (ConfigManager.Instance.SSetting.KeyBoardType == 1)
                {
                    KeyBoardManager.Instance.ShowTouchInput();
                }

                return JSAPIResponse.Success().ToJson();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return JSAPIResponse.Exception(ex).ToJson();
            }
        }

        public string KeyBoard_Hide()
        {
            try
            {
                if (ConfigManager.Instance.SSetting.KeyBoardType == 2)
                {
                    KeyBoardManager.Instance.Hide();
                }
                else if (ConfigManager.Instance.SSetting.KeyBoardType == 1)
                {
                    KeyBoardManager.Instance.HideTouchInput();
                }
                return JSAPIResponse.Success().ToJson();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return JSAPIResponse.Exception(ex).ToJson();
            }
        }

        #endregion


    }
}
