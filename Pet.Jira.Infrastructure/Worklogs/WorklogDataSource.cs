using Pet.Jira.Application.Worklogs;
using Pet.Jira.Infrastructure.Jira;

namespace Pet.Jira.Infrastructure.Worklogs
{
    public class WorklogDataSource : IWorklogDataSource
    {
        private readonly IJiraService _jiraService;

        public WorklogDataSource(IJiraService jiraService)
        {
            _jiraService = jiraService;
        }
    }
}
