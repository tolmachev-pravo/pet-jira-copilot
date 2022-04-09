using Pet.Jira.Domain.Models.Issues;
using Pet.Jira.Domain.Models.Worklogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pet.Jira.Infrastructure.Mock
{
    internal static class MockWorklogStorage
    {
        private static class IssueGenerator
        {
            public static Issue Create()
            {
                var key = $"CASEM-{new Random().Next(1000, 10000)}";
                var summary = TextGenerator.Create();
                return new Issue
                {
                    Key = key,
                    Summary = summary
                };
            }

            public static string Text = "";
        }

        private static class TextGenerator
        {
            public static string Create()
            {
                Random random = new Random();
                var builder = new StringBuilder();
                var wordsCount = random.Next(5, 15);
                char[] lowers = Enumerable.Range(0, 32).Select((x, i) => (char)('а' + i)).ToArray();
                char[] uppers = Enumerable.Range(0, 32).Select((x, i) => (char)('А' + i)).ToArray();
                for (int i = 0; i < wordsCount; i++)
                {
                    string word = string.Empty;
                    var lettersCount = random.Next(5, 15);
                    for (int j = 0; j < lettersCount; j++)
                    {
                        int letterPosition = random.Next(0, lowers.Length - 1);
                        word += lowers[letterPosition];
                    }

                    builder.Append(word);
                    builder.Append(" ");
                }

                return builder.ToString();
            }

            public static string Text = "";
        }
        public static IList<Issue> Issues = new List<Issue>
        {
            IssueGenerator.Create(),
            IssueGenerator.Create(),
            IssueGenerator.Create(),
            IssueGenerator.Create(),
            IssueGenerator.Create(),
            IssueGenerator.Create(),
            IssueGenerator.Create(),
            IssueGenerator.Create()
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
