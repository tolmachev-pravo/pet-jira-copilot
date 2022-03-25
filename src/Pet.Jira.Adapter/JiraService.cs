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
        private const string InProgressStatusId = "3";
        private const string StatusFieldName = "status";
        private const int StartWorkingTime = 10;
        private const int EndWorkingTime = 19;

        private readonly Atlassian.Jira.Jira _jiraClient;
        private readonly IJiraConfiguration _config;

        public JiraService(
            IJiraConfiguration config)
        {
            _jiraClient = Atlassian.Jira.Jira.CreateRestClient(config.Url, config.UserName, config.Password);
            _config = config;
        }

        public async Task<IPagedQueryResult<Atlassian.Jira.Issue>> GetIssues(string jql, int count) =>
            await _jiraClient.Issues.GetIssuesFromJqlAsync(
                new IssueSearchOptions(jql)
                {
                    MaxIssuesPerRequest = count,
                    ValidateQuery = true
                });

        /// <summary>
        /// 
        /// </summary>
        /// <param name="period"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public async Task<IEnumerable<ActualWorklog>> GetCurrentUserActualWorklogsAsync(
            string period,
            int count)
        {
            var issues = await GetIssues(
                jql: $"worklogDate >= '{period}' AND worklogAuthor = currentUser() ORDER BY updatedDate DESC",
                count: count);
            var actualWorklogs = new List<ActualWorklog> { };
            foreach (var issue in issues)
            {
                var worklogs = await _jiraClient.Issues.GetWorklogsAsync(issue.Key.Value);
                worklogs = worklogs.Where(record => record.Author == _config.UserName).ToList();
                actualWorklogs.AddRange(worklogs.Select(worklog => new ActualWorklog
                {
                    StartDate = worklog.StartDate.Value,
                    TimeSpent = TimeSpan.FromSeconds(worklog.TimeSpentInSeconds),
                    Issue = new Issue
                    {
                        Key = issue.Key.Value,
                        Summary = issue.Summary,
                        Link = Path.Combine(_jiraClient.Url, "browse", issue.Key.Value)
                    }
                }));
            }
            return actualWorklogs;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="period"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public async Task<IEnumerable<EstimatedWorklog>> GetCurrentUserEstimatedWorklogsAsync(
            string period,
            int count)
        {
            var issues = await GetIssues(
                jql: $"assignee = currentUser() AND updatedDate >= '{period}' ORDER BY updatedDate DESC",
                count: count);
            var rawEstimatedWorklogs = new List<EstimatedWorklog> { };
            var dtNow = await _jiraClient.ServerInfo.GetServerInfoAsync();
            foreach (var issue in issues)
            {
                var changeLogs = await _jiraClient.Issues.GetChangeLogsAsync(issue.Key.Value);
                changeLogs = changeLogs.Where(record => record.Items.Any(item => item.FieldName == StatusFieldName)).ToList();
                DateTime startDate = DateTime.MinValue;
                var inProgress = false;
                foreach (var changeLog in changeLogs)
                {
                    if (changeLog.Items.Any(item => item.FieldName == StatusFieldName && item.ToId == InProgressStatusId))
                    {
                        startDate = changeLog.CreatedDate;
                        inProgress = true;
                    }

                    if (changeLog.Items.Any(item => item.FieldName == StatusFieldName && item.FromId == InProgressStatusId))
                    {
                        inProgress = false;
                        rawEstimatedWorklogs.Add(new EstimatedWorklog
                        {
                            StartDate = startDate,
                            EndDate = changeLog.CreatedDate,
                            Issue = new Issue
                            {
                                Key = issue.Key.Value,
                                Summary = issue.Summary,
                                Link = Path.Combine(_jiraClient.Url, "browse", issue.Key.Value)
                            }
                        });
                    }
                }

                if (inProgress)
                {
                    rawEstimatedWorklogs.Add(new EstimatedWorklog
                    {
                        StartDate = startDate,
                        EndDate = dtNow.ServerTime.Value.DateTime,
                        Issue = new Issue
                        {
                            Key = issue.Key.Value,
                            Summary = issue.Summary,
                            Link = Path.Combine(_jiraClient.Url, "browse", issue.Key.Value)
                        }
                    });
                }
            }
            return rawEstimatedWorklogs;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rawEstimatedWorklogs"></param>
        /// <param name="StartDate"></param>
        /// <param name="EndDate"></param>
        private IEnumerable<EstimatedWorklog> PrepareEstimatedWorklogs(
            IEnumerable<EstimatedWorklog> rawEstimatedWorklogs,
            DateTime StartDate,
            DateTime EndDate)
        {
            List<EstimatedWorklog> estimatedWorklogs = new List<EstimatedWorklog>();
            var startDate = EndDate.Date;
            while (startDate >= StartDate.Date)
            {
                var fromStartDate = startDate.AddHours(StartWorkingTime);
                var toStartDate = startDate.AddHours(EndWorkingTime);
                var dateWorklogs = rawEstimatedWorklogs
                    .Where(item => item.EndDate > fromStartDate 
                                   && item.StartDate < toStartDate)
                    .ToList();
                foreach (var dateWorklog in dateWorklogs)
                {
                    var estimatedStartDate = dateWorklog.StartDate > fromStartDate
                        ? dateWorklog.StartDate
                        : fromStartDate;
                    var estimatedEndDate = dateWorklog.EndDate < toStartDate
                        ? dateWorklog.EndDate
                        : toStartDate;
                    estimatedWorklogs.Add(new EstimatedWorklog
                    {
                        StartDate = estimatedStartDate,
                        EndDate = estimatedEndDate,
                        Issue = dateWorklog.Issue
                    });
                }
                startDate = startDate.AddDays(-1);
            }

            return estimatedWorklogs;
        }

        public async Task<IEnumerable<DayUserWorklog>> GetUserDayWorklogs(
            DateTime fromDate,
            DateTime toDate,
            int issueCount)
        {
            var startDateJiraFormat = fromDate.ToString("yyyy/MM/dd");
            var rawEstimatedWorklogs = await GetCurrentUserEstimatedWorklogsAsync(startDateJiraFormat, issueCount);
            var estimatedWorklogs = PrepareEstimatedWorklogs(rawEstimatedWorklogs, fromDate, toDate);
            var actualWorklogs = await GetCurrentUserActualWorklogsAsync(startDateJiraFormat, issueCount);
            
            var result = new List<DayUserWorklog>();
            var cycleDate = toDate.Date;
            while (cycleDate >= fromDate.Date)
            {
                result.Add(new DayUserWorklog
                {
                    Date = cycleDate,
                    ActualWorklogs = actualWorklogs.Where(record => record.StartDate.Date == cycleDate).ToList(),
                    EstimatedWorklogs = estimatedWorklogs.Where(record => record.StartDate.Date == cycleDate).ToList()
                });
                cycleDate = cycleDate.AddDays(-1);
            }

            Calculate(result);
            return result;
        }

        private void Calculate(IEnumerable<DayUserWorklog> worklogs)
        {
            foreach (var worklog in worklogs)
            {
                var workTime = new TimeSpan(8, 0, 0).Ticks;
                // Время зафиксированное за день
                var dayTimeSpent = new TimeSpan(worklog.ActualWorklogs.Sum(record => record.TimeSpent.Ticks));
                // Привязка актуальных таймлогов к 
                foreach (var estimatedWorklog in worklog.EstimatedWorklogs)
                {
                    estimatedWorklog.ActualWorklogs = worklog.ActualWorklogs
                        .Where(record => record.Issue.Key == estimatedWorklog.Issue.Key
                                         && record.StartDate == estimatedWorklog.EndDate)
                        .ToList();
                }

                // Автоматические
                var autoActualWorklogs = worklog.EstimatedWorklogs.SelectMany(record => record.ActualWorklogs);
                // Вручную внесенные таймлоги
                var manualActualWorklogs = worklog.ActualWorklogs.Except(autoActualWorklogs);
                // Вручную списанное время
                var manualTimeSpent = manualActualWorklogs.Sum(record => record.TimeSpent.Ticks);

                // Время выполнения всех задач
                var fullRawTimeSpent = worklog.EstimatedWorklogs.Sum(record => record.RawTimeSpent.Ticks);
                // Предполагаемый остаток для автоматического списания времени
                var estimatedRestAutoTimeSpent = Convert.ToDecimal(workTime - manualTimeSpent);
                //if (fullRawTimeSpent + manualTimeSpent < workTime)
                //{
                //    var percent1 = Convert.ToDecimal(fullRawTimeSpent) / new TimeSpan(9, 0, 0).Ticks;
                //    estimatedRestAutoTimeSpent =
                //        fullRawTimeSpent - new TimeSpan(1, 0, 0).Ticks * percent1 - manualTimeSpent;
                //}

                // Заполняем предполагаемое время для каждой задачи в пропорциях
                foreach (var estimatedWorklog in worklog.EstimatedWorklogs)
                {
                    if (estimatedWorklog.ActualTimeSpent.Ticks == 0)
                    {
                        var percent = Convert.ToDecimal(estimatedWorklog.RawTimeSpent.Ticks) / fullRawTimeSpent;
                        var estimatedTimeSpent = new TimeSpan(Convert.ToInt64(percent * estimatedRestAutoTimeSpent));
                        estimatedWorklog.EstimatedTimeSpent = estimatedTimeSpent;
                    }
                    else
                    {
                        estimatedWorklog.EstimatedTimeSpent = estimatedWorklog.ActualTimeSpent;
                    }
                }
            }
        }

        public async Task AddTimeLog(string issueKey, TimeSpan timeSpan, DateTime startTime)
        {
            try
            {
                var minutesLag = timeSpan.Seconds >= 30 ? 1 : 0;
                var worklog = new Worklog(
                    $"{timeSpan.Hours}h {timeSpan.Minutes + minutesLag}m", 
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
