using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace App.Components
{
    /// <summary>
    /// HTTP 操作相关（GET/POST/...)
    /// </summary>
    internal static class HttpHelper
    {
        /// <summary>获取Http响应文本</summary>
        public static string GetResponseText(HttpWebResponse response)
        {
            string encoding = response.ContentEncoding;
            if (encoding == null || encoding.Length < 1)
                encoding = "UTF-8";
            var reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(encoding));
            return reader.ReadToEnd();
        }

        //---------------------------------------------------------
        // Get 方法
        //---------------------------------------------------------
        /// <summary>Get</summary>
        public static string Get(string url, CookieContainer cookieContainer = null)
        {
            // 请求
            cookieContainer = cookieContainer ?? new CookieContainer();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.CookieContainer = cookieContainer;

            // 返回
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            response.Cookies = cookieContainer.GetCookies(response.ResponseUri);
            return GetResponseText(response);
        }

    }
}