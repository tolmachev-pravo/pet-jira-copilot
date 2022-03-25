using Pet.Jira.Domain.Models.Issues;
using System;

namespace Pet.Jira.Domain.Models.Worklogs
{
    public interface IWorklog
    {
        DateTime StartedAt { get; set; }
        DateTime CompletedAt { get; set; }
        TimeSpan ElapsedTime { get; set; }
        IIssue Issue { get; set; }
    }
}
