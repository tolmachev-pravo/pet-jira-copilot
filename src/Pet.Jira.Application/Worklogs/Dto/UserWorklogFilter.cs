using Pet.Jira.Domain.Models.Abstract;
using Pet.Jira.Domain.Models.Issues;
using System;

namespace Pet.Jira.Application.Worklogs.Dto
{
    public class UserWorklogFilter : IEntity<string>
    {
        public string Username { get; set; }
        public TimeSpan? DailyWorkingStartTime { get; set; }
        public TimeSpan? DailyWorkingEndTime { get; set; }
        public IssueStatus IssueStatus { get; set; }

        public string Key => Username;
    }
}
