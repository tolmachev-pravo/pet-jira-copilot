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
    }
}
