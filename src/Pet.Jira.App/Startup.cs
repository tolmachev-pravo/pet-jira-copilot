using Microsoft.Extensions.Logging;
using Pet.Jira.Adapter;
using System.Threading.Tasks;

namespace Pet.Jira.App
{
    public class Startup : IStartup
    {
        private readonly ILogger _logger;
        private readonly JiraService _jiraService;

        public Startup(
            ILogger<Startup> logger,
            JiraService jiraService)
        {
            _logger = logger;
            _jiraService = jiraService;
        }

        public async Task Run()
        {
            await _jiraService.GetCurrentUserEstimatedWorklogsAsync("-28d", 40);
        }
    }
}
