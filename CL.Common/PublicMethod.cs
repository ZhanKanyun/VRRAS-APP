using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Json;
using System.Text;

namespace CL.Common
{
    public class PublicMethod
    {
        #region ■------------------ 复制对象-反射

        /// <summary>
        /// 获取或赋值相同属性的对象，找到T对象属性相同名称的进行赋值
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="objOrg">原对象</param>
        /// <returns></returns>
        public static T GetCopy<T>(object objOrg)
        {
            var objOrgType = objOrg.GetType();
            var objOrgProps = objOrgType.GetProperties();
            var objNew = Activator.CreateInstance<T>();
            var objNewType = objNew.GetType();
            var objNewProps = objNewType.GetProperties();
            foreach (PropertyInfo objNewP in objNewProps)
            {
                if (objNewP.Name.Contains("Record")) continue;
                var query = objOrgProps.Where(x => x.Name == objNewP.Name);
                if (query != null)
                {
                    try
                    {
                        var o = query.First().GetValue(objOrg, null);
                        objNewP.SetValue(objNew, o);//给传入的数据赋值
                    }
                    catch (Exception)
                    {

                        // throw;
                    }
                }
            }
            return (T)objNew;
        }

        /// <summary>
        /// 获取或赋值相同属性的对象，找到T对象属性相同名称的进行赋值
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="objOrg">原对象</param>
        /// <param name="objOrgPropertyInfo">原对象属性</param>
        /// <param name="objNewPropertyInfo">新对象属性</param>
        /// <returns></returns>
        public static T GetCopy<T>(object objOrg, PropertyInfo[] objOrgPropertyInfo, PropertyInfo[] objNewPropertyInfo)
        {
            var objOrgProps = objOrgPropertyInfo;
            var objNew = Activator.CreateInstance<T>();
            var objNewProps = objNewPropertyInfo;
            foreach (PropertyInfo objNewP in objNewProps)
            {
                if (objNewP.Name.Contains("Record")) continue;
                var query = objOrgProps.Where(x => x.Name == objNewP.Name);
                if (query != null)
                {
                    try
                    {
                        var o = query.First().GetValue(objOrg, null);
                        objNewP.SetValue(objNew, o);//给传入的数据赋值
                    }
                    catch (Exception)
                    {

                        // throw;
                    }
                }
            }
            return (T)objNew;
        }

        /// <summary>
        /// 获取或赋值相同属性的对象，找到T对象属性相同名称的进行赋值
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <typeparam name="T1">原对象类型</typeparam>
        /// <param name="objOrg">原对象集合</param>
        /// <returns></returns>
        public static List<T> GetCopy<T, T1>(List<T1> objOrg)
        {
            var objOrgType = objOrg.GetType();
            var objOrgProps = objOrgType.GetProperties();
            var objNew = Activator.CreateInstance<T>();
            var objNewType = objNew.GetType();
            var objNewProps = objNewType.GetProperties();
            var objNewList = new List<T>();
            foreach (var item in objOrg)
            {
                var o = GetCopy<T>(item, objOrgProps, objNewProps);
                objNewList.Add(o);
            }
            return objNewList;
        }

        #endregion

        #region IsFirst 限定程序只运行一次

        [DllImport("Kernel32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr OpenMutex(uint dwDesiredAccess, int bInheritHandle, string lpName);

        [DllImport("Kernel32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr CreateMutex(IntPtr lpMutexAttributes, int bInitialOwner, string lpName);

        /// <summary>
        /// 检查指定的程序是否是第一次运行
        /// </summary>
        /// <param name="appCode">程序代号(可以自己指定)</param>
        /// <returns></returns>
        public static bool IsFirst(string appCode)
        {
            bool result = false;

            if (OpenMutex(0x1F0001, 0, appCode) == IntPtr.Zero)
            {
                CreateMutex(IntPtr.Zero, 0, appCode);
                result = true;
            }

            return result;
        }

        #endregion

        /// <summary>
        /// 获取一个对象所占内存的大小
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="t">实例</param>
        /// <returns>长度（字节数）</returns>
        public static long GetObjectByte<T>(T t) where T : class
        {
            DataContractJsonSerializer formatter = new DataContractJsonSerializer(typeof(T));
            using (MemoryStream stream = new MemoryStream())
            {
                formatter.WriteObject(stream, t);
                return stream.Length;
            }
        }

        public static string GetLocalIp()
        {
            ///获取本地的IP地址
            string AddressIP = string.Empty;
            foreach (IPAddress _IPAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (_IPAddress.AddressFamily.ToString() == "InterNetwork")
                {
                    AddressIP = _IPAddress.ToString();
                }
            }
            return AddressIP;
        }

        /// <summary>
        /// 设置该字节的某一位的值(将该位设置成0或1)
        /// </summary>
        /// <param name="data">要设置的字节byte</param>
        /// <param name="index">要设置的位， 值从低到高为 0-7</param>
        /// <param name="flag">要设置的值 true(1) / false(0)</param>
        /// <returns></returns>
        public static byte SetBitValue(byte data, int index, bool flag)
        {
            index += 1;
            if (index > 8 || index < 1)
                throw new ArgumentOutOfRangeException();
            int v = index < 2 ? index : (2 << (index - 2));
            return flag ? (byte)(data | v) : (byte)(data & ~v);
        }

        /// <summary>
        /// 从一个字节中取指定位的值
        /// </summary>
        /// <param name="value">要分析的字节</param>
        /// <param name="bitCount">取多少个位</param>
        /// <param name="rightOffset">右偏移几位，如果是2，就是从第3位开始取，就是先左偏移rightOffset，右偏移8-bitCount位</param>
        /// <returns></returns>
        public static int GetBitValue(byte value, int bitCount, int rightOffset)
        {
            if (rightOffset > 7)
            {
                throw new Exception("偏移已经达到8位");
            }
            byte result = (byte)(value << rightOffset);//先左位移
            result = (byte)(result >> 8 - bitCount);//再右位移

            return result;
        }

        /// <summary> 
        /// 字节数组转16进制字符串 
        /// </summary> 
        /// <param name="bytes"></param> 
        /// <returns></returns> 
        public static string ByteArrayToHexStr(byte[] bytes)
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    returnStr += " " + bytes[i].ToString("X2");
                }
            }
            return returnStr;
        }
    }
}
