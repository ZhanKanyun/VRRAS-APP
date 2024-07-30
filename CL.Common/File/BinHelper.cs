using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CL.Common
{
    public class BinHelper
    {
        /// <summary>
        /// 序列化对象
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        public static byte[] SerializeObject(object obj)
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
        /// 将对象压缩保存为文件
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="filePath"></param>
        public static bool SaveToFile(object obj, string filePath)
        {

            if ((obj.GetType().Attributes & TypeAttributes.Serializable) == TypeAttributes.Serializable)
            {
                MemoryStream source = new MemoryStream(SerializeObject(obj));
                FileStream destination = new FileStream(filePath, FileMode.Create);
                DeflateStream zipStream = new DeflateStream(destination, CompressionMode.Compress);
                source.CopyTo(zipStream);
                zipStream.Close();
                destination.Close();
                source.Close();
                return true;
            }
            else
            {
                throw new Exception("未标记序列化");
            }


        }
            /// <summary>

        /// <summary>
        /// 从文件中读出对象
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static object OpenFromFile(string filePath)
        {
            FileStream source = new FileStream(filePath, FileMode.Open);
            DeflateStream zipStream = new DeflateStream(source, CompressionMode.Decompress);
            MemoryStream destination = new MemoryStream();
            try
            {
                zipStream.CopyTo(destination);
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                destination.Position = 0;
                object obj = formatter.Deserialize(destination);
                destination.Close();
                zipStream.Close();
                source.Close();
                return obj;
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                destination.Close();
                zipStream.Close();
                source.Close();
            }
        }
    }
}
