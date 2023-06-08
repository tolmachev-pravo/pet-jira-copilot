using Atlassian.Jira;
using Pet.Jira.Application.Authentication;
using Pet.Jira.Application.Storage;
using Pet.Jira.Application.Time;
using Pet.Jira.Application.Worklogs;
using Pet.Jira.Application.Worklogs.Queries;
using Pet.Jira.Domain.Models.Users;
using Pet.Jira.Domain.Models.Worklogs;
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
        private readonly IIdentityService _identityService;
        private readonly ITimeProvider _timeProvider;
        private readonly IStorage<string, UserProfile> _userProfileStorage;

        public JiraWorklogDataSource(
            IJiraService jiraService,
            IJiraQueryFactory queryFactory,
            IIdentityService identityService,
            ITimeProvider timeProvider,
            IStorage<string, UserProfile> userProfileStorage)
        {
            _jiraService = jiraService;
            _queryFactory = queryFactory;
            _identityService = identityService;
            _timeProvider = timeProvider;
            _userProfileStorage = userProfileStorage;
        }

        /// <summary>
        /// Get issue worklogs
        /// </summary>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IEnumerable<IWorklog>> GetIssueWorklogsAsync(
            GetIssueWorklogs.Query query, 
            CancellationToken cancellationToken = default)
        {
            var issueQuery = _queryFactory.Create()
                .Where("worklogDate", JiraQueryComparisonType.GreaterOrEqual, query.StartDate)
                .Where("worklogDate", JiraQueryComparisonType.LessOrEqual, query.EndDate)
                .Where("worklogAuthor", JiraQueryComparisonType.Equal, JiraQueryMacros.CurrentUser)
                .OrderBy("updatedDate", JiraQueryOrderType.Desc)
                .ToString();
            var issueSearchOptions = new IssueSearchOptions(issueQuery)
            {
                MaxIssuesPerRequest = JiraConstants.DefaultMaxIssuesPerRequest
            };

            var user = await _identityService.GetCurrentUserAsync();
            var userProfile = await _userProfileStorage.GetValueAsync(user.Key, cancellationToken);

            var worklogFilter = new Func<Worklog, bool>(worklog =>
                worklog.Author == userProfile.Username
                && worklog.StartDate >= query.StartDate
                && worklog.StartDate <= query.EndDate);

            var issueWorklogs = await _jiraService.GetIssueWorklogsAsync(issueSearchOptions, worklogFilter, cancellationToken);

            return issueWorklogs.Select(issueWorklog => issueWorklog.Adapt(_timeProvider, userProfile.TimeZoneInfo));
        }

        /// <summary>
        /// Get raw issue worklogs
        /// </summary>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IEnumerable<IWorklog>> GetRawIssueWorklogsAsync(
            GetRawIssueWorklogs.Query query, 
            CancellationToken cancellationToken = default)
        {
            var issueQuery = _queryFactory.Create()
                .Where("updatedDate", JiraQueryComparisonType.GreaterOrEqual, query.StartDate)
                .Where("assignee", JiraQueryComparisonType.Equal, JiraQueryMacros.CurrentUser)
                .OrderBy("updatedDate", JiraQueryOrderType.Desc)
                .ToString();
            var issueSearchOptions = new IssueSearchOptions(issueQuery)
            {
                MaxIssuesPerRequest = JiraConstants.DefaultMaxIssuesPerRequest
            };

            var issues = await _jiraService.GetIssuesAsync(issueSearchOptions, cancellationToken);

            var user = await _identityService.GetCurrentUserAsync();
            var userProfile = await _userProfileStorage.GetValueAsync(user.Key, cancellationToken);

            var changeLogFilter = new Func<IssueChangeLog, bool>(changeLog =>
                changeLog.Items.Any(item => item.FieldName == JiraConstants.Status.FieldName));

            var changeLogItemFilter = new Func<IssueChangeLogItem, bool>(changeLogItem =>
                changeLogItem.FieldName == JiraConstants.Status.FieldName
                && (changeLogItem.ToId == query.IssueStatusId
                    || changeLogItem.FromId == query.IssueStatusId));

            var issueChangeLogItems = await _jiraService.GetIssueChangeLogItemsAsync(issues, changeLogFilter, changeLogItemFilter, cancellationToken);

            var result = new List<RawIssueWorklog> { };
            foreach (var issue in issues)
            {
                var rawIssueWorklogs = issueChangeLogItems
                    .Where(item => item.ChangeLog.Issue.Key == issue.Key)
                    .OrderBy(item => item.ChangeLog.CreatedDate)
                    .ToList()
                    .ConvertTo<RawIssueWorklog>(query.IssueStatusId, _timeProvider, userProfile.TimeZoneInfo)
                    .Where(issueWorklog => issueWorklog.IsBetween(query.StartDate, query.EndDate));
                result.AddRange(rawIssueWorklogs);
            }

            var commentWorklogs = await GetCommentRawIssueWorklogsAsync(query, cancellationToken);
            result.AddRange(commentWorklogs);
            return result.Where(item => item.Author == userProfile.Username);
        }

        /// <summary>
        /// Get raw issue worklogs
        /// </summary>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IEnumerable<RawIssueWorklog>> GetCommentRawIssueWorklogsAsync(
            GetRawIssueWorklogs.Query query,
            CancellationToken cancellationToken = default)
        {
            var issueQuery = _queryFactory.Create()
                .Where("updatedDate", JiraQueryComparisonType.GreaterOrEqual, query.StartDate)
                .Where("watcher", JiraQueryComparisonType.Equal, JiraQueryMacros.CurrentUser)
                .Where("assignee", JiraQueryComparisonType.NotEqual, JiraQueryMacros.CurrentUser)
                .OrderBy("updatedDate", JiraQueryOrderType.Desc)
                .ToString();
            var issueSearchOptions = new IssueSearchOptions(issueQuery)
            {
                MaxIssuesPerRequest = JiraConstants.DefaultMaxIssuesPerRequest
            };

            var issues = await _jiraService.GetIssuesAsync(issueSearchOptions, cancellationToken);

            var user = await _identityService.GetCurrentUserAsync();
            var userProfile = await _userProfileStorage.GetValueAsync(user.Key, cancellationToken);

            var filter = new Func<Comment, bool>(comment =>
                comment.Author == userProfile.Username
                && comment.CreatedDate.Value >= query.StartDate
                && comment.CreatedDate.Value <= query.EndDate);

            var comments = await _jiraService.GetIssueCommentsAsync(issues, filter, cancellationToken);

            var result = new List<RawIssueWorklog> { };
            foreach (var issue in issues)
            {
                var rawIssueWorklogs = comments
                    .Where(item => item.Issue.Key == issue.Key)
                    .OrderBy(item => item.CreatedDate)
                    .ToList()
                    .ConvertTo<RawIssueWorklog>(_timeProvider, userProfile.TimeZoneInfo, WorklogSource.Comment, query.CommentWorklogTime)
                    .Where(issueWorklog => issueWorklog.IsBetween(query.StartDate, query.EndDate));
                result.AddRange(rawIssueWorklogs);
            }

            return result.Where(item => item.Author == userProfile.Username);
        }
    }
}
