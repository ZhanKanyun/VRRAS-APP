
using CL.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using static System.Windows.Forms.AxHost;

namespace KCL
{
    /// <summary>
    /// JS接口-评测模块
    /// </summary>
    public class Assess_JSAPI
    {
        #region ■------------------ 单例

        private Assess_JSAPI() { }

        public readonly static Assess_JSAPI Instance = new Assess_JSAPI();

        #endregion



        /// <summary>
        /// 暂停记录数据
        /// </summary>
        public string StartRecord()
        {
            try
            {
                if (DeviceDataAnalysisManager.Instance.Start())
                {
                    return JSAPIResponse.Success().ToJson();
                }
                else
                {
                    return JSAPIResponse.Error().ToJson();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return JSAPIResponse.Exception(ex).ToJson();
            }
        }

        /// <summary>
        /// 暂停记录数据
        /// </summary>
        public string PauseRecord()
        {
            try
            {
                DeviceDataAnalysisManager.Instance.Pause();
                return JSAPIResponse.Success().ToJson();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return JSAPIResponse.Exception(ex).ToJson();
            }
        }

        /// <summary>
        /// 继续记录数据
        /// </summary>
        public string ContinueRecord()
        {
            try
            {
                DeviceDataAnalysisManager.Instance.Continue();
                return JSAPIResponse.Success().ToJson();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return JSAPIResponse.Exception(ex).ToJson();
            }
        }

        /// <summary>
        /// 停止记录数据-web调用
        /// </summary>
        public string StopRecord()
        {
            try
            {
                DeviceDataAnalysisManager.Instance.Stop();

                return JSAPIResponse.Success(DeviceDataAnalysisManager.Instance.CurrentGaitRecord).ToJson() ;   
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return JSAPIResponse.Exception(ex).ToJson();
            }
        }

        /// <summary>
        /// 获取当前记录数据-web调用
        /// </summary>
        public string GetCurrentRecord()
        {
            try
            {
                return JSAPIResponse.Success(DeviceDataAnalysisManager.Instance.CurrentGaitRecord).ToJson();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return JSAPIResponse.Exception(ex).ToJson();
            }
        }
    }
}
