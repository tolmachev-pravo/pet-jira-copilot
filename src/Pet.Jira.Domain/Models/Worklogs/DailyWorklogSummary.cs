using System;
using System.Collections.Generic;
using System.Linq;

namespace Pet.Jira.Domain.Models.Worklogs
{
    public class DailyWorklogSummary
    {
        public DateTime Date { get; set; }
        public List<ActualWorklog> ActualWorklogs { get; set; } = new List<ActualWorklog>();
        public List<EstimatedWorklog> EstimatedWorklogs { get; set; } = new List<EstimatedWorklog>();

        public IOrderedEnumerable<IWorklog> Worklogs => EstimatedWorklogs.Cast<IWorklog>().Union(ActualWorklogs.Cast<IWorklog>())
            .OrderBy(record => record.StartedAt);

        public TimeSpan ActualWorklogsSum => new TimeSpan(ActualWorklogs?.Sum(item => item.ElapsedTime.Ticks) ?? 0);
        public TimeSpan EstimatedWorklogsSum => new TimeSpan(EstimatedWorklogs?.Sum(item => item.ElapsedTime.Ticks) ?? 0);

        public TimeSpan CalculatedWorklogsSum =>
            ActualWorklogsSum + new TimeSpan(EstimatedWorklogs?.Sum(item => item.RestTime.Ticks) ?? 0);

        public int Progress => CalculatedWorklogsSum > TimeSpan.Zero 
            ? Convert.ToInt32(ActualWorklogsSum * 100 / CalculatedWorklogsSum)
            : 0;

        public int RawEstimatedWorklogCount => EstimatedWorklogs.Count(item => item.RestTime > TimeSpan.Zero);
        public bool HasRawEstimatedWorklogs => RawEstimatedWorklogCount > 0;
    };
}
