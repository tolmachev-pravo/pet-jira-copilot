using System;
using System.Collections.Generic;
using System.Linq;

namespace Pet.Jira.Application.Extensions
{
    public static class TimeSpanExtensions
    {
        public static TimeSpan Sum(this IEnumerable<TimeSpan> timeSpans)
        {
            return timeSpans?.Aggregate((time1, time2) => time1 + time2) ?? TimeSpan.Zero;
        }
    }
}
