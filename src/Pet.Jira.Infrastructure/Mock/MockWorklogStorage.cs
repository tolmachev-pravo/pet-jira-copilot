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

        public static IList<IWorklog> IssueWorklogs = new List<IWorklog>
        {
            new IssueWorklog
            {
                StartDate = DateTime.Now.Date.AddHours(11),
                TimeSpent = TimeSpan.FromHours(1),
                Issue = Issues[0]
            },
            new IssueWorklog
            {
                StartDate = DateTime.Now.Date.AddDays(-1).AddHours(16),
                TimeSpent = TimeSpan.FromHours(5),
                Issue = Issues[1]
            },
            new IssueWorklog
            {
                StartDate = DateTime.Now.Date.AddDays(-2).AddHours(19),
                TimeSpent = TimeSpan.FromHours(8),
                Issue = Issues[4]
            }
        };

        public static IList<IWorklog> RawIssueWorklogs = new List<IWorklog>
        {
            new RawIssueWorklog
            {
                StartDate = DateTime.Now.Date.AddHours(10),
                CompleteDate = DateTime.Now.Date.AddHours(11),
                Issue = Issues[0]
            },
            new RawIssueWorklog
            {
                StartDate = DateTime.Now.Date.AddHours(11),
                CompleteDate = DateTime.Now.Date.AddHours(17),
                Issue = Issues[1]
            },
            new RawIssueWorklog
            {
                StartDate = DateTime.Now.Date.AddHours(17),
                CompleteDate = DateTime.Now.Date.AddHours(19),
                Issue = Issues[2]
            },
            new RawIssueWorklog
            {
                StartDate = DateTime.Now.Date.AddDays(-1).AddHours(11),
                CompleteDate = DateTime.Now.Date.AddDays(-1).AddHours(16),
                Issue = Issues[1]
            },
            new RawIssueWorklog
            {
                StartDate = DateTime.Now.Date.AddDays(-1).AddHours(16),
                CompleteDate = DateTime.Now.Date.AddDays(-1).AddHours(20),
                Issue = Issues[3]
            },
            new RawIssueWorklog
            {
                StartDate = DateTime.Now.Date.AddDays(-5).AddHours(19),
                CompleteDate = DateTime.Now.Date.AddDays(-2).AddHours(19),
                TimeSpent = TimeSpan.FromHours(72),
                Issue = Issues[4]
            }
        };
    }
}
