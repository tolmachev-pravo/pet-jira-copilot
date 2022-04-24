using Pet.Jira.Application.Issues;
using Pet.Jira.Application.Issues.Queries;
using Pet.Jira.Domain.Models.Issues;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Infrastructure.Mock
{
    internal class MockIssueDataSource : IIssueDataSource
    {
        public async Task<IEnumerable<IssueStatus>> GetIssueStatusesAsync(GetIssueStatuses.Query query, CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(MockIssueStorage.IssueStatuses);
        }
    }
}
