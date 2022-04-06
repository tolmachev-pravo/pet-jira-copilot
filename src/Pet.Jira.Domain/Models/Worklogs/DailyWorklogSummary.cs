using System;
using System.Collections.Generic;
using System.Linq;

namespace Pet.Jira.Domain.Models.Worklogs
{
    public class DailyWorklogSummary
    {
        public DateTime Date { get; set; }
        public List<ActualWorklog> ActualWorklogs { get; set; }
        public List<EstimatedWorklog> EstimatedWorklogs { get; set; }

        public TimeSpan ActualWorklogsSum => new TimeSpan(ActualWorklogs.Sum(item => item.ElapsedTime.Ticks));
        public TimeSpan EstimatedWorklogsSum => new TimeSpan(EstimatedWorklogs.Sum(item => item.ElapsedTime.Ticks));
        public TimeSpan CalculatedWorklogsSum => ActualWorklogsSum + new TimeSpan(EstimatedWorklogs.Sum(item => item.RestTime.Ticks));
    }
}
