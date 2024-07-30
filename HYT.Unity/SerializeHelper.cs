using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KT.TCP
{
    public class SerializeHelper
    {
        public SerializeHelper() { }
        public static readonly SerializeHelper Instance = new SerializeHelper();

        /// <summary>
        /// 对象序列化
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        public byte[] SerializeObject(object obj)
        {
            if ((obj.GetType().Attributes & TypeAttributes.Serializable) == TypeAttributes.Serializable)
            {
                MemoryStream stream = new MemoryStream();
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                formatter.Serialize(stream, obj);
                byte[] buffer = stream.GetBuffer();
                stream.Close();
                return buffer;
            }
            else
                return null;
        }

        /// <summary>
        /// 反序列化为对象
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public T DeserializeObject<T>(byte[] byteArray) where T : class
        {
            try
            {
                MemoryStream stream = new MemoryStream(byteArray);
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                stream.Position = 0;
                object obj = formatter.Deserialize(stream);
                stream.Close();
                if (obj is T)
                {
                    return obj as T;
                }
                else
                {
                    return null;
                }
            }
            catch { return null; }

        }
    }
}
