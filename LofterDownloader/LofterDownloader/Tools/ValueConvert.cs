using System;
using System.Collections.Generic;
using System.Text;

namespace LofterDownloader.Tools
{
    /// <summary>
    /// 用于数据转换的类
    /// </summary>
    public static class ValueConvert
    {
        /// <summary>
        /// DateTime类型转换为时间戳(毫秒值)
        /// </summary>
        /// <param name="time">时间</param>
        /// <returns>时间戳(毫秒值)</returns>
        public static long DateToTicks(DateTime? time)
        {
            return ((time.HasValue ? time.Value.Ticks : DateTime.Parse("1990-01-01").Ticks) - 621355968000000000) / 10000;
        }

        /// <summary>
        /// 时间戳(毫秒值)String转换为DateTime类型转换
        /// </summary>
        /// <param name="time">时间戳(毫秒值)</param>
        /// <returns>具体时间</returns>
        public static DateTime TicksToDate(string time)
        {
            return new DateTime((Convert.ToInt64(time) * 10000) + 621355968000000000);
        }
    }
}
