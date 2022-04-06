using Atlassian.Jira;
using Pet.Jira.Application.Worklogs;
using Pet.Jira.Application.Worklogs.Queries;
using Pet.Jira.Domain.Models.Worklogs;
using Pet.Jira.Infrastructure.Jira.Dto;
using Pet.Jira.Infrastructure.Jira.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Infrastructure.Jira
{
    public class JiraWorklogDataSource : IWorklogDataSource
    {
        private readonly IJiraService _jiraService;
        private readonly IJiraQueryFactory _queryFactory;

        public JiraWorklogDataSource(
            IJiraService jiraService,
            IJiraQueryFactory queryFactory)
        {
            _jiraService = jiraService;
            _queryFactory = queryFactory;
        }

        public async Task<IEnumerable<IssueWorklog>> GetIssueWorklogsAsync(
            GetIssueWorklogs.Query query, 
            CancellationToken cancellationToken = default)
        {
            var issueQuery = _queryFactory.Create()
                .Where("worklogDate", JiraQueryComparisonType.GreaterOrEqual, query.StartDate)
                .Where("worklogDate", JiraQueryComparisonType.LessOrEqual, query.EndDate)
                .Where("worklogAuthor", JiraQueryComparisonType.Equal, JiraQueryMacros.CurrentUser)
                .OrderBy("updatedDate", JiraQueryOrderType.Desc)
                .ToString();
            var issueSearchOptions = new IssueSearchOptions(issueQuery);

            var currentUser = await _jiraService.GetCurrentUserAsync();
            var worklogFilter = new Func<Worklog, bool>(worklog =>
                worklog.Author == currentUser.Username
                && worklog.StartDate >= query.StartDate
                && worklog.StartDate <= query.EndDate);

            var issueWorklogs = await _jiraService.GetIssueWorklogsAsync(issueSearchOptions, worklogFilter, cancellationToken);

            return issueWorklogs.Select(issueWorklog => issueWorklog.Adapt<IssueWorklog>());
        }

        public async Task<IEnumerable<RawIssueWorklog>> GetRawIssueWorklogsAsync(
            GetRawIssueWorklogs.Query query, 
            CancellationToken cancellationToken = default)
        {
            var issueQuery = _queryFactory.Create()
                .Where("updatedDate", JiraQueryComparisonType.GreaterOrEqual, query.StartDate)
                .Where("assignee", JiraQueryComparisonType.Equal, JiraQueryMacros.CurrentUser)
                .OrderBy("updatedDate", JiraQueryOrderType.Desc)
                .ToString();
            var issueSearchOptions = new IssueSearchOptions(issueQuery);
            var issues = await _jiraService.GetIssuesAsync(issueSearchOptions, cancellationToken);

            var changeLogFilter = new Func<IssueChangeLog, bool>(changeLog =>
                changeLog.Items.Any(item => item.FieldName == JiraConstants.Status.FieldName));

            var changeLogItemFilter = new Func<IssueChangeLogItem, bool>(changeLogItem =>
                changeLogItem.FieldName == JiraConstants.Status.FieldName
                && (changeLogItem.ToId == JiraConstants.Status.InProgress
                    || changeLogItem.FromId == JiraConstants.Status.InProgress));

            var issueChangeLogItems = await _jiraService.GetIssueChangeLogItemsAsync(issues, changeLogFilter, changeLogItemFilter, cancellationToken);

            var result = new List<RawIssueWorklog> { };
            foreach (var issue in issues)
            {
                var items = issueChangeLogItems.Where(record => record.ChangeLog.Issue.Key == issue.Key)
                    .OrderBy(record => record.ChangeLog.CreatedDate)
                    .ToList();
                var rawIssueWorklogs = ConvertTo(items).Where(record => record.IsBetween(query.StartDate, query.EndDate));
                result.AddRange(rawIssueWorklogs);
            }

            return result;
        }

        private IEnumerable<RawIssueWorklog> ConvertTo(
            IList<IssueChangeLogItemDto> issueChangeLogItems)
        {
            var i = 0;
            while (i < issueChangeLogItems.Count())
            {
                var item = issueChangeLogItems[i];
                // 1. Первый элемент сразу выходит из прогресса. Значит это завершающий
                if (item.FromInProgress)
                {
                    yield return new RawIssueWorklog()
                    {
                        CompletedAt = item.ChangeLog.CreatedDate,
                        StartedAt = DateTime.MinValue,
                        Issue = item.ChangeLog.Issue.Adapt()
                    };
                }
                // 2. Это последний элемент и он не завершается
                else if (i == (issueChangeLogItems.Count() - 1))
                {
                    yield return new RawIssueWorklog()
                    {
                        CompletedAt = DateTime.MaxValue,
                        StartedAt = item.ChangeLog.CreatedDate,
                        Issue = item.ChangeLog.Issue.Adapt()
                    };
                }
                // 3. Обычный случай когда после FromInProgress следует ToInProgress
                else
                {
                    yield return new RawIssueWorklog()
                    {
                        CompletedAt = issueChangeLogItems[i + 1].ChangeLog.CreatedDate,
                        StartedAt = item.ChangeLog.CreatedDate,
                        Issue = item.ChangeLog.Issue.Adapt()
                    };
                }

                i += 2;
            }
        }
    }
}
