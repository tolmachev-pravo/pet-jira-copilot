using System;
using System.Collections.Generic;
using System.Linq;

namespace Pet.Jira.Adapter
{
    public class DayUserWorklog
    {
        public DateTime Date { get; set; }
        public List<ActualWorklog> ActualWorklogs { get; set; }
        public List<EstimatedWorklog> EstimatedWorklogs { get; set; }

        public TimeSpan ActualWorklogsSum => new TimeSpan(ActualWorklogs.Sum(item => item.TimeSpent.Ticks));
        public TimeSpan EstimatedWorklogsSum => new TimeSpan(EstimatedWorklogs.Sum(item => item.TimeSpent.Ticks));
    }
}
