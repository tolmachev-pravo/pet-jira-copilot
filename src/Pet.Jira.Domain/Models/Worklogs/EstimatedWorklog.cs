using Pet.Jira.Domain.Models.Issues;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pet.Jira.Domain.Models.Worklogs
{
    public class EstimatedWorklog : IWorklog
    {
        private TimeSpan? _restTimeSpent;

        public DateTime StartedAt { get; set; }
        public DateTime CompletedAt { get; set; }
        public TimeSpan ElapsedTime { get; set; }
        public IIssue Issue { get; set; }
        public List<ActualWorklog> ActualWorklogs { get; set; }

        public TimeSpan RawTimeSpent => TimeSpan.FromSeconds(Math.Round((CompletedAt - StartedAt).TotalSeconds));
        public TimeSpan EstimatedTimeSpent { get; set; }
        public TimeSpan ActualTimeSpent => new TimeSpan(ActualWorklogs.Sum(item => item.ElapsedTime.Ticks));
        public TimeSpan RestTime
        {
            get { return _restTimeSpent ?? TimeSpan.FromSeconds(Math.Round((EstimatedTimeSpent - ActualTimeSpent).TotalSeconds)); }
            set { _restTimeSpent = value; }
        }
    }
}
