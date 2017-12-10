using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using App.Components;

namespace App.Schedule
{
    public delegate void TaskDelegate(Job task, string info);

    /// <summary>
    /// 任务调度引擎
    /// </summary>
    public class ScheduleEngine
    {
        // 私有
        private bool _running = true;
        private string _configFile;

        // 属性
        public string Version { get { return ReflectionHelper.AssemblyVersion.ToString(); } }
        public ScheduleConfig Config { get; set; }
        public int SaveSeconds { get; set; } = 60;

        // 事件
        public event Action<string> ConfigFailure;
        public event TaskDelegate TaskRunning;
        public event TaskDelegate TaskSuccess;
        public event TaskDelegate TaskFailure;

        //----------------------------------------
        // 构造函数
        //----------------------------------------
        public ScheduleEngine(ScheduleConfig config)
        {
            this.Config = config;
        }

        public ScheduleEngine(string configFile)
        {
            _configFile = configFile;
        }

        //----------------------------------------
        // 运行控制
        //----------------------------------------
        /// <summary>停止</summary>
        public void Stop()
        {
            this._running = false;
            this.Config?.Save();
        }

        /// <summary>运行</summary>
        public void Start()
        {
            // 读取配置文件
            if (!_configFile.IsNullOrEmpty())
            {
                try
                {
                    this.Config = new ScheduleConfig(_configFile);
                }
                catch (Exception ex)
                {
                    ConfigFailure?.Invoke("Config file load fail : " + ex.Message);
                    return;
                }
            }

            // 检测配置是否有效
            if (!CheckConfig())
            {
                ConfigFailure?.Invoke("Config check fail");
                return;
            }

            // 用线程开启循环（避免堵塞主进程）
            new Thread(new ThreadStart(Loop)).Start();
        }

        // 检测配置是否正确
        private bool CheckConfig()
        {
            if (Config == null || Config.Jobs == null)
                return false;
            foreach (var task in Config.Jobs)
            {
                if (task.Schedule == null || task.JobRunner == null)
                    return false;
            }
            return true;
        }

        // 无限循环
        protected void Loop()
        {
            while (_running)
            {
                // 遍历任务并处理
                var now = DateTime.Now;
                foreach (var task in Config.Jobs)
                    ProcessTask(task, now);

                // 每隔一段时间才写入一次磁盘，避免对磁盘造成压力
                if (Common.GetSeconds(Config.LogDt, now) >= this.SaveSeconds)
                {
                    Config.LogDt = now;
                    Config.Save();
                }
                Thread.Sleep(Config.Sleep);
            }
        }


        /// <summary>运行任务</summary>
        protected bool ProcessTask(Job task, DateTime dt)
        {
            // 当它不存在
            if (!task.Enable)
                return true;

            // 是否处于运行时间内
            if (!task.Schedule.InTime(dt))
                return false;

            // 成功状态，且未到下次运行时间
            if (task.Status == JobStatus.Success && !NeedToRunAgain(task.LastRunDt, task.Success, dt))
                return true;

            // 失败状态，但又要开始一个周期了
            if (task.Status == JobStatus.Failure && NeedToRunAgain(task.LastRunDt, task.Success, dt))
            {
                task.Failure.TryTimes = 0;
                task.Status = JobStatus.Waiting;
            }

            // 处于等待状态；或与上次运行成功的间隔时间足够了；或上次运行失败的间隔时间足够了（且失败次数未饱和）；
            if (
                (task.Status == JobStatus.Waiting && NeedToRunAgain(task.LastRunDt, task.Success, dt))
             || (task.Status == JobStatus.Success && NeedToRunAgain(task.LastRunDt, task.Success, dt))
             || (task.Status == JobStatus.Failure && NeedToRunAgain(task.LastRunDt, task.Failure, dt) && task.Failure.TryTimes < task.Failure.MaxTimes)
            )
            {
                TaskRunning?.Invoke(task, "");

                // 先运行依赖任务
                if (task.Dependency != null && task.Dependency.Count > 0)
                {
                    foreach (var t in task.Dependency)
                    {
                        if (!ProcessTask(t, dt))
                            return false;
                    }
                }

                // 再运行本任务
                task.LastRunDt = dt;
                task.Status = JobStatus.Waiting;
                try
                {
                    var runner = task.JobRunner;
                    
                    // TODO: 改为异步任务或者线程
                    if (runner.Run(dt, task.Data))
                    {
                        task.Status = JobStatus.Success;
                        task.Failure.TryTimes = 0;
                        TaskSuccess?.Invoke(task, "");
                        return true;
                    }
                    else
                    {
                        task.Status = JobStatus.Failure;
                        task.Failure.TryTimes++;
                        TaskFailure?.Invoke(task, "");
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    task.Status = JobStatus.Failure;
                    task.Failure.TryTimes++;
                    TaskFailure?.Invoke(task, ex.Message);
                }
            }

            return false;
        }


        // 新任务时间到了么
        protected bool NeedToRunAgain(DateTime lastDt, DateSpan span, DateTime now)
        {
            if (span == null) return true;
            var dt2 = lastDt
                .AddYears(span.Year)
                .AddMonths(span.Month)
                .AddDays(span.Day)
                .AddHours(span.Hour)
                .AddMinutes(span.Minute)
                .AddSeconds(span.Second)
                ;
            return now >= dt2;
        }

    }
}
