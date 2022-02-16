using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Atlassian.Jira;
using Microsoft.Extensions.Configuration;

namespace Pet.Jira.App
{
    public class JiraService
    {
        private readonly IConfigurationRoot _config;
        private readonly Atlassian.Jira.Jira _jiraClient;

        public JiraService(
            IConfigurationRoot config)
        {
            _config = config;

            var userName = _config["Jira:UserName"];
            var password = _config["Jira:Password"];
            var url = _config["Jira:Url"];
            _jiraClient = Atlassian.Jira.Jira.CreateRestClient(url, userName, password);
        }

        public async Task Test()
        {
            var issues = await _jiraClient.Issues.GetIssuesFromJqlAsync(new IssueSearchOptions("assignee = currentUser() AND updatedDate >= -4w ORDER BY updatedDate DESC")
            {
                MaxIssuesPerRequest = 40,
                ValidateQuery = true
            });

            var timeLogs = new List<TimeLog> { };

            foreach (var issue in issues)
            {
                var changeLog = await _jiraClient.Issues.GetChangeLogsAsync(issue.Key.Value);
                DateTime from = DateTime.MinValue;
                foreach (var changeLogItem in changeLog)
                {
                    if (changeLogItem.Items.Any(item => item.FieldName == "status" && item.ToId == "3"))
                    {

                        from = changeLogItem.CreatedDate;
                    }
                    if (changeLogItem.Items.Any(item => item.FieldName == "status" && item.FromId == "3"))
                    {
                        timeLogs.Add(new TimeLog
                        {
                            From = from,
                            To = changeLogItem.CreatedDate,
                            IssueName = issue.Key.Value
                        });
                    }
                }
            }
            var startDate = DateTime.Now.Date;
            Dictionary<DateTime, List<TimeLog>> dictionary = new Dictionary<DateTime, List<TimeLog>>();
            while (startDate > new DateTime(2022, 01, 01))
            {
                var fromStartDate = startDate.AddHours(10);
                var toStartDate = startDate.AddHours(19);
                var dateTimeLogs = timeLogs
                    .Where(item => (item.To > fromStartDate && item.From < toStartDate));
                List<TimeLog> list = new List<TimeLog>();
                foreach (var dateTimeLog in dateTimeLogs)
                {
                    var fDate = dateTimeLog.From > fromStartDate
                        ? dateTimeLog.From
                        : fromStartDate;
                    var tDate = dateTimeLog.To < toStartDate
                        ? dateTimeLog.To
                        : toStartDate;
                    list.Add(new TimeLog
                    {
                        From = fDate,
                        To = tDate,
                        IssueName = dateTimeLog.IssueName
                    });
                }
                dictionary.Add(startDate, list);
                Console.WriteLine($@"{startDate.ToShortDateString()} - {new TimeSpan(list.Sum(item => item.Diff.Ticks))}");
                startDate = startDate.AddDays(-1);
            }

        }

        public class TimeLog
        {
            public DateTime From { get; set; }

            public DateTime To { get; set; }

            public string IssueName { get; set; }

            public TimeSpan Diff => (To - From);
        }
    }
}
