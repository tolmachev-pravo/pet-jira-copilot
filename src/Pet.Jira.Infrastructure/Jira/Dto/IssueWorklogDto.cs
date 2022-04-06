using System;
using Pet.Jira.Domain.Models.Worklogs;

namespace Pet.Jira.Infrastructure.Jira.Dto
{
    public class IssueWorklogDto
    {
        public DateTime? StartDate { get; set; }
        public long TimeSpentInSeconds { get; set; }
        public IssueDto Issue { get; set; }

        public TimeSpan TimeSpent => TimeSpan.FromSeconds(TimeSpentInSeconds);
        public DateTime? EndDate => StartDate != null ? StartDate.Value.AddSeconds(TimeSpentInSeconds) : default;
        
        public T Adapt<T>()
            where T: IWorklog, new()
        {
            return new T
            {
                StartedAt = StartDate.Value,
                ElapsedTime = TimeSpent,
                CompletedAt = EndDate.Value,
                Issue = Issue.Adapt()
            };
        }

        public static IssueWorklogDto Create(
            Atlassian.Jira.Worklog worklog,
            IssueDto issue)
        {
            return new IssueWorklogDto
            {
                StartDate = worklog.StartDate,
                TimeSpentInSeconds = worklog.TimeSpentInSeconds,
                Issue = issue
            };
        }
    }
}
