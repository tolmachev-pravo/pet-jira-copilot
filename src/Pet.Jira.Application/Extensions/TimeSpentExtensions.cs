using Pet.Jira.Domain.Models.Worklogs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pet.Jira.Application.Extensions
{
    public static class TimeSpentExtensions
    {
        public static TimeSpan TimeSpent(this IEnumerable<IHasTimeSpent> worklogs)
        {
            return worklogs?
                .Select(worklog => worklog.TimeSpent)
                .Aggregate((time1, time2) => time1 + time2) ?? TimeSpan.Zero;
        }
    }
}
