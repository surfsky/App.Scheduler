using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Runtime.InteropServices;
using App.Scheduler;
using System.Reflection;
using System.IO;

namespace App.Consoler
{
    /// <summary>
    /// 定时调度控制台
    /// </summary>
    class Program
    {
        // 调度引擎对象
        static ScheduleEngine engine;

        //------------------------------------------------
        // 主入口
        //------------------------------------------------
        static void Main(string[] args)
        {
            // 确保只有一个程序在运行
            if (AreadyExistInstance())
            {
                Console.WriteLine("本程序已有一个实例在运行。");
                return;
            }
            // 设置程序关闭事件（自动保存配置）：不稳定会报错
            //SetConsoleCtrlHandler(new AppQuitDelegate(AppQuit), true);

            // Start
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            Console.WriteLine("App.Scheduler consoler {0}", version);
            Console.WriteLine("Engine version {0}", App.Scheduler.ScheduleEngine.Version);

            // 调度引擎
            var folder = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;
            string configFile = string.Format("{0}\\scheduler.config", folder);
            engine = new ScheduleEngine(configFile);
            engine.ConfigFailure += (info) => { Logger.Error("{0}", info); Console.ReadKey(); };
            engine.JobRunning += (job, info) => Logger.Info("{0} {1} running", job.Name, job.Data);
            engine.JobSuccess += (job, info) => Logger.Info("{0} {1} ok", job.Name, job.Data);
            engine.JobFailure += (job, info) => Logger.Warn("{0} {1} fail, times={2}, info={3}", job.Name, job.Data, job.Failure.TryTimes, info);
            //engine.ConfigSave += (cfg) => cfg.Save();
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
