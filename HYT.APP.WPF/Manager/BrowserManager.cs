using CL.Common;
using Microsoft.Web.WebView2.Core;
//using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.IO;

namespace KCL
{
    public class BrowserManager
    {
        private BrowserManager() { }
        public static readonly BrowserManager Instance = new BrowserManager();


        public WebView2 Browser { get; private set; }

        public async void InitBrowser(WebView2 browser, string appPath, string appTestPath, Action callback)
        {
            try
            {
                LogHelper.Info("Browser InitBrowser Run");
                Browser = browser;

                var cachePath = AppDomain.CurrentDomain.BaseDirectory + "cache";

                var env = await CoreWebView2Environment.CreateAsync(userDataFolder: cachePath,
                      browserExecutableFolder: null,
                      options: new CoreWebView2EnvironmentOptions("-disable-web-security --allow-file-access-from-files"));

                await Browser.EnsureCoreWebView2Async(env);

                LogHelper.Info("Browser EnsureCoreWebView2Async");
                Browser.CoreWebView2.Settings.AreHostObjectsAllowed = true;
                Browser.CoreWebView2.Settings.IsZoomControlEnabled = false;//禁止鼠标缩放
                string path = appTestPath;
                if (string.IsNullOrEmpty(path) || !File.Exists(path))//测试路径为空，就使用正式路径
                {
                    if (string.IsNullOrEmpty(appPath))
                    {
                        path = AppDomain.CurrentDomain.BaseDirectory + "Web\\index.html";
                    }
                    else
                    {
                        path = AppDomain.CurrentDomain.BaseDirectory + appPath;
                    }
                }


                //Browser.CoreWebView2.AddHostObjectToScript("bridge", new Bridge());


                Browser.Source = new Uri(path);
                LogHelper.Info($"Browser Source={path}");
                Browser.CoreWebView2.AddHostObjectToScript("CSharp_App", App_JSAPI.Instance);
                Browser.CoreWebView2.AddHostObjectToScript("CSharp_DB", DB_JSAPI.Instance);
                Browser.CoreWebView2.AddHostObjectToScript("CSharp_Device", Device_JSAPI.Instance);
                Browser.CoreWebView2.AddHostObjectToScript("CSharp_Assess", Assess_JSAPI.Instance);
                Browser.CoreWebView2.AddHostObjectToScript("CSharp_DeviceDataAnalysisManager", DeviceDataAnalysisManager_JSAPI.Fun);
                //Browser.CoreWebView2.AddHostObjectToScript("DeviceDataAnalysisManager", DeviceDataAnalysisManager.Instance);
                Browser.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;//关闭右键菜单

                //监听消息
                //Browser.WebMessageReceived += Browser_WebMessageReceived;

                callback?.Invoke();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        private void Browser_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                //接收到的字符串
                string msg = e.TryGetWebMessageAsString();
                //接收到的json
                string msgJson = e.WebMessageAsJson;
            }
            catch (Exception)
            {

            }
        }

        public void ExecuteJSAsync(string script)
        {
            try
            {
                //Browser.Dispatcher.Invoke(() => { });

                Browser.Dispatcher.Invoke(() =>
                {
                    if (Browser.CoreWebView2 != null)
                    {
                        Browser.CoreWebView2.ExecuteScriptAsync(script);
                    }
                });

            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }
    }
}
