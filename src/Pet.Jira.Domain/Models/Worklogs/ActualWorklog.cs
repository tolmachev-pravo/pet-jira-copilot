using Pet.Jira.Domain.Models.Issues;
using System;

namespace Pet.Jira.Domain.Models.Worklogs
{
    public class ActualWorklog : IWorklog
    {
        public DateTime StartedAt { get; set; }
        public DateTime CompletedAt { get; set; }
        public TimeSpan ElapsedTime { get; set; }
        public IIssue Issue { get; set; }

        public static ActualWorklog Create(IWorklog worklog)
        {
            return new ActualWorklog
            {
                CompletedAt = worklog.CompletedAt,
                StartedAt = worklog.StartedAt,
                Issue = worklog.Issue,
                ElapsedTime = worklog.ElapsedTime
            };
        }
    }
}
