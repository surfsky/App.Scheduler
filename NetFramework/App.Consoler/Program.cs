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
            finally { }
            return false;
        }

        //------------------------------------------------
        // 主入口
        //------------------------------------------------
        static void Main(string[] args)
        {
            // 确保只有一个程序在运行
            if (Sys.AreadyExistInstance())
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

            // Engine
            //engine = CreateEngineFromConfig();
            //engine.JobStart   += (job, info, _) => Logger.Info("{0} {1} start", job.Name, job.Data);
            //engine.ConfigSave += (cfg) => cfg.Save();
            engine = CreateEngine();
            engine.ConfigFailure += (info) => { Logger.Error("{0}", info); Console.ReadKey(); };
            engine.JobRunning += (job, info, _) =>
            {
                JobContext.Current["name"] = "hello";
                Logger.Info("{0} {1}", job.Name, job.Data); // ⋙▷...
            };
            engine.JobFinish += (job, info, success) =>
            {
                if (success)
                    Logger.Info(@"{0} {1} √", job.Name, job.Data);
                else
                    Logger.Warn(@"{0} {1} ×, times={2}, info={3}", job.Name, job.Data, job.Failure.TryTimes, info);
            };
            engine.Start();
        }


        /// <summary>用代码构建引擎对象</summary>
        static ScheduleEngine CreateEngine()
        {
            ScheduleConfig cfg = new ScheduleConfig();
            cfg.LogDt = DateTime.Now;
            cfg.Sleep = 200;
            cfg.Jobs = new List<Job>();
            cfg.Jobs.Add(new Job()
            {
                ID = "1",
                Name = "Connect",
                Runner = typeof(ConnectJob),
                Data = "http://www.baidu.com",
                Success = new DateSpan(0, 0, 0, 0, 0, 2),
                Failure = new DateSpan(0, 0, 0, 0, 0, 0, 0, 9),
                Schedule = new Schedule("* * * * * * *")
                //Schedule = new Schedule(days: new List<int> { 1 })
            });
            /*
            cfg.Jobs.Add(new Job()
            {
                ID = "2",
                Name = "Ramdom",
                Runner = typeof(RandomJob),
                Success = new DateSpan(0, 0, 0, 0, 0, 1),
                Failure = new DateSpan(0, 0, 0, 0, 0, 0, 0, 9),
                Schedule = new Schedule("* * * * * *")
            });
            cfg.Jobs.Add(new Job()
            {
                ID = "3",
                Name = "Dummy",
                Runner = typeof(DummyJob),
                Data = "1",
                Success = new DateSpan(0, 0, 0, 0, 0, 1),
                Failure = new DateSpan(0, 0, 0, 0, 0, 0, 0, 9),
                Schedule = new Schedule("* * * * * *")
            });
            */
            return new ScheduleEngine(cfg);
        }

        /// <summary>从配置中构建引擎对象</summary>
        private static ScheduleEngine CreateEngineFromConfig()
        {
            var folder = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;
            string configFile = string.Format("{0}\\scheduler.config", folder);
            return new ScheduleEngine(configFile);
        }


    }
}
