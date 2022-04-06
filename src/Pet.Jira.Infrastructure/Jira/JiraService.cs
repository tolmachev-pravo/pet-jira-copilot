using Atlassian.Jira;
using Microsoft.Extensions.Options;
using Pet.Jira.Application.Authentication;
using Pet.Jira.Application.Extensions;
using Pet.Jira.Application.Worklogs.Dto;
using Pet.Jira.Domain.Models.Users;
using Pet.Jira.Infrastructure.Jira.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Infrastructure.Jira
{
    public class JiraService : IJiraService
    {
        private readonly IJiraLinkGenerator _linkGenerator;
        private readonly Atlassian.Jira.Jira _jiraClient;
        private readonly IJiraConfiguration _config;
        private readonly User _user;

        public JiraService(
            IOptions<JiraConfiguration> jiraConfiguration,
            IJiraLinkGenerator linkGenerator,
            IIdentityService identityService)
        {
            _linkGenerator = linkGenerator;
            _config = jiraConfiguration.Value;
            _user = identityService.CurrentUser;
            _jiraClient = Atlassian.Jira.Jira.CreateRestClient(_config.Url, _user.Username, _user.Password);
        }

        /// <summary>
        /// Get issues
        /// </summary>
        /// <param name="issueSearchOptions"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IEnumerable<IssueDto>> GetIssuesAsync(
            IssueSearchOptions issueSearchOptions,
            CancellationToken cancellationToken = default)
        {
            var issues = await _jiraClient.Issues.GetIssuesFromJqlAsync(issueSearchOptions, cancellationToken);
            return issues.Select(record => IssueDto.Create(record, _linkGenerator));
        }

        /// <summary>
        /// Get issue change logs
        /// </summary>
        /// <param name="issueSearchOptions"></param>
        /// <param name="changeLogFilter"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IEnumerable<IssueChangeLogDto>> GetIssueChangeLogsAsync(
            IssueSearchOptions issueSearchOptions,
            Func<IssueChangeLog, bool> changeLogFilter = null,
            CancellationToken cancellationToken = default)
        {
            var issues = await GetIssuesAsync(issueSearchOptions, cancellationToken);
            return await GetIssueChangeLogsAsync(issues, changeLogFilter, cancellationToken);
        }

        /// <summary>
        /// Get issue change logs
        /// </summary>
        /// <param name="issues"></param>
        /// <param name="changeLogFilter"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IEnumerable<IssueChangeLogDto>> GetIssueChangeLogsAsync(
            IEnumerable<IssueDto> issues,
            Func<IssueChangeLog, bool> changeLogFilter = null,
            CancellationToken cancellationToken = default)
        {
            var result = new List<IssueChangeLogDto> { };
            foreach (var issue in issues)
            {
                var issueChangeLogs = await _jiraClient.Issues.GetChangeLogsAsync(issue.Key, cancellationToken);
                issueChangeLogs.WhereIfNotNull(changeLogFilter);

                result.AddRange(issueChangeLogs.Select(issueChangeLog =>
                    IssueChangeLogDto.Create(issueChangeLog, issue)));
            }

            return result;
        }

        /// <summary>
        /// Get issue worklogs
        /// </summary>
        /// <param name="issueSearchOptions"></param>
        /// <param name="worklogFilter"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IEnumerable<IssueWorklogDto>> GetIssueWorklogsAsync(
            IssueSearchOptions issueSearchOptions,
            Func<Worklog, bool> worklogFilter = null,
            CancellationToken cancellationToken = default)
        {
            var issues = await GetIssuesAsync(issueSearchOptions, cancellationToken);
            return await GetIssueWorklogsAsync(issues, worklogFilter, cancellationToken);
        }

        /// <summary>
        /// Get issue worklogs
        /// </summary>
        /// <param name="issues"></param>
        /// <param name="worklogFilter"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IEnumerable<IssueWorklogDto>> GetIssueWorklogsAsync(
            IEnumerable<IssueDto> issues,
            Func<Worklog, bool> worklogFilter = null,
            CancellationToken cancellationToken = default)
        {
            var result = new List<IssueWorklogDto> { };
            foreach (var issue in issues)
            {
                var issueWorklogs = await _jiraClient.Issues.GetWorklogsAsync(issue.Key, cancellationToken);
                issueWorklogs = issueWorklogs.WhereIfNotNull(worklogFilter);

                result.AddRange(issueWorklogs.Select(issueWorklog => 
                    IssueWorklogDto.Create(issueWorklog, issue)));
            }

            return result;
        }

        /// <summary>
        /// Get issue change log items
        /// </summary>
        /// <param name="issues"></param>
        /// <param name="changeLogFilter"></param>
        /// <param name="changeLogItemFilter"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IEnumerable<IssueChangeLogItemDto>> GetIssueChangeLogItemsAsync(
            IEnumerable<IssueDto> issues,
            Func<IssueChangeLog, bool> changeLogFilter = null,
            Func<IssueChangeLogItem, bool> changeLogItemFilter = null,
            CancellationToken cancellationToken = default)
        {
            var result = new List<IssueChangeLogItemDto> { };
            foreach (var issue in issues)
            {
                var issueChangeLogs = await _jiraClient.Issues.GetChangeLogsAsync(issue.Key, cancellationToken);
                issueChangeLogs.WhereIfNotNull(changeLogFilter);

                foreach (var issueChangeLog in issueChangeLogs)
                {
                    var issueChangeLogItems = issueChangeLog.Items;
                    issueChangeLogItems = issueChangeLogItems.WhereIfNotNull(changeLogItemFilter);

                    result.AddRange(issueChangeLogItems.Select(issueChangeLogItem =>
                        new IssueChangeLogItemDto
                        {
                            FromId = issueChangeLogItem.FromId,
                            ToId = issueChangeLogItem.ToId,
                            ChangeLog = IssueChangeLogDto.Create(issueChangeLog, issue)
                        }));
                }
            }

            return result;
        }

        /// <summary>
        /// Get current user
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<UserDto> GetCurrentUserAsync(
            CancellationToken cancellationToken = default)
        {
            var myself = await _jiraClient.Users.GetMyselfAsync(cancellationToken);
            return new UserDto
            {
                Username = myself.Username
            };
        }

        /// <summary>
        /// Add worklog
        /// </summary>
        /// <param name="worklogDto"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task AddWorklogAsync(
            AddedWorklogDto worklogDto,
            CancellationToken cancellationToken = default)
        {
            var minutesLag = worklogDto.ElapsedTime.Seconds >= 30 ? 1 : 0;
            var worklog = new Worklog(
                $"{worklogDto.ElapsedTime.Hours}h {worklogDto.ElapsedTime.Minutes + minutesLag}m",
                worklogDto.StartedAt,
                "Dev");
            await _jiraClient.Issues.AddWorklogAsync(worklogDto.IssueKey, worklog, token: cancellationToken);
        }

        /// <summary>
        /// Login
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<LoginResponse> LoginAsync(
            LoginRequest request, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var jiraClient = Atlassian.Jira.Jira.CreateRestClient(_config.Url, request.Username, request.Password);
                await jiraClient.ServerInfo.GetServerInfoAsync(token: cancellationToken);
                return new LoginResponse(true);
            }
            catch (Exception e)
            {
                return new LoginResponse(false, e.Message);
            }
        }

        public async Task<string> GetCurrentUserAvatarAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var myself = await _jiraClient.Users.GetMyselfAsync(cancellationToken);
                var avatarUrl = myself.AvatarUrls.Small;
                var avatar = _jiraClient.RestClient.DownloadData(avatarUrl);
                string img64 = Convert.ToBase64String(avatar);
                string urlData = string.Format("data:image/jpg;base64, {0}", img64);
                return urlData;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
