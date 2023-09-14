using Pet.Jira.Domain.Models.Worklogs;
using System;

namespace Pet.Jira.Application.Worklogs.Dto
{
    public class AddedWorklogDto
    {
        public DateTime StartedAt { get; set; }
        public TimeSpan ElapsedTime { get; set; }
        public string IssueKey { get; set; }
        public string Comment { get; set; }

        public static AddedWorklogDto Create(WorkingDayWorklog worklog)
        {
            return new AddedWorklogDto
            {
                StartedAt = worklog.StartDate,
                IssueKey = worklog.Issue.Key,
                ElapsedTime = worklog.RemainingTimeSpent,
                Comment = WorklogComment(worklog)
            };
        }

        private static string WorklogComment(WorkingDayWorklog worklog)
        {
            if (!string.IsNullOrEmpty(worklog.Comment))
            {
                return worklog.Comment;
            }

            switch (worklog.Source)
            {
                case WorklogSource.Assignee:
                    return $"Working on task {worklog.Issue?.Key}";
                case WorklogSource.Comment:
                    return $"Task discussion {worklog.Issue?.Key}";
                case WorklogSource.Calendar:
                    return $"Discussion {worklog.Issue?.Key}";
                default:
                    return "Default worklog";
            }
        }
    }
}
