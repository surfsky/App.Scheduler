using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Schedule
{
    /// <summary>
    /// 公共方法类
    /// </summary>
    internal class Common
    {
        // 获取两个时间的差值（秒）
        public static long GetSeconds(DateTime startDt, DateTime endDt)
        {
            return (endDt.Ticks - startDt.Ticks) / 10_000_000;
        }

    }
}
