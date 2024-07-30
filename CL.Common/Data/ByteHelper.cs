using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CL.Common.Data
{
    /// <summary>
    /// 字节处理
    /// </summary>
    public class ByteHelper
    {
        /// <summary>
        /// 从1个字节中取多个位的值
        /// </summary>
        /// <param name="source">要分析的字节</param>
        /// <param name="index">从右数第多少位开始，0开始</param>
        /// <param name="bitCount">取多少个位</param>
        /// <returns></returns>
        public static byte GetBitValueFromOne(byte source, int index, int bitCount)
        {
            if (index<0 || index > 7)
            {
                throw new ArgumentOutOfRangeException();
            }
            byte result = (byte)(source << 8-1-index);//先左位移
            result = (byte)(result >> 8 - bitCount);//再右位移

            return result;
        }

        /// <summary>
        /// 从2个字节中取多个位的值
        /// </summary>
        /// <param name="high"></param>
        /// <param name="low"></param>
        /// <param name="index">从右数第多少位开始，0开始</param>
        /// <param name="bitCount">取多少个位</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static ushort GetBitValueFromTwo(byte high,byte low, int index, int bitCount)
        {
            if (index < 0 || index > 15)
            {
                throw new ArgumentOutOfRangeException();
            }
            byte[] data=new byte[2];
            data[0] = low;
            data[1] = high;
            ushort source=BitConverter.ToUInt16(data,0);

            ushort result = (ushort)(source << 16 - 1 - index);//先左位移
            result = (ushort)(result >> 16 - bitCount);//再右位移

            return result;
        }

        /// <summary>
        /// 单字节 设置多位
        /// </summary>
        /// <param name="source">源字节</param>
        /// <param name="index">从右边数第几位 0开始</param>
        /// <param name="bitCount"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static byte SetBitValueToOne(byte source, int index, int bitCount, byte value)
        {
            if (index < 0 || index > 7)
            {
                throw new ArgumentOutOfRangeException();
            }

            //要设置的位全部设为0，先取反再与
            byte temp = (byte)(Math.Pow(2, bitCount) - 1);
            temp = (byte)(value << index - (bitCount - 1));
            source = (byte)(source & ~temp);

            //或运算
            value = (byte)(value << index - (bitCount - 1));
            source = (byte)(source | value);

            return source;
        }

        /// <summary>
        /// 单字节  设置1位
        /// </summary>
        /// <param name="source"></param>
        /// <param name="index">从右边数第几位 0开始</param>
        /// <param name="flag">设为0还是1</param>
        /// <returns></returns>
        public static byte SetbitValue(byte source, int index, int flag)
        {
            if (index > 7 || index < 0)
                throw new ArgumentOutOfRangeException();
            int v = 1 << index;
            return flag==1 ? (byte)(source | v) : (byte)(source & ~v);
        }
    }
}
