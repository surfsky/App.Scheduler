using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Web;

namespace App.Components
{
    /// <summary>
    /// 一些常用的扩展
    /// </summary>
    internal static class Extensions
    {
        /// <summary>字符串是否为空</summary>
        public static bool IsNullOrEmpty(this string txt)
        {
            return String.IsNullOrEmpty(txt);
        }

        /// <summary>对象是否为空或为空字符串</summary>
        public static bool IsNullOrEmpty(this object o)
        {
            return (o == null) ? true : o.ToString().IsNullOrEmpty();
        }

        /// <summary>将可空对象转化为字符串</summary>
        public static string ToText(this object o, string format="{0}")
        {
            //return o == null ? "" : o.ToString();
            return string.Format(format, o);
        }

        /// <summary>将可空bool对象转化为字符串</summary>
        public static string ToText(this bool? o, string trueText = "true", string falseText = "false")
        {
            return o == null
                ? ""
                : (o.Value ? trueText : falseText)
                ;
        }

        /// <summary>将可空对象转化为整型字符串</summary>
        public static string ToIntText(this object o)
        {
            if (o == null) return "";
            else return Convert.ToInt32(o).ToString();
        }

        /// <summary>将可空对象转化为整型</summary>
        public static int ToInt32(this object o)
        {
            if (o == null) return -1;
            else return Convert.ToInt32(o);
        }

        /// <summary>将可空对象转化为时间类型</summary>
        public static DateTime ToDateTime(this object o)
        {
            return o == null ? new DateTime() : DateTime.Parse(o.ToString());
        }

        /// <summary>转化为逗号分隔的字符串</summary>
        public static string ToCommaString(this IEnumerable source)
        {
            string txt = "";
            foreach (var item in source)
                txt += item.ToString() + ",";
            return txt.TrimEnd(',');
        }

        /// <summary>转化为整型列表</summary>
        public static List<int> CastInt(this IEnumerable source)
        {
            var result = new List<int>();
            foreach (var item in source)
            {
                result.Add(int.Parse(item.ToString()));
            }
            return result;
        }

        /// <summary>转化为整型列表</summary>
        public static List<string> CastString(this IEnumerable source)
        {
            var result = new List<string>();
            foreach (var item in source)
            {
                result.Add(item.ToString());
            }
            return result;
        }

        /// <summary>转化为枚举列表</summary>
        public static List<T> CastEnum<T>(this IEnumerable source) where T : struct
        {
            var result = new List<T>();
            foreach (var item in source)
            {
                object o = Enum.ToObject(typeof(T), Convert.ToInt32(item));
                result.Add((T)o);
            }
            return result;
        }

    }
}