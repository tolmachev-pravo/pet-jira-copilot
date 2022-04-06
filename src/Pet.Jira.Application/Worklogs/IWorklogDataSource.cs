using Pet.Jira.Application.Worklogs.Queries;
using Pet.Jira.Domain.Models.Worklogs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Application.Worklogs
{
    public interface IWorklogDataSource
    {
        Task<IEnumerable<IssueWorklog>> GetIssueWorklogsAsync(
            GetIssueWorklogs.Query query,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<RawIssueWorklog>> GetRawIssueWorklogsAsync(
            GetRawIssueWorklogs.Query query,
            CancellationToken cancellationToken = default);
    }
}
