using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CL.Common
{
    public class LogHelper
    {
        public readonly static Logger logger = LogManager.LoadConfiguration(@"Content\nlog.config").GetCurrentClassLogger();
        public static void Debug(string messsage)
        {
            logger.Debug(messsage);
        }
        public static void Info(string message)
        {
            logger.Info(message);
        }
        public static void Warn(string message)
        {
            logger.Warn(message);
        }
        public static void Error(Exception ex)
        {
            logger.Error(ex);
        }
        public static void Error(string ex)
        {
            logger.Error(ex);
        }
        public static void Error(Type type, Exception ex)
        {
            logger.Error(ex);
        }
        public static void Fatal(string message)
        {
            logger.Fatal(message);
        }
    }
}
