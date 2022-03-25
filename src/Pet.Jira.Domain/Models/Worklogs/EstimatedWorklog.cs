using Pet.Jira.Domain.Models.Issues;
using System;

namespace Pet.Jira.Domain.Models.Worklogs
{
    public class EstimatedWorklog : IWorklog
    {
        public DateTime StartedAt { get; set; }
        public DateTime CompletedAt { get; set; }
        public TimeSpan ElapsedTime { get; set; }
        public TimeSpan RestTime { get; set; }
        public IIssue Issue { get; set; }
    }
}
