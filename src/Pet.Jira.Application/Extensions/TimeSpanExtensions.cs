using System;
using System.Collections.Generic;
using System.Linq;

namespace Pet.Jira.Application.Extensions
{
    public static class TimeSpanExtensions
    {
        public static TimeSpan Sum(this IEnumerable<TimeSpan> timeSpans)
        {
            if (timeSpans.IsEmpty())
            {
                return TimeSpan.Zero;
            }

            return timeSpans.Aggregate((time1, time2) => time1 + time2);
        }
    }
}
