using Pet.Jira.Application.Issues.Queries;
using Pet.Jira.Domain.Models.Issues;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Application.Issues
{
    public interface IIssueDataSource
    {
        Task<IEnumerable<IssueStatus>> GetIssueStatusesAsync(
            GetIssueStatuses.Query query,
            CancellationToken cancellationToken = default);

        Task<string> GetIssueOpenPullRequestUrlAsync(
            GetIssueOpenPullRequestUrl.Query query,
            CancellationToken cancellationToken = default);

		Task<Issue> GetIssueAsync(
            string issueKey,
            CancellationToken cancellationToken = default);
	}
}
