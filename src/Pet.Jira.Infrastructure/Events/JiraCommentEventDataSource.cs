using Atlassian.Jira;
using Pet.Jira.Application.Authentication;
using Pet.Jira.Application.Events;
using Pet.Jira.Application.Storage;
using Pet.Jira.Domain.Models.Events;
using Pet.Jira.Domain.Models.Users;
using Pet.Jira.Infrastructure.Jira;
using Pet.Jira.Infrastructure.Jira.Query;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Infrastructure.Events
{
    public class JiraCommentEventDataSource : IEventDataSource
    {
        private readonly IJiraService _jiraService;
        private readonly IJiraQueryFactory _queryFactory;
        private readonly IIdentityService _identityService;
        private readonly IStorage<string, UserProfile> _userProfileStorage;

        public JiraCommentEventDataSource(
            IJiraService jiraService,
            IJiraQueryFactory queryFactory,
            IIdentityService identityService,
            IStorage<string, UserProfile> userProfileStorage)
        {
            _jiraService = jiraService;
            _queryFactory = queryFactory;
            _identityService = identityService;
            _userProfileStorage = userProfileStorage;
        }

        public async Task<IReadOnlyList<Domain.Models.Events.Event>> GetEventsAsync(
            DateOnly from,
            DateOnly to,
            CancellationToken ct)
        {
            var fromDateTime = from.ToDateTime(TimeOnly.MinValue);
            var toDateTime = to.ToDateTime(TimeOnly.MaxValue);

            var issueQuery = _queryFactory.Create()
                .Where("updatedDate", JiraQueryComparisonType.GreaterOrEqual, fromDateTime)
                .Where("watcher", JiraQueryComparisonType.Equal, JiraQueryMacros.CurrentUser)
                .OrderBy("updatedDate", JiraQueryOrderType.Desc)
                .ToString();

            var issueSearchOptions = new IssueSearchOptions(issueQuery)
            {
                MaxIssuesPerRequest = JiraConstants.DefaultMaxIssuesPerRequest
            };

            var issues = await _jiraService.GetIssuesAsync(issueSearchOptions, ct);

            var user = await _identityService.GetCurrentUserAsync();
            var userProfile = await _userProfileStorage.GetValueAsync(user.Key, ct);

            var commentFilter = new Func<Comment, bool>(comment =>
                comment.Author == userProfile!.Username
                && comment.CreatedDate.Value >= fromDateTime
                && comment.CreatedDate.Value <= toDateTime);

            var comments = await _jiraService.GetIssueCommentsAsync(issues, commentFilter, ct);

            var events = new List<Domain.Models.Events.Event>();
            foreach (var comment in comments)
            {
                events.Add(new Domain.Models.Events.Event(
                    Start: comment.CreatedDate,
                    End: comment.CreatedDate,
                    Title: comment.Issue.Name,
                    Key: null,
                    Description: comment.Body,
                    Link: comment.Issue.Link != null ? new Uri(comment.Issue.Link) : null,
                    Issue: comment.Issue.Adapt(),
                    Source: EventSource.Comment));
            }

            return events;
        }
    }
}
