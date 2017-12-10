using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System;
using App.Components;

namespace App.Schedule
{
    /// <summary>
    /// 时间差表达式（弃用TimeSpan是因为只能设置天数，且不可以xml序列化）
    /// </summary>
    [JsonConverter(typeof(DateSpanConverter))]
    public class DateSpan
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public int Hour { get; set; }
        public int Minute { get; set; }
        public int Second { get; set; }

        // 尝试次数/最大次数
        public int TryTimes { get; set; }
        public int MaxTimes { get; set; }

        //
        public DateSpan() { }
        public DateSpan(int year, int month, int day, int hour, int minute, int second, int tryTimes = 0, int maxTimes = 99)
        {
            this.Year = year;
            this.Month = month;
            this.Day = day;
            this.Hour = hour;
            this.Minute = minute;
            this.Second = second;
            this.TryTimes = tryTimes;
            this.MaxTimes = maxTimes;
        }
        public DateSpan(string text)
        {
            string exp = @"(?<year>\d{4})-(?<month>\d{2})-(?<day>\d{2}) (?<hour>\d{2}):(?<minute>\d{2}):(?<second>\d{2}) (?<tryTimes>\d{1,3})\/(?<maxTimes>\d{1,3})";
            Regex r = new Regex(exp, RegexOptions.Compiled);
            Match m = r.Match(text);
            if (m.Success)
            {
                this.Year   = m.Result("${year}").ToInt32();
                this.Month  = m.Result("${month}").ToInt32();
                this.Day    = m.Result("${day}").ToInt32();
                this.Hour   = m.Result("${hour}").ToInt32();
                this.Minute = m.Result("${minute}").ToInt32();
                this.Second = m.Result("${second}").ToInt32();
                this.TryTimes = m.Result("${tryTimes}").ToInt32();
                this.MaxTimes = m.Result("${maxTimes}").ToInt32();
            }
        }

        // 转化为字符串
        public override string ToString()
        {
            return string.Format("{0:0000}-{1:00}-{2:00} {3:00}:{4:00}:{5:00} {6}/{7}", Year, Month, Day, Hour, Minute, Second, TryTimes, MaxTimes);
        }
    }


    /// <summary>
    /// DateSpan 格式化转化器
    /// </summary>
    public class DateSpanConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateSpan);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return new DateSpan(reader.Value.ToString());
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(((DateSpan)value).ToString());
        }
    }

}
