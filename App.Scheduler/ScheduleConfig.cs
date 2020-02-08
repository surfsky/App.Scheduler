using System;
using System.Collections.Generic;
using System.IO;
using App.Components;

namespace App.Scheduler
{
    /// <summary>
    /// 调度的配置信息
    /// </summary>
    public class ScheduleConfig
    {
        /// <summary>每次循环休眠毫秒数</summary>
        public int Sleep { get; set; }
        /// <summary>最后日志时间</summary>
        public DateTime LogDt { get; set; }
        /// <summary>任务</summary>
        public List<Job> Jobs { get; set; }

        //
        string _configFile;

        // 构造
        public ScheduleConfig() { }
        public ScheduleConfig(string configFile)
        {
            _configFile = configFile;
            var cfg = Load(configFile);
            this.Sleep = cfg.Sleep;
            this.LogDt = cfg.LogDt;
            this.Jobs = cfg.Jobs;
        }

        // 加载
        static ScheduleConfig Load(string filePath)
        {
            if (!File.Exists(filePath))
                CreateDemo().Save();
            return SerializeHelper.LoadJson(filePath, typeof(ScheduleConfig)) as ScheduleConfig;
        }

        // 保存
        public void Save()
        {
            if (!string.IsNullOrEmpty(_configFile))
               SerializeHelper.SaveJson(_configFile, this);
        }

        // 创建示例数据
        static ScheduleConfig CreateDemo()
        {
            ScheduleConfig cfg = new ScheduleConfig();
            cfg.Sleep = 400;
            cfg.LogDt = DateTime.Now;
            cfg.Jobs = new List<Job>
            {
                new Job()
            };
            return cfg;
        }
    }
}
