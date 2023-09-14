using Pet.Jira.Application.Issues;
using Pet.Jira.Application.Issues.Queries;
using Pet.Jira.Domain.Models.Issues;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Infrastructure.Jira
{
    public class JiraIssueDataSource : IIssueDataSource
    {
        private readonly IJiraService _jiraService;

		public JiraIssueDataSource(
            IJiraService jiraService)
        {
            _jiraService = jiraService;
		}

        public async Task<string> GetIssueOpenPullRequestUrlAsync(
            GetIssueOpenPullRequestUrl.Query query, CancellationToken cancellationToken = default)
        {
            var devStatusDetail = await _jiraService.GetIssueDevStatusDetailAsync(query.Identifier, cancellationToken: cancellationToken);
            if (devStatusDetail?.Detail == null)
            {
                return null;
            }

            var detail = devStatusDetail.Detail;
            var githubApplication = devStatusDetail.Detail
                .FirstOrDefault(application => string.Equals(application.Instance.Id, "github", System.StringComparison.OrdinalIgnoreCase));
            if (githubApplication == null)
            {
                return null;
            }

            var openPullRequests = githubApplication.PullRequests
                .Where(pullRequest => string.Equals(pullRequest.Status, "OPEN", System.StringComparison.OrdinalIgnoreCase));
            if (openPullRequests.Count() == 0
                || openPullRequests.Count() > 1)
            {
                return null;
            }

            var pullRequest = openPullRequests.First();
            return pullRequest.Url.ToString();
        }

        public async Task<IEnumerable<IssueStatus>> GetIssueStatusesAsync(
            GetIssueStatuses.Query query,
            CancellationToken cancellationToken = default)
        {
            var issueStatuses = await _jiraService.GetIssueStatusesAsync(cancellationToken);
            return issueStatuses.Select(issueStatus => new IssueStatus
            {
                Id = issueStatus.Id,
                Name = issueStatus.Name
            });
        }

		public async Task<Issue> GetIssueAsync(
            string issueKey,
            CancellationToken cancellationToken = default)
		{
			var issue = await _jiraService.GetIssueAsync(issueKey, cancellationToken);
            return issue?.Adapt();
		}
	}
}
