using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Pet.Jira.App
{
    public class Startup : IStartup
    {
        private readonly ILogger _logger;
        private readonly JiraService _jiraService;

        public bool IsStarted { get; set; }

        public Startup(
            ILogger<Startup> logger,
            JiraService jiraService)
        {
            _logger = logger;
            _jiraService = jiraService;
        }

        public async Task Run()
        {
            await _jiraService.Test();
        }
    }
}
