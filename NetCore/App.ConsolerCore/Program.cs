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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;

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

            // Log
            LogManager.LoadConfiguration("nlog.config");
            NLog.Logger logger = LogManager.GetCurrentClassLogger();
            logger.Info("Logger start");

            // Engine
            //engine = CreateEngineFromConfig();
            engine = CreateEngine();
            engine.ConfigFailure += (info) => { logger.Error("{0}", info); Console.ReadKey(); };
            engine.JobRunning += (job, info, _) =>
            {
                JobContext.Current["name"] = "hello";
                logger.Info("{0} {1}", job.Name, job.Data);
            };
            engine.JobFinish += (job, info, success) =>
            {
                if (success)
                    logger.Info(@"{0} {1} √", job.Name, job.Data);
                else
                    logger.Warn(@"{0} {1} ×, times={2}, info={3}", job.Name, job.Data, job.Failure.TryTimes, info);
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
                 Data = "http://www.baidu.com",
                 Runner = typeof(ConnectJob),
                 Name = "Connect",
                 Success = new DateSpan(0, 0, 0, 0, 0, 30),
                 Failure = new DateSpan(0, 0, 0, 0, 0, 0, 0, 9),
                 Schedule = new Schedule("* * * * * *")
            });
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
