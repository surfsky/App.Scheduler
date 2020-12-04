using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using App.Components;

namespace App.Scheduler
{
    /// <summary>
    /// 调度表达式：年 月 日 时 分 秒 周
    /// </summary>
    [JsonConverter(typeof(ScheduleConverter))]
    public class Schedule
    {
        string _expression;

        // 年月日时分秒周
        [JsonIgnore] public List<int> Years   { get; set; } = new List<int>();
        [JsonIgnore] public List<int> Months  { get; set; } = new List<int>();
        [JsonIgnore] public List<int> Days    { get; set; } = new List<int>();
        [JsonIgnore] public List<int> Hours   { get; set; } = new List<int>();
        [JsonIgnore] public List<int> Minutes { get; set; } = new List<int>();
        [JsonIgnore] public List<int> Seconds { get; set; } = new List<int>();
        [JsonIgnore] public List<DayOfWeek> WeekDays { get; set; } = new List<DayOfWeek>();


        /// <summary>构建调度表达式</summary>
        public Schedule() { }

        /// <summary>构建调度表达式</summary>
        /// <param name="expression">格式如：年 月 日 时 分 秒 周</param>
        /// <example>
        /// * * 1 0 0 0 *       表示每月 1 日 0 点 0 分启动任务
        /// * * * 0,12 0 0 *    表示每天 0 点和 12 点启动任务
        /// * * * 0 0 0 1       表示每周一 0 点启动任务
        /// </example>
        public Schedule(string expression)
        {
            if (expression.IsEmpty())
                return;
            var parts = expression.Trim().Split(' ');
            if (parts.Length != 7)
                return;

            this._expression = expression;
            this.Years    = parts[0] == "*" ? new List<int>() : parts[0].Split(',').CastInt();
            this.Months   = parts[1] == "*" ? new List<int>() : parts[1].Split(',').CastInt();
            this.Days     = parts[2] == "*" ? new List<int>() : parts[2].Split(',').CastInt();
            this.Hours    = parts[3] == "*" ? new List<int>() : parts[3].Split(',').CastInt();
            this.Minutes  = parts[4] == "*" ? new List<int>() : parts[4].Split(',').CastInt();
            this.Seconds  = parts[5] == "*" ? new List<int>() : parts[5].Split(',').CastInt();
            this.WeekDays = parts[6] == "*" ? new List<DayOfWeek>() : parts[6].Split(',').CastEnum<DayOfWeek>();
        }

        /// <summary>构建调度表达式</summary>
        Schedule(
            List<int> years = null, List<int> months = null, List<int> days = null,
            List<int> hours = null, List<int> minutes = null, List<int> seconds = null, 
            List<DayOfWeek> weekdays=null)
        {
            this.Years = years;
            this.Months = months;
            this.Days = days;
            this.Hours = hours;
            this.Minutes = minutes;
            this.Seconds = seconds;
            this.WeekDays = weekdays;
        }

        /// <summary>转化为字符串</summary>
        public override string ToString()
        {
            return this._expression;
        }

        /// <summary>是否处于运行时间</summary>
        public bool InTime(DateTime dt)
        {
            if (this.Years.IsEmpty() || this.Years.Contains(dt.Year))
                if (this.Months.IsEmpty() || this.Months.Contains(dt.Month))
                    if (this.Days.IsEmpty() || this.Days.Contains(dt.Day))
                        if (this.Hours.IsEmpty() || this.Hours.Contains(dt.Hour))
                            if (this.Minutes.IsEmpty() || this.Minutes.Contains(dt.Minute))
                                if (this.Seconds.IsEmpty() || this.Seconds.Contains(dt.Second))
                                    if (this.WeekDays.IsEmpty() || this.WeekDays.Contains(dt.DayOfWeek))
                                    return true;
            return false;
        }
    }


    /// <summary>
    /// 调度表达式格式化转化器
    /// </summary>
    internal class ScheduleConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Schedule);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return new Schedule(reader.Value.ToString());
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(((Schedule)value).ToString());
        }
    }

}
