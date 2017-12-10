using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Runtime.InteropServices;
using App.Schedule;
using App.Components;

namespace App.Consoler
{
    /// <summary>
    /// 定时调度控制台
    /// </summary>
    class Program
    {
        //
        static ScheduleEngine engine;

        // 主入口
        static void Main(string[] args)
        {
            // 确保只有一个程序在运行
            if (AreadyExistInstance())
            {
                Console.WriteLine("本程序已有一个实例在运行。");
                return;
            }

            // 设置关闭事件
            //SetConsoleCtrlHandler(new AppQuitDelegate(AppQuit), true);

            // 开启任务引擎
            Console.WriteLine("App.Schedule consoler {0}", ReflectionHelper.AssemblyVersion);
            string configFile = string.Format("{0}\\schedule.config", ReflectionHelper.AssemblyDirectory);
            engine = new ScheduleEngine(configFile);
            engine.ConfigFailure += (info) => Logger.Error("{0}", info);
            engine.TaskSuccess += (task, info) => Logger.Info("{0} {1} ok", task.Name, task.Data);
            engine.TaskFailure += (task, info) => Logger.Warn("{0} {1} fail, times={2}, info={3}", task.Name, task.Data, task.Failure.TryTimes, info);
            engine.Start();
        }


        // 是否已经存在进程实例
        public static bool AreadyExistInstance()
        {
            Process currentProcess = Process.GetCurrentProcess();
            string currentFileName = currentProcess.MainModule.FileName;
            Process[] processes = Process.GetProcessesByName(currentProcess.ProcessName);
            foreach (Process process in processes)
                if (process.MainModule.FileName == currentFileName)
                    if (process.Id != currentProcess.Id)
                        return true;
            return false;
        }

        // 控制台关闭时写入日志
        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleCtrlHandler(AppQuitDelegate quit, bool add);
        public delegate bool AppQuitDelegate(int ctrl);
        public static bool AppQuit(int ctrl)
        {
            try
            {
                Logger.Warn("Program exit");
                engine?.Stop();
            }
            finally {}
            return false;
        }
    }
}
