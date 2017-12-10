using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using App.Components;

namespace App.Schedule
{
    /// <summary>
    /// 时间差表达式（弃用TimeSpan是因为只能设置天数，且不可以xml序列化）
    /// </summary>
    [JsonConverter(typeof(ScheduleConverter))]
    public class Schedule
    {
        // 年月日时分秒周
        [JsonIgnore] public List<int> Years { get; set; } = new List<int>();
        [JsonIgnore] public List<int> Months { get; set; } = new List<int>();
        [JsonIgnore] public List<int> Days { get; set; } = new List<int>();
        [JsonIgnore] public List<int> Hours { get; set; } = new List<int>();
        [JsonIgnore] public List<int> Minutes { get; set; } = new List<int>();
        [JsonIgnore] public List<DayOfWeek> WeekDays { get; set; } = new List<DayOfWeek>();

        string _expression;

        //
        public Schedule() { }
        public Schedule(string expression)
        {
            if (expression.IsNullOrEmpty())
                return;
            var parts = expression.Trim().Split(' ');
            if (parts.Length != 6)
                return;

            this._expression = expression;
            this.Years    = parts[0] == "*" ? new List<int>() : parts[0].Split(',').CastInt();
            this.Months   = parts[1] == "*" ? new List<int>() : parts[1].Split(',').CastInt();
            this.Days     = parts[2] == "*" ? new List<int>() : parts[2].Split(',').CastInt();
            this.Hours    = parts[3] == "*" ? new List<int>() : parts[3].Split(',').CastInt();
            this.Minutes  = parts[4] == "*" ? new List<int>() : parts[4].Split(',').CastInt();
            this.WeekDays = parts[5] == "*" ? new List<DayOfWeek>() : parts[5].Split(',').CastEnum<DayOfWeek>();
        }

        // 转化为字符串
        public override string ToString()
        {
            return this._expression;
        }

        /// <summary>是否处于运行时间</summary>
        public bool InTime(DateTime dt)
        {
            if (this.Years.Count == 0 || this.Years.Contains(dt.Year))
                if (this.Months.Count == 0 || this.Months.Contains(dt.Month))
                    if (this.Days.Count == 0 || this.Days.Contains(dt.Day))
                        if (this.Hours.Count == 0 || this.Hours.Contains(dt.Hour))
                            if (this.Minutes.Count == 0 || this.Minutes.Contains(dt.Minute))
                                if (this.WeekDays.Count == 0 || this.WeekDays.Contains(dt.DayOfWeek))
                                    return true;
            return false;
        }
    }


    /// <summary>
    /// DateSpan 格式化转化器
    /// </summary>
    public class ScheduleConverter : JsonConverter
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
