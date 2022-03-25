using Pet.Jira.Application.Worklogs;
using Pet.Jira.Application.Worklogs.Dto;
using Pet.Jira.Infrastructure.Jira;
using System.Threading.Tasks;

namespace Pet.Jira.Infrastructure.Worklogs
{
    public class WorklogRepository : IWorklogRepository
    {
        private readonly IJiraService _jiraService;

        public WorklogRepository(IJiraService jiraService)
        {
            _jiraService = jiraService;
        }

        public async Task AddAsync(AddedWorklogDto worklog)
        {
            await _jiraService.AddWorklogAsync(worklog);
        }
    }
}
