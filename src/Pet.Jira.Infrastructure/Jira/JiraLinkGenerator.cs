using Microsoft.Extensions.Options;
using Pet.Jira.Application.Extensions;

namespace Pet.Jira.Infrastructure.Jira
{
    public class JiraLinkGenerator : IJiraLinkGenerator
    {
        private readonly IJiraConfiguration _config;

        public JiraLinkGenerator(IOptions<JiraConfiguration> jiraConfiguration)
        {
            _config = jiraConfiguration.Value;
        }

        public string Generate(string issueKey) => _config.Url.AppendUrl("browse", issueKey);
    }
}
