using Pet.Jira.Application.Worklogs;
using Pet.Jira.Application.Worklogs.Queries;
using Pet.Jira.Domain.Models.Worklogs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Infrastructure.Mock
{
    internal class MockWorklogDataSource : IWorklogDataSource
    {
        public async Task<IEnumerable<IssueWorklog>> GetIssueWorklogsAsync(GetIssueWorklogs.Query query, CancellationToken cancellationToken = default)
        {
            return MockWorklogStorage.IssueWorklogs;
        }

        public async Task<IEnumerable<RawIssueWorklog>> GetRawIssueWorklogsAsync(GetRawIssueWorklogs.Query query, CancellationToken cancellationToken = default)
        {
            return MockWorklogStorage.RawIssueWorklogs;
        }
    }
}
