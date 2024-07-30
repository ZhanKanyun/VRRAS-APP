using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace KCL
{
    /// <summary>
    /// JS接口响应信息
    /// </summary>
    public class JSAPIResponse
    {
        public JSAPIResponse() { }

        /// <summary>
        /// 响应代码 200=成功
        /// </summary>
        public int Code
        {
            get { return (int)JSAPIResponseCode; }
        }

        public JSAPIResponseCodes JSAPIResponseCode { get; set; }
        public bool IsSuccess
        {
            get
            {
                return JSAPIResponseCode == JSAPIResponseCodes.Success;
            }
        }
        public bool success
        {
            get
            {
                return JSAPIResponseCode == JSAPIResponseCodes.Success;
            }
        }

        /// <summary>
        /// 响应信息 显示信息
        /// </summary>
        public string message { get; set; }

        /// <summary>
        /// 异常信息
        /// </summary>
        public string exception { get; set; }

        /// <summary>
        /// 字符串数据
        /// </summary>
        public string data_str1 { get; set; }
        public string data_str2 { get; set; }

        public int data_int { get; set; }

        /// <summary>
        /// 对象数据 可以是集合
        /// </summary>
        public object data { get; set; } = null;

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }


        /// <summary>
        /// 成功
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static JSAPIResponse Success(string msg = "")
        {
            return new JSAPIResponse()
            {
                JSAPIResponseCode = JSAPIResponseCodes.Success,
                message = string.IsNullOrEmpty(msg) ? "操作成功" : msg,
            };
        }
        /// <summary>
        /// 成功
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static JSAPIResponse Success(object data, string msg = "", string dataStr1 = "", string dataStr2 = "", int dataInt = 0)
        {
            return new JSAPIResponse()
            {
                JSAPIResponseCode = JSAPIResponseCodes.Success,
                message = string.IsNullOrEmpty(msg) ? "操作成功" : msg,
                data = data,
                data_str1 = dataStr1,
                data_str2 = dataStr2,
                data_int = dataInt
            };
        }

        /// <summary>
        /// 失败
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static JSAPIResponse Error(string msg = "", object data = null, string dataStr1 = "", string dataStr2 = "", int dataInt = 0)
        {
            return new JSAPIResponse()
            {
                JSAPIResponseCode = JSAPIResponseCodes.Error,
                message = string.IsNullOrEmpty(msg) ? "操作失败" : msg,
                data = data,
                data_str1 = dataStr1,
                data_str2 = dataStr2,
                data_int = dataInt
            };
        }

        /// <summary>
        /// 异常
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static JSAPIResponse Exception(System.Exception ex)
        {
            return new JSAPIResponse()
            {
                JSAPIResponseCode = JSAPIResponseCodes.Exception,
                message = "请求异常，错误信息："+ ex.Message,
                exception = ex.Message + "\r\n" + ex.StackTrace
            };
        }
    }

    public enum JSAPIResponseCodes
    {
        Success = 200,
        Error = 0,
        Exception = 110
    }

    [Serializable]
    public class JSAPIPage<T>
    {
        public JSAPIPage(int count, List<T> data, int pageIndex)
        {
            Count = count;
            Data = data;
            PageIndex = pageIndex;
        }
        public int Count { get; set; }
        /// <summary>
        /// 删除数据后，当前页可能会没数据 自动减1 发回去更新
        /// </summary>
        public int PageIndex { get; set; }
        public List<T> Data { get; set; }
    }
}
