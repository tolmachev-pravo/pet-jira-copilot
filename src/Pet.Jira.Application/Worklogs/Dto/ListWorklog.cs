using Pet.Jira.Domain.Models.Issues;
using Pet.Jira.Domain.Models.Worklogs;
using System;

namespace Pet.Jira.Application.Worklogs.Dto
{
    public class ListWorklog
    {
        public ListWorklogType Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime CompleteDate { get; set; }
        public TimeSpan TimeSpent { get; set; }
        public IIssue Issue { get; set; }

        public static ListWorklog Create(IWorklog worklog, ListWorklogType type)
        {
            return new ListWorklog
            {
                StartDate = worklog.StartedAt,
                CompleteDate = worklog.CompletedAt,
                TimeSpent = worklog.ElapsedTime,
                Issue = worklog.Issue,
                Type = type
            };
        }

        public static ListWorklog Create(EstimatedWorklog worklog, ListWorklogType type)
        {
            return new ListWorklog
            {
                StartDate = worklog.StartedAt,
                CompleteDate = worklog.CompletedAt,
                TimeSpent = worklog.RestTime,
                Issue = worklog.Issue,
                Type = type
            };
        }
    }
}
