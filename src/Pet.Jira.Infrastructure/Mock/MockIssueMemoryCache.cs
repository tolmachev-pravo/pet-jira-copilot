using Microsoft.Extensions.Options;
using Pet.Jira.Application.Common.Extensions;
using Pet.Jira.Application.Storage;
using Pet.Jira.Domain.Models.Issues;
using Pet.Jira.Infrastructure.Jira;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Pet.Jira.Infrastructure.Mock
{
    public class MockIssueMemoryCache : BaseMemoryCache<string, Issue>
    {
        private readonly IJiraConfiguration _jiraConfiguration;

        public MockIssueMemoryCache(
            IOptions<JiraConfiguration> jiraConfiguration)
        {
            _jiraConfiguration = jiraConfiguration.Value;
        }

        public override async Task<ConcurrentDictionary<string, Issue>> GetValuesAsync()
        {
            var storage = await base.GetValuesAsync();
            if (storage.IsEmpty() && !_jiraConfiguration.CachedIssues.IsEmpty())
            {
                var issues = _jiraConfiguration.CachedIssues;
                foreach (var issue in issues)
                {
                    TryAdd(new Issue { Key = issue, Summary = TextGenerator.Create(maxWords: 10, maxLetters: 10) });
                }
            }
            return await base.GetValuesAsync();
        }
    }
}
