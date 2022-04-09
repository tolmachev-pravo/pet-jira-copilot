using Pet.Jira.Application.Worklogs;
using Pet.Jira.Application.Worklogs.Queries;
using Pet.Jira.Domain.Models.Worklogs;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Infrastructure.Mock
{
    internal class MockWorklogDataSource : IWorklogDataSource
    {
        public Task<IEnumerable<IssueWorklog>> GetIssueWorklogsAsync(GetIssueWorklogs.Query query, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(MockWorklogStorage.IssueWorklogs.AsEnumerable());
        }

        public Task<IEnumerable<RawIssueWorklog>> GetRawIssueWorklogsAsync(GetRawIssueWorklogs.Query query, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(MockWorklogStorage.RawIssueWorklogs.AsEnumerable());
        }
    }
}
