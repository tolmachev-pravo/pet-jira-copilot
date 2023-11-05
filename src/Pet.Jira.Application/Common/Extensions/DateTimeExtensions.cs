using System;

namespace Pet.Jira.Application.Common.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime EndOfDay(this DateTime source)
        {
            return source.Date.AddDays(1).AddTicks(-1);
        }

        public static DateTime StartOfDay(this DateTime source)
        {
            return source.Date;
        }
    }
}
