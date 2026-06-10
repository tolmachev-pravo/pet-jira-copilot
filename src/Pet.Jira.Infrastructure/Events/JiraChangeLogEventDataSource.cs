using Atlassian.Jira;
using Pet.Jira.Application.Events;
using Pet.Jira.Domain.Models.Events;
using Pet.Jira.Infrastructure.Jira;
using Pet.Jira.Infrastructure.Jira.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Infrastructure.Events
{
    public abstract class JiraChangeLogEventDataSource : IEventDataSource
    {
        private readonly IJiraService _jiraService;
        private readonly IJiraQueryFactory _queryFactory;

        protected JiraChangeLogEventDataSource(
            IJiraService jiraService,
            IJiraQueryFactory queryFactory)
        {
            _jiraService = jiraService;
            _queryFactory = queryFactory;
        }

        protected abstract string AssigneeField { get; }

        protected abstract Func<IssueChangeLogItem, bool> ChangeLogItemFilter { get; }

        public async Task<IReadOnlyList<Domain.Models.Events.Event>> GetEventsAsync(
            DateOnly from,
            DateOnly to,
            CancellationToken cancellationToken)
        {
            var fromDateTime = from.ToDateTime(TimeOnly.MinValue);
            var toDateTime = to.ToDateTime(TimeOnly.MaxValue);

            var issueQuery = _queryFactory.Create()
                .Where("updatedDate", JiraQueryComparisonType.GreaterOrEqual, fromDateTime)
                .Where(AssigneeField, JiraQueryComparisonType.Equal, JiraQueryMacros.CurrentUser)
                .Where("type", JiraQueryComparisonType.NotEqual, "Story")
                .OrderBy("updatedDate", JiraQueryOrderType.Desc)
                .ToString();

            var issueSearchOptions = new IssueSearchOptions(issueQuery)
            {
                MaxIssuesPerRequest = JiraConstants.DefaultMaxIssuesPerRequest
            };

            var issues = await _jiraService.GetIssuesAsync(issueSearchOptions, cancellationToken);

            var changeLogFilter = new Func<IssueChangeLog, bool>(changeLog =>
                changeLog.Items.Any(item => item.FieldName == JiraConstants.Status.FieldName));

            var changeLogItems = await _jiraService.GetIssueChangeLogItemsAsync(
                issues, changeLogFilter, ChangeLogItemFilter, cancellationToken);

            var events = new List<Domain.Models.Events.Event>();
            foreach (var issue in issues)
            {
                var issueItems = changeLogItems
                    .Where(item => item.ChangeLog.Issue.Key == issue.Key)
                    .OrderBy(item => item.ChangeLog.CreatedDate)
                    .ToList();

                for (int i = 0; i < issueItems.Count - 1; i++)
                {
                    var start = issueItems[i].ChangeLog.CreatedDate;
                    var end = issueItems[i + 1].ChangeLog.CreatedDate;

                    if (end >= fromDateTime && start <= toDateTime)
                    {
                        events.Add(new Domain.Models.Events.Event(
                            Start: start,
                            End: end,
                            Title: issue.Name,
                            Key: null,
                            Description: null,
                            Link: issue.Link != null ? new Uri(issue.Link) : null,
                            Issue: issue.Adapt(),
                            Source: EventSource.Task));
                    }
                }
            }

            return events;
        }
    }
}
