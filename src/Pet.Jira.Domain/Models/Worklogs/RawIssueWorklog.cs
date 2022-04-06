using Pet.Jira.Domain.Models.Issues;
using System;

namespace Pet.Jira.Domain.Models.Worklogs
{
    public class RawIssueWorklog : IWorklog
    {
        public DateTime StartedAt { get; set; }
        public DateTime CompletedAt { get; set; }

        public TimeSpan ElapsedTime
        {
            get => CompletedAt - StartedAt;
            set { }
        }

        public IIssue Issue { get; set; }

        public bool IsBetween(DateTime from, DateTime to)
        {
            return StartedAt < to && CompletedAt > from;
        }
    }
}
