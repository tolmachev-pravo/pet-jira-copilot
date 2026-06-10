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
    public class JiraTesterEventDataSource : IEventDataSource
    {
        private const string InTestingStatusId = "10116";

        private readonly IJiraService _jiraService;
        private readonly IJiraQueryFactory _queryFactory;

        public JiraTesterEventDataSource(
            IJiraService jiraService,
            IJiraQueryFactory queryFactory)
        {
            _jiraService = jiraService;
            _queryFactory = queryFactory;
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
                .Where("Tester", JiraQueryComparisonType.Equal, JiraQueryMacros.CurrentUser)
                .Where("type", JiraQueryComparisonType.NotEqual, "Story")
                .OrderBy("updatedDate", JiraQueryOrderType.Desc)
                .ToString();

            var issueSearchOptions = new IssueSearchOptions(issueQuery)
            {
                MaxIssuesPerRequest = JiraConstants.DefaultMaxIssuesPerRequest
            };

            var issues = await _jiraService.GetIssuesAsync(issueSearchOptions, ct);

            var changeLogFilter = new Func<IssueChangeLog, bool>(changeLog =>
                changeLog.Items.Any(item => item.FieldName == JiraConstants.Status.FieldName));

            var changeLogItemFilter = new Func<IssueChangeLogItem, bool>(changeLogItem =>
                changeLogItem.FieldName == JiraConstants.Status.FieldName
                && (changeLogItem.ToId == InTestingStatusId
                    || changeLogItem.FromId == InTestingStatusId));

            var changeLogItems = await _jiraService.GetIssueChangeLogItemsAsync(
                issues, changeLogFilter, changeLogItemFilter, ct);

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
