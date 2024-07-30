using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KT.TCP
{
    /// <summary>
    /// 客户端日志
    /// </summary>
    public class TCPLog
    {
        public TCPLogTypes LogType;

        public Type type;

        public string Log;

        public override string ToString()
        {
            return $"{type.FullName} {Log}";
        }
    }

    public enum TCPLogTypes
    {
        Info,
        Error
    }
}
