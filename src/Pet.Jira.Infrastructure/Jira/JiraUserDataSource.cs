using Pet.Jira.Application.Authentication;
using Pet.Jira.Application.Users;
using Pet.Jira.Domain.Models.Users;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Infrastructure.Jira
{
    internal class JiraUserDataSource : IUserDataSource
    {
        private readonly IJiraService _jiraService;
        private readonly IIdentityService _identityService;
        private readonly IUserStorage _userStorage;

        public JiraUserDataSource(
            IJiraService jiraService,
            IIdentityService identityService,
            IUserStorage userStorage)
        {
            _jiraService = jiraService;
            _identityService = identityService;
            _userStorage = userStorage;
        }

        public async Task<User> GetCurrentUserAsync(CancellationToken cancellationToken = default)
        {
            var userName = _identityService.CurrentUser.Username;
            if (_userStorage.TryGetValue(userName, out var result))
            {
                return result;
            }
            else
            {
                var userDto = await _jiraService.GetCurrentUserAsync(cancellationToken);
                var user = userDto.ConvertToUser();
                _userStorage.TryAdd(user);
                return user;
            }            
        }
    }
}
