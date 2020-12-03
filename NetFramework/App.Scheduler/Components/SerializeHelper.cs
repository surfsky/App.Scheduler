using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace App.Components
{
    /// <summary>
    /// 序列化辅助方法
    /// </summary>
    internal class SerializeHelper
    {
        //------------------------------------------------
        // XML
        //------------------------------------------------
        /// <summary>OBJECT -> XML</summary>
        public static void SaveXml(string filePath, object obj)
        {
            Type type = obj.GetType();
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                XmlSerializer xs = new XmlSerializer(type);
                xs.Serialize(writer, obj);
                writer.Close();
            }
        }

        /// <summary>XML -> OBJECT</summary>
        public static object LoadXml(string filePath, Type type)
        {
            if (!File.Exists(filePath))
                return null;
            using (StreamReader reader = new StreamReader(filePath))
            {
                XmlSerializer xs = new XmlSerializer(type);
                object obj = xs.Deserialize(reader);
                reader.Close();
                return obj;
            }
        }


        //------------------------------------------------
        // JSON
        //------------------------------------------------
        /// <summary>OBJECT -> JSON</summary>
        public static void SaveJson(string filePath, object obj, JsonSerializerSettings settings=null)
        {
            Type type = obj.GetType();
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                settings = settings ?? GetDefaultJsonSettings();
                string txt = JsonConvert.SerializeObject(obj, settings);
                writer.Write(txt);
                writer.Close();
            }
        }

        /// <summary>JSON -> OBJECT</summary>
        public static object LoadJson(string filePath, Type type, JsonSerializerSettings settings=null)
        {
            if (!File.Exists(filePath))
                return null;
            using (StreamReader reader = new StreamReader(filePath))
            {
                string txt = reader.ReadToEnd();
                reader.Close();
                settings = settings ?? GetDefaultJsonSettings();
                return JsonConvert.DeserializeObject(txt, type, settings);
            }
        }

        //
        static JsonSerializerSettings GetDefaultJsonSettings()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            //settings.ContractResolver = new CamelCasePropertyNamesContractResolver(); // 属性为小写开头驼峰式
            settings.NullValueHandling = NullValueHandling.Ignore;                    // 忽略null值
            settings.DateFormatHandling = DateFormatHandling.MicrosoftDateFormat;     // 日期格式
            settings.DateFormatString = "yyyy-MM-dd HH:mm:ss";                        // 
            settings.Formatting = Formatting.Indented;                                // 递进
            settings.MaxDepth = 10;                                                   // 设置序列化的最大层数  
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;            // 指定如何处理循环引用
            return settings;
        }
    }
}