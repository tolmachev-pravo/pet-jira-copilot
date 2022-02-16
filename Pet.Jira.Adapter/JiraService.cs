using Atlassian.Jira;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Pet.Jira.Adapter
{
    public class JiraService
    {
        private readonly Atlassian.Jira.Jira _jiraClient;

        public JiraService(
            IJiraConfiguration config)
        {
            _jiraClient = Atlassian.Jira.Jira.CreateRestClient(config.Url, config.UserName, config.Password);
        }

        public async Task<IPagedQueryResult<Issue>> GetIssues(string jql, int count) =>
            await _jiraClient.Issues.GetIssuesFromJqlAsync(
                new IssueSearchOptions(jql)
                {
                    MaxIssuesPerRequest = count,
                    ValidateQuery = true
                });

        public async Task<Dictionary<DateTime, List<TimeLog>>> GetCalculatedIssueTimeLogs(string issueJql, int issueCount)
        {
            var issues = await GetIssues(issueJql, issueCount);
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
                            IssueName = issue.Key.Value,
                            IssueSummary = issue.Summary,
                            IssueLink = Path.Combine(_jiraClient.Url, "browse", issue.Key.Value)
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
                        IssueName = dateTimeLog.IssueName,
                        IssueSummary = dateTimeLog.IssueSummary,
                        IssueLink = dateTimeLog.IssueLink
                    });
                }

                dictionary.Add(startDate, list);
                Console.WriteLine(
                    $@"{startDate.ToShortDateString()} - {new TimeSpan(list.Sum(item => item.Diff.Ticks))}");
                startDate = startDate.AddDays(-1);
            }

            return dictionary;
        }

        public async Task AddTimeLog(string issueKey, TimeSpan timeSpan, DateTime startTime)
        {
            try
            {
                var worklog = new Worklog(
                    $"{timeSpan.Hours}h {timeSpan.Minutes}m", 
                    startTime,
                    "Dev");
                await _jiraClient.Issues.AddWorklogAsync(issueKey, worklog);
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}
