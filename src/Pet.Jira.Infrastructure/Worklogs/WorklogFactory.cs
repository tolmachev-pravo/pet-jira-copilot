using Atlassian.Jira;
using Pet.Jira.Domain.Models.Worklogs;
using Pet.Jira.Infrastructure.Jira;
using System;

namespace Pet.Jira.Infrastructure.Worklogs
{
    public class WorklogFactory
    {
        private readonly IJiraLinkGenerator _linkGenerator;

        public WorklogFactory(
            IJiraLinkGenerator linkGenerator)
        {
            _linkGenerator = linkGenerator;
        }

        public T Create<T>(
            DateTime startedAt,
            DateTime completedAt,
            Issue issue)
            where T: EstimatedWorklog, new()
        {
            return new T
            {
                StartedAt = startedAt,
                CompletedAt = completedAt,
                Issue = new Domain.Models.Issues.Issue
                {
                    Key = issue.Key.Value,
                    Summary = issue.Summary,
                    Link = _linkGenerator.Generate(issue.Key.Value)
                }
            };
        }

        public T Create<T>(DateTime startedAt, DateTime completedAt, Domain.Models.Issues.IIssue issue)
            where T : EstimatedWorklog, new()
        {
            return new T
            {
                StartedAt = startedAt,
                CompletedAt = completedAt,
                Issue = issue
            };
        }
    }
}
