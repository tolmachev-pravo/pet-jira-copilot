using Atlassian.Jira;
using Pet.Jira.Application.Authentication;
using Pet.Jira.Application.Worklogs.Dto;
using Pet.Jira.Infrastructure.Jira.Dto;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Infrastructure.Jira
{
    public interface IJiraService
    {
        Task AddWorklogAsync(AddedWorklogDto worklog);
        Task<LoginResponse> Login(LoginRequest request);
        Task<string> GetCurrentUserAvatar(CancellationToken cancellationToken = default);

        Task<IEnumerable<IssueDto>> GetIssuesAsync(
            IssueSearchOptions issueSearchOptions,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<IssueWorklogDto>> GetIssueWorklogsAsync(
            IssueSearchOptions issueSearchOptions,
            Func<Worklog, bool> worklogFilter = null,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<IssueWorklogDto>> GetIssueWorklogsAsync(
            IEnumerable<IssueDto> issues,
            Func<Worklog, bool> worklogFilter = null,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<IssueChangeLogDto>> GetIssueChangeLogsAsync(
            IEnumerable<IssueDto> issues,
            Func<IssueChangeLog, bool> changeLogFilter = null,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<IssueChangeLogItemDto>> GetIssueChangeLogItemsAsync(
            IEnumerable<IssueDto> issues,
            Func<IssueChangeLog, bool> changeLogFilter = null,
            Func<IssueChangeLogItem, bool> changeLogItemFilter = null,
            CancellationToken cancellationToken = default);

        Task<UserDto> GetCurrentUserAsync(
            CancellationToken cancellationToken = default);
    }
}
