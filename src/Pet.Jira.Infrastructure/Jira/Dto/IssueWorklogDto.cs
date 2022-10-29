using Pet.Jira.Application.Time;
using Pet.Jira.Domain.Models.Worklogs;
using System;

namespace Pet.Jira.Infrastructure.Jira.Dto
{
    public class IssueWorklogDto
    {
        public DateTime? StartDate { get; set; }
        public long TimeSpentInSeconds { get; set; }
        public IssueDto Issue { get; set; }

        public TimeSpan TimeSpent => TimeSpan.FromSeconds(TimeSpentInSeconds);
        public DateTime? EndDate => StartDate != null ? StartDate.Value.AddSeconds(TimeSpentInSeconds) : default;
        
        public T Adapt<T>(ITimeProvider timeProvider, TimeZoneInfo userTimeZone)
            where T: IWorklog, new()
        {
            return new T
            {
                StartDate = timeProvider.ConvertToUserTimezone(StartDate.Value, userTimeZone),
                TimeSpent = TimeSpent,
                CompleteDate = timeProvider.ConvertToUserTimezone(EndDate.Value, userTimeZone),
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
