using Pet.Jira.Domain.Models.Issues;
using Pet.Jira.Domain.Models.Worklogs;
using System;
using System.Collections.Generic;

namespace Pet.Jira.Infrastructure.Mock
{
    internal static class MockWorklogStorage
    {
        public static IList<Issue> Issues = new List<Issue>
        {
            new Issue
            {
                Key = "Task-0",
                Summary = Guid.NewGuid().ToString()
            },
            new Issue
            {
                Key = "Task-1",
                Summary = Guid.NewGuid().ToString()
            },
            new Issue
            {
                Key = "Task-2",
                Summary = Guid.NewGuid().ToString()
            },
            new Issue
            {
                Key = "Task-3",
                Summary = Guid.NewGuid().ToString()
            },
            new Issue
            {
                Key = "Task-4",
                Summary = Guid.NewGuid().ToString()
            }
        };

        public static IList<IssueWorklog> IssueWorklogs = new List<IssueWorklog>
        {
            new IssueWorklog
            {
                StartedAt = DateTime.Now.Date.AddHours(11),
                ElapsedTime = TimeSpan.FromHours(1),
                Issue = Issues[0]
            },
            new IssueWorklog
            {
                StartedAt = DateTime.Now.Date.AddDays(-1).AddHours(16),
                ElapsedTime = TimeSpan.FromHours(5),
                Issue = Issues[1]
            },
            new IssueWorklog
            {
                StartedAt = DateTime.Now.Date.AddDays(-2).AddHours(19),
                ElapsedTime = TimeSpan.FromHours(8),
                Issue = Issues[4]
            }
        };

        public static IList<RawIssueWorklog> RawIssueWorklogs = new List<RawIssueWorklog>
        {
            new RawIssueWorklog
            {
                StartedAt = DateTime.Now.Date.AddHours(10),
                CompletedAt = DateTime.Now.Date.AddHours(11),
                ElapsedTime = TimeSpan.FromHours(1),
                Issue = Issues[0]
            },
            new RawIssueWorklog
            {
                StartedAt = DateTime.Now.Date.AddHours(11),
                CompletedAt = DateTime.Now.Date.AddHours(17),
                ElapsedTime = TimeSpan.FromHours(6),
                Issue = Issues[1]
            },
            new RawIssueWorklog
            {
                StartedAt = DateTime.Now.Date.AddHours(17),
                CompletedAt = DateTime.Now.Date.AddHours(19),
                ElapsedTime = TimeSpan.FromHours(2),
                Issue = Issues[2]
            },
            new RawIssueWorklog
            {
                StartedAt = DateTime.Now.Date.AddDays(-1).AddHours(11),
                CompletedAt = DateTime.Now.Date.AddDays(-1).AddHours(16),
                ElapsedTime = TimeSpan.FromHours(5),
                Issue = Issues[1]
            },
            new RawIssueWorklog
            {
                StartedAt = DateTime.Now.Date.AddDays(-1).AddHours(16),
                CompletedAt = DateTime.Now.Date.AddDays(-1).AddHours(20),
                ElapsedTime = TimeSpan.FromHours(4),
                Issue = Issues[3]
            },
            new RawIssueWorklog
            {
                StartedAt = DateTime.Now.Date.AddDays(-5).AddHours(19),
                CompletedAt = DateTime.Now.Date.AddDays(-2).AddHours(19),
                ElapsedTime = TimeSpan.FromHours(72),
                Issue = Issues[4]
            }
        };
    }
}
