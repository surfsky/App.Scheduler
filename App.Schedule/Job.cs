using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Newtonsoft.Json;

namespace App.Schedule
{
    /// <summary>
    /// 任务参数
    /// </summary>
    public struct JobParameter
    {
        public DateTime Dt { get; set; }
        public string Data { get; set; }
    }

    /// <summary>
    /// 任务运行器接口
    /// </summary>
    public interface IJobRunner
    {
        bool Run(DateTime dt, string data);
    }

    /// <summary>
    /// 任务状态
    /// </summary>
    [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public enum JobStatus
    {
        Waiting,
        Success,
        Failure
    }

    /// <summary>
    /// 任务基类
    /// 参照 Quartz cron 表达式: https://yq.aliyun.com/articles/62723#_Toc465868115
    /// - 顺序调整为：年 月 日 时 分 周
    /// - 每个部分可用逗号分隔
    /// - 只保留 * 符号
    /// </summary>
    public class Job : IJobRunner
    {
        /// <summary>名称</summary>
        public string Name { get; set; }

        /// <summary>是否有效</summary>
        public bool Enable { get; set; } = true;

        /// <summary>调度表达式。格式为"年 月 日 时 分 周"。如"* * * 8,18, * *"表示每日8点、18点运行</summary>
        public Schedule Schedule { get; set; }

        /// <summary>任务完成状态</summary>
        public JobStatus Status { get; set; } = JobStatus.Success;

        /// <summary>最后运行时间</summary>
        public DateTime LastRunDt { get; set; }

        /// <summary>上次成功运行后，再次运行需要的时间间隔</summary>
        public DateSpan Success { get; set; }

        /// <summary>上次运行失败后，再次运行需要的时间间隔</summary>
        public DateSpan Failure { get; set; } = new DateSpan(0, 0, 0, 0, 5, 0, 0, 9);

        /// <summary>依赖的前置任务（依赖任务全部完成后，本任务才运行）</summary>
        public List<Job> Dependency { get; set; } = new List<Job>();


        //-------------------------------------------
        /// <summary>运行器的类型(ITaskRunner)</summary>
        public Type Runner { get; set; }

        /// <summary>运行参数</summary>
        public string Data { get; set; }



        //-------------------------------------------
        // 构造函数
        //-------------------------------------------
        public Job()
        {
            this.Runner = this.GetType();
        }
        public Job(string name, string schedule, DateSpan interval, DateSpan failure=null)
        {
            this.Name = name;
            this.Schedule = new Schedule(schedule);
            this.Success = interval;
            this.Failure = failure ?? new DateSpan(0, 0, 0, 0, 0, 0, 0, 0);
            this.Runner = this.GetType();
        }


        //-------------------------------------------
        // 方法
        //-------------------------------------------
        /// <summary>真正的任务运行逻辑，供子类实现。IRunner接口成员</summary>
        public virtual bool Run(DateTime dt, string data)
        {
            return true;
        }

        // 获取ITaskRunner对象
        private IJobRunner _runner;
        [JsonIgnore] public IJobRunner JobRunner
        {
            get
            {
                if (_runner == null)
                {
                    var assembly = Assembly.GetAssembly(this.Runner);
                    _runner = assembly.CreateInstance(this.Runner.FullName) as IJobRunner;
                }
                return _runner;
            }
        }

    }
}
