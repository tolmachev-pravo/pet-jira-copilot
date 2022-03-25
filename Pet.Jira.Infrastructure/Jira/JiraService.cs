using Atlassian.Jira;
using Microsoft.Extensions.Options;
using Pet.Jira.Domain.Models.Worklogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pet.Jira.Infrastructure.Jira
{
    public class JiraService : IJiraService
    {
        private readonly JiraLinkGenerator _linkGenerator;
        private readonly Atlassian.Jira.Jira _jiraClient;
        private readonly IJiraConfiguration _config;

        public JiraService(
            IOptions<JiraConfiguration> jiraConfiguration,
            JiraLinkGenerator linkGenerator)
        {
            _linkGenerator = linkGenerator;
            _config = jiraConfiguration.Value;
            _jiraClient = Atlassian.Jira.Jira.CreateRestClient(_config.Url, _config.Username, _config.Password);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="period"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        // ToDo: Добавить передачу фильтра для Issue (нужен генератор запросов) + фильтрация для Worklog
        public async Task<IEnumerable<ActualWorklog>> GetActualWorklogs(
            string period,
            int count)
        {
            var issues = await GetIssues(
                jql: $"worklogDate >= '{period}' AND worklogAuthor = currentUser() ORDER BY updatedDate DESC",
                count: count);

            var actualWorklogs = new List<ActualWorklog> { };
            foreach (var issue in issues)
            {
                var issueWorklogs = await _jiraClient.Issues.GetWorklogsAsync(issue.Key.Value);
                var userIssueWorklogs = issueWorklogs.Where(record => record.Author == _config.Username).ToList();
                actualWorklogs.AddRange(userIssueWorklogs.Select(worklog => new ActualWorklog
                {

                    StartedAt = worklog.StartDate.Value,
                    ElapsedTime = TimeSpan.FromSeconds(worklog.TimeSpentInSeconds),
                    Issue = new Domain.Models.Issues.Issue
                    {
                        Key = issue.Key.Value,
                        Summary = issue.Summary,
                        Link = _linkGenerator.Generate(issue.Key.Value)
                    }
                }));
            }
            return actualWorklogs;
        }

        private async Task<IPagedQueryResult<Atlassian.Jira.Issue>> GetIssues(string jql, int count) =>
            await _jiraClient.Issues.GetIssuesFromJqlAsync(
                new IssueSearchOptions(jql)
                {
                    MaxIssuesPerRequest = count,
                    ValidateQuery = true
                });
    }
}
