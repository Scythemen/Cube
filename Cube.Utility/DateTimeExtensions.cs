using System;

namespace Cube.Utility
{
    public static class DateTimeExtensions
    {
        public static DateTime ParseUnixTimeSeconds(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }

        /// <summary>
        /// 获取时间范围内，所在星期的起始结束时间，周一为星期的第一天
        /// </summary>
        /// <param name="fromTime">Null 则取当前时间</param>
        /// <param name="toTime">Null 则取当前时间</param>
        /// <returns></returns>
        public static (DateTime from, DateTime to) GetWeekDayRange(string fromTime = null, string toTime = null)
        {
            DateTime from = DateTime.Now;
            DateTime to = DateTime.Now;
            if (string.IsNullOrWhiteSpace(fromTime) && string.IsNullOrWhiteSpace(toTime))
            {
                from = GetWeekMonday();
                to = from.AddDays(7).AddSeconds(-1);
            }
            else if (!string.IsNullOrWhiteSpace(fromTime)
                     && !string.IsNullOrWhiteSpace(toTime)
                     && DateTime.TryParse(fromTime, out DateTime ft)
                     && DateTime.TryParse(toTime, out DateTime tt))
            {
                from = GetWeekMonday(fromTime);
                to = GetWeekSunday(toTime).Date.AddHours(23).AddMinutes(59).AddSeconds(59);
            }
            else
            {
                fromTime = fromTime ?? toTime;
                from = GetWeekMonday(fromTime);
                to = from.AddDays(7).AddSeconds(-1);
            }

            return (from, to);
        }


        /// <summary>
        ///  获取日期所在星期的周一日期，周一为星期的第一天
        /// </summary>
        /// <param name="date">Null 则取当天</param>
        /// <returns></returns>
        public static DateTime GetWeekMonday(string date = null)
        {
            DateTime from = DateTime.Now.Date;
            if (!string.IsNullOrWhiteSpace(date) && DateTime.TryParse(date, out DateTime dt))
            {
                from = dt.Date;
            }

            return GetWeekMonday(from);
        }

        /// <summary>
        ///  获取日期所在星期的周一日期，周一为星期的第一天
        /// </summary>
        /// <param name="date"> </param>
        /// <returns></returns>
        public static DateTime GetWeekMonday(DateTime date)
        {
            var start = date.Date;
            for (int i = 0; i < 7; i++)
            {
                start = date.AddDays(-i);
                if (start.DayOfWeek == DayOfWeek.Monday)
                {
                    break;
                }
            }

            return start;
        }


        /// <summary>
        /// 获取日期所在星期的周日日期，周一为星期的第一天
        /// </summary>
        /// <param name="date">Null 则取当天</param>
        /// <returns></returns>
        public static DateTime GetWeekSunday(string date = null)
        {
            DateTime from = DateTime.Now.Date;
            if (!string.IsNullOrWhiteSpace(date) && DateTime.TryParse(date, out DateTime dt))
            {
                from = dt.Date;
            }

            return GetWeekSunday(from);
        }

        /// <summary>
        /// 获取日期所在星期的周日日期，周一为星期的第一天
        /// </summary>
        /// <param name="date"> </param>
        /// <returns></returns>
        public static DateTime GetWeekSunday(DateTime date)
        {
            var end = date.Date;
            for (int i = 0; i < 7; i++)
            {
                end = date.AddDays(i);
                if (end.DayOfWeek == DayOfWeek.Sunday)
                {
                    break;
                }
            }

            return end;
        }
    }
}