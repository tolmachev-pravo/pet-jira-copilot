using Pet.Jira.Application.Worklogs;
using Pet.Jira.Domain.Models.Worklogs;
using Pet.Jira.Infrastructure.Jira;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pet.Jira.Infrastructure.Worklogs
{
    public class WorklogDataSource : IWorklogDataSource
    {
        private readonly IJiraService _jiraService;

        public WorklogDataSource(IJiraService jiraService)
        {
            _jiraService = jiraService;
        }

        public Task<IEnumerable<DailyWorklogSummary>> GetUserDayWorklogs(DateTime fromDate, DateTime toDate, int issueCount)
        {
            return _jiraService.GetUserDayWorklogs(fromDate, toDate, issueCount);
        }
    }
}
