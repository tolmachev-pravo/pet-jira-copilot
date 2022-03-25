using Microsoft.Extensions.Options;
using System.IO;

namespace Pet.Jira.Infrastructure.Jira
{
    public class JiraLinkGenerator
    {
        private readonly IJiraConfiguration _config;

        public JiraLinkGenerator(IOptions<JiraConfiguration> jiraConfiguration)
        {
            _config = jiraConfiguration.Value;
        }

        public string Generate(string issueKey) => Path.Combine(_config.Url, "browse", issueKey);
    }
}
