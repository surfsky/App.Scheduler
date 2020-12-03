using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using App.Components;

namespace App.Scheduler
{
    /// <summary>任务事件代理</summary>
    /// <param name="job">任务对象</param>
    /// <param name="info">附加数据</param>
    public delegate void JobDelegate(Job job, string info, bool success);

    /// <summary>
    /// Schedule engine
    /// </summary>
    public class ScheduleEngine
    {
        // 私有
        private bool _running = true;
        private string _configFile;

        // 属性
        public static string Version
        {
            get
            {
                var assembly = Assembly.GetAssembly(typeof(ScheduleEngine));
                return assembly.GetName().Version.ToString();
            }
        }

        /// <summary>Schedule config</summary>
        public ScheduleConfig Config { get; set; }

        /// <summary>The interval seconds for saveing config into file</summary>
        public int SaveSeconds { get; set; } = 60;

        // 事件
        /// <summary>Load config fail event</summary>
        public event Action<string> ConfigFailure;
        
        /// <summary>Config save event</summary>
        public event Action<ScheduleConfig> ConfigSave;

        /// <summary>Job starts event</summary>
        public event JobDelegate JobStart;

        /// <summary>Job running event</summary>
        public event JobDelegate JobRunning;

        /// <summary>Job end event</summary>
        public event JobDelegate JobFinish;

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
            foreach (var job in Config.Jobs)
            {
                if (job.Schedule == null || job.JobRunner == null)
                    return false;
            }
            return true;
        }

        // 无限循环
        void Loop()
        {
            while (_running)
            {
                // 遍历任务并处理
                var now = DateTime.Now;
                foreach (var job in Config.Jobs)
                {
                    new Thread(() => {
                        Process(job, now);
                        JobContext.RemoveCurrent();
                    }).Start();
                }

                // 每隔一段时间才写入一次磁盘，避免对磁盘造成压力
                if (Common.GetSeconds(Config.LogDt, now) >= this.SaveSeconds)
                {
                    Config.LogDt = now;
                    Config.Save();
                    ConfigSave?.Invoke(this.Config);
                }
                Thread.Sleep(Config.Sleep);
            }
        }


        /// <summary>运行任务</summary>
        protected async Task<bool> Process(Job job, DateTime dt)
        {
            // 当它不存在
            if (!job.Enable)
                return true;

            // 是否处于运行时间内
            if (!job.Schedule.InTime(dt))
                return false;

            // 成功状态，且未到下次运行时间
            if (job.Status == JobStatus.Success && !NeedToRunAgain(job.LastRunDt, job.Success, dt))
                return true;

            // 失败状态，但又要开始一个周期了
            if (job.Status == JobStatus.Failure && NeedToRunAgain(job.LastRunDt, job.Success, dt))
            {
                job.Failure.TryTimes = 0;
                job.Status = JobStatus.Waiting;
            }

            // 处于等待状态；或与上次运行成功的间隔时间足够了；或上次运行失败的间隔时间足够了（且失败次数未饱和）；
            if (
                (job.Status == JobStatus.Waiting && NeedToRunAgain(job.LastRunDt, job.Success, dt))
             || (job.Status == JobStatus.Success && NeedToRunAgain(job.LastRunDt, job.Success, dt))
             || (job.Status == JobStatus.Failure && NeedToRunAgain(job.LastRunDt, job.Failure, dt) && job.Failure.TryTimes < job.Failure.MaxTimes)
            )
            {
                // 开始任务
                JobStart?.Invoke(job, "", true);

                // 先运行依赖任务（依赖任务要依次运行）
                if (job.Dependency != null && job.Dependency.Count > 0)
                {
                    foreach (var t in job.Dependency)
                    {
                        var b = await Process(t, dt);
                        if (!b)
                            return false;
                    }
                }

                // 真正运行本任务
                job.LastRunDt = dt;
                job.Status = JobStatus.Running;

                // 启动任务
                RunJob(job);
                return (job.Status == JobStatus.Success);
            }

            return false;
        }

        /// <summary>运行任务</summary>
        private void RunJob(Job job)
        {
            JobRunning?.Invoke(job, "", true);
            try
            {
                var runner = job.JobRunner;
                if (runner.Run(job.LastRunDt, job.Data))
                {
                    job.Status = JobStatus.Success;
                    job.Failure.TryTimes = 0;
                    JobFinish?.Invoke(job, "", true);
                }
                else
                {
                    job.Status = JobStatus.Failure;
                    job.Failure.TryTimes++;
                    JobFinish?.Invoke(job, "", false);
                }
            }
            catch (Exception ex)
            {
                job.Status = JobStatus.Failure;
                job.Failure.TryTimes++;
                JobFinish?.Invoke(job, ex.Message, false);
            }
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
