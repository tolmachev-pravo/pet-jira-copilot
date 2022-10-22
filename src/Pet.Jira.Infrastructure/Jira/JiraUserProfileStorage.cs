using Pet.Jira.Domain.Models.Users;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Infrastructure.Jira
{
    internal class JiraUserProfileStorage
    {
        private readonly IJiraService _jiraService;

        public JiraUserProfileStorage(IJiraService jiraService)
        {
            _jiraService = jiraService;
        }

        public async Task<UserProfile> GetValueAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _jiraService.GetCurrentUserAsync(cancellationToken);
                return user.ConvertToUserProfile();
            }
            catch
            {
                return default;
            }
        }

        public Task<bool> TryAddAsync(UserProfile entity, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }

        public Task<bool> TryRemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }

        public Task<bool> TryUpdateAsync(string key, UserProfile newEntity, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }
    }
}
