using Pet.Jira.Domain.Models.Issues;
using System;

namespace Pet.Jira.Domain.Models.Worklogs
{
    public class RawIssueWorklog : IWorklog
    {
        public DateTime StartDate { get; set; }
        public DateTime CompleteDate { get; set; }

        public TimeSpan TimeSpent
        {
            get => CompleteDate - StartDate;
            set { }
        }

        public IIssue Issue { get; set; }

        public bool IsBetween(DateTime from, DateTime to)
        {
            return StartDate < to && CompleteDate > from;
        }
    }
}
