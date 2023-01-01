using Pet.Jira.Domain.Models.Issues;
using System;

namespace Pet.Jira.Domain.Models.Worklogs
{
    public interface IWorklog
    {
        DateTime StartDate { get; set; }
        DateTime CompleteDate { get; set; }
        TimeSpan TimeSpent { get; set; }
        IIssue Issue { get; set; }
        string Author { get; set; }
        WorklogSource Source { get; set; }
    }
}
