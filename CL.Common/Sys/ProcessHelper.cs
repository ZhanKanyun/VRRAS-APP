using CL.Common;
using System.Diagnostics;

namespace CL.Common
{
    public class ProcessHelper
    {
        /// <summary>
        /// 杀死指定名称的进程（排除当前进程）
        /// </summary>
        /// <returns></returns>
        public static bool Kill(string processName)
        {
            try
            {
                Process current = Process.GetCurrentProcess();
                Process[] processes = Process.GetProcessesByName(processName);
                foreach (Process process in processes)
                {
                    if (process.Id != current.Id)//检测进程ID
                    {
                        process.Kill();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// 杀死包含指定名称的所有相关进程（排除当前进程）
        /// </summary>
        /// <param name="matchName"></param>
        /// <returns></returns>
        public static bool KillMatchName(string matchName)
        {
            
                Process current = Process.GetCurrentProcess();
                var processes = Process.GetProcesses();
                foreach (var process in processes)
                {
                    if (process.ProcessName.Contains(matchName))
                    {
                        if (process.Id != current.Id)
                        {
                            Console.WriteLine($"【训练进程检测】检测到 {process.Id}-{process.ProcessName} ，杀死进程");
                            process.Kill();
                        }
                    }
                }

                return true;
            
        }

        /// <summary>
        /// 杀死包含指定名称的所有相关进程（排除当前进程）
        /// </summary>
        /// <param name="matchNames"></param>
        /// <returns></returns>
        public static bool KillMatchNames(List<string> matchNames)
        {
            
                Process current = Process.GetCurrentProcess();
                var processes = Process.GetProcesses();

                foreach (var process in processes)
                {
                    foreach (var matchName in matchNames)
                    {
                        if (process.ProcessName.Contains(matchName))
                        {
                            if (process.Id != current.Id)
                            {
                                Console.WriteLine($"【训练进程检测】检测到 {process.Id}-{process.ProcessName} ，杀死进程");
                                process.Kill();
                            }
                        }
                    }
                }

                return true;
     
        }

        /// <summary>
        /// 杀死训练相关进程
        /// </summary>
        /// <param name="matchNames"></param>
        /// <returns></returns>
        public static bool KillTrainProcess()
        {
            
            List<string> matchNames = new List<string>();
            matchNames.Add("Train");
            matchNames.Add("HYT");
            //matchNames.Add("SWBT");
            Process current = Process.GetCurrentProcess();
                var processes = Process.GetProcesses();

                foreach (var process in processes)
                {
                    foreach (var matchName in matchNames)
                    {
                        if (process.ProcessName.Contains(matchName))
                        {
                            if (process.Id != current.Id)
                            {
                                Console.WriteLine($"【训练进程检测】检测到 {process.Id}-{process.ProcessName} ，杀死进程");
                                process.Kill();
                            }
                        }
                    }
                }

                return true;
        
        }

        /// <summary>
        /// 杀死包含指定名称的所有相关进程（排除当前进程）
        /// </summary>
        /// <param name="matchNames"></param>
        /// <returns></returns>
        public static bool KillMatchNames(string matchNames)
        {
            
                List<string> listMatchName = matchNames.Split('|').ToList();
                return KillMatchNames(listMatchName);
         
        }
    }
}
