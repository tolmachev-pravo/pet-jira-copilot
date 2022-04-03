using Atlassian.Jira;
using Microsoft.Extensions.Options;
using Pet.Jira.Application.Authentication;
using Pet.Jira.Application.Worklogs.Dto;
using Pet.Jira.Domain.Models.Worklogs;
using Pet.Jira.Infrastructure.Worklogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pet.Jira.Infrastructure.Jira
{
    public class JiraService : IJiraService
    {
        private readonly JiraLinkGenerator _linkGenerator;
        private readonly WorklogFactory _worklogFactory;
        private readonly Atlassian.Jira.Jira _jiraClient;
        private readonly IJiraConfiguration _config;

        public JiraService(
            IOptions<JiraConfiguration> jiraConfiguration,
            JiraLinkGenerator linkGenerator,
            WorklogFactory worklogFactory,
            IIdentityService identityService)
        {
            _linkGenerator = linkGenerator;
            _worklogFactory = worklogFactory;
            _config = jiraConfiguration.Value;
            var user = identityService.CurrentUser;
            _jiraClient = Atlassian.Jira.Jira.CreateRestClient(_config.Url, user.Username, user.Password);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="period"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        // ToDo: Добавить передачу фильтра для Issue (нужен генератор запросов) + фильтрация для Worklog
        public async Task<IEnumerable<ActualWorklog>> GetActualWorklogsAsync(
            string period,
            int count)
        {
            var issues = await GetIssuesAsync(
                jql: $"worklogDate >= '{period}' AND worklogAuthor = currentUser() ORDER BY updatedDate DESC",
                count: count);

            var actualWorklogs = new List<ActualWorklog> { };
            foreach (var issue in issues)
            {
                var issueWorklogs = await _jiraClient.Issues.GetWorklogsAsync(issue.Key.Value);
                var userIssueWorklogs = issueWorklogs.Where(record => record.Author == _config.Username).ToList();
                actualWorklogs.AddRange(userIssueWorklogs.Select(worklog => new ActualWorklog
                {

                    StartedAt = worklog.StartDate.Value,
                    ElapsedTime = TimeSpan.FromSeconds(worklog.TimeSpentInSeconds),
                    Issue = new Domain.Models.Issues.Issue
                    {
                        Key = issue.Key.Value,
                        Summary = issue.Summary,
                        Link = _linkGenerator.Generate(issue.Key.Value)
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
        public async Task<IEnumerable<EstimatedWorklog>> GetEstimatedWorklogsAsync(
            string period,
            int count)
        {
            var issues = await GetIssuesAsync(
                jql: $"assignee = currentUser() AND updatedDate >= '{period}' ORDER BY updatedDate DESC",
                count: count);
            var rawEstimatedWorklogs = new List<EstimatedWorklog> { };
            var serverInfo = await _jiraClient.ServerInfo.GetServerInfoAsync();
            foreach (var issue in issues)
            {
                var changeLogs = await _jiraClient.Issues.GetChangeLogsAsync(issue.Key.Value);
                changeLogs = changeLogs.Where(record => record.Items.Any(item => item.FieldName == JiraConstants.Status.FieldName)).ToList();
                DateTime startDate = DateTime.MinValue;
                var inProgress = false;
                foreach (var changeLog in changeLogs)
                {
                    if (changeLog.Items.Any(item => item.FieldName == JiraConstants.Status.FieldName 
                    && item.ToId == JiraConstants.Status.InProgress))
                    {
                        startDate = changeLog.CreatedDate;
                        inProgress = true;
                    }

                    if (changeLog.Items.Any(item => item.FieldName == JiraConstants.Status.FieldName
                    && item.FromId == JiraConstants.Status.InProgress))
                    {
                        inProgress = false;
                        var estimatedWorklog = _worklogFactory.Create<EstimatedWorklog>(
                            startedAt: startDate,
                            completedAt: changeLog.CreatedDate,
                            issue: issue);
                        rawEstimatedWorklogs.Add(estimatedWorklog);
                    }
                }

                if (inProgress)
                {
                    var estimatedWorklog = _worklogFactory.Create<EstimatedWorklog>(
                        startedAt: startDate,
                        completedAt: serverInfo.ServerTime.Value.DateTime,
                        issue: issue);
                    rawEstimatedWorklogs.Add(estimatedWorklog);
                }
            }
            return rawEstimatedWorklogs;
        }

        public async Task<IEnumerable<DailyWorklogSummary>> GetUserDayWorklogs(
            DateTime fromDate,
            DateTime toDate,
            int issueCount)
        {
            var startDateJiraFormat = fromDate.ToString("yyyy/MM/dd");
            var rawEstimatedWorklogs = await GetEstimatedWorklogsAsync(startDateJiraFormat, issueCount);
            var estimatedWorklogs = PrepareEstimatedWorklogs(rawEstimatedWorklogs, fromDate, toDate);
            var actualWorklogs = await GetActualWorklogsAsync(startDateJiraFormat, issueCount);

            var result = new List<DailyWorklogSummary>();
            var cycleDate = toDate.Date;
            while (cycleDate >= fromDate.Date)
            {
                result.Add(new DailyWorklogSummary
                {
                    Date = cycleDate,
                    ActualWorklogs = actualWorklogs.Where(record => record.StartedAt.Date == cycleDate).ToList(),
                    EstimatedWorklogs = estimatedWorklogs.Where(record => record.StartedAt.Date == cycleDate).ToList()
                });
                cycleDate = cycleDate.AddDays(-1);
            }

            Calculate(result);
            return result;
        }

        private void Calculate(IEnumerable<DailyWorklogSummary> worklogs)
        {
            foreach (var worklog in worklogs)
            {
                var workTime = new TimeSpan(8, 0, 0).Ticks;
                // Время зафиксированное за день
                var dayTimeSpent = new TimeSpan(worklog.ActualWorklogs.Sum(record => record.ElapsedTime.Ticks));
                // Привязка актуальных таймлогов к 
                foreach (var estimatedWorklog in worklog.EstimatedWorklogs)
                {
                    estimatedWorklog.ActualWorklogs = worklog.ActualWorklogs
                        .Where(record => record.Issue.Key == estimatedWorklog.Issue.Key
                                         && record.StartedAt == estimatedWorklog.CompletedAt)
                        .ToList();
                }

                // Автоматические
                var autoActualWorklogs = worklog.EstimatedWorklogs.SelectMany(record => record.ActualWorklogs);
                // Вручную внесенные таймлоги
                var manualActualWorklogs = worklog.ActualWorklogs.Except(autoActualWorklogs);
                // Вручную списанное время
                var manualTimeSpent = manualActualWorklogs.Sum(record => record.ElapsedTime.Ticks);

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
                var fromStartDate = startDate.AddHours(JiraConstants.Date.StartWorkingTime);
                var toStartDate = startDate.AddHours(JiraConstants.Date.EndWorkingTime);
                var dateWorklogs = rawEstimatedWorklogs
                    .Where(item => item.CompletedAt > fromStartDate
                                   && item.StartedAt < toStartDate)
                    .ToList();
                foreach (var dateWorklog in dateWorklogs)
                {
                    var estimatedStartDate = dateWorklog.StartedAt > fromStartDate
                        ? dateWorklog.StartedAt
                        : fromStartDate;
                    var estimatedEndDate = dateWorklog.CompletedAt < toStartDate
                        ? dateWorklog.CompletedAt
                        : toStartDate;
                    estimatedWorklogs.Add(new EstimatedWorklog
                    {
                        StartedAt = estimatedStartDate,
                        CompletedAt = estimatedEndDate,
                        Issue = dateWorklog.Issue
                    });
                }
                startDate = startDate.AddDays(-1);
            }

            return estimatedWorklogs;
        }

        private async Task<IPagedQueryResult<Atlassian.Jira.Issue>> GetIssuesAsync(string jql, int count) =>
            await _jiraClient.Issues.GetIssuesFromJqlAsync(
                new IssueSearchOptions(jql)
                {
                    MaxIssuesPerRequest = count,
                    ValidateQuery = true
                });

        public async Task AddWorklogAsync(AddedWorklogDto worklogDto)
        {
            try
            {
                var minutesLag = worklogDto.ElapsedTime.Seconds >= 30 ? 1 : 0;
                var worklog = new Worklog(
                    $"{worklogDto.ElapsedTime.Hours}h {worklogDto.ElapsedTime.Minutes + minutesLag}m",
                    worklogDto.StartedAt,
                    "Dev");
                await _jiraClient.Issues.AddWorklogAsync(worklogDto.IssueKey, worklog);
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<LoginResponse> Login(LoginRequest request)
        {
            try
            {
                var jiraClient = Atlassian.Jira.Jira.CreateRestClient(_config.Url, request.Username, request.Password);
                await jiraClient.ServerInfo.GetServerInfoAsync();
                return new LoginResponse(true);
            }
            catch (Exception e)
            {
                return new LoginResponse(false, e.Message);
            }
        }
    }
}
