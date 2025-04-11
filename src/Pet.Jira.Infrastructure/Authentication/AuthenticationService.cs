using Pet.Jira.Application.Authentication;
using Pet.Jira.Application.Users;
using Pet.Jira.Application.Users.Commands;
using Pet.Jira.Infrastructure.Jira;
using System.Threading.Tasks;

namespace Pet.Jira.Infrastructure.Authentication
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IJiraService _jiraService;
        private readonly IUserDataSource _userDataSource;
        private readonly IUserRepository _userRepository;

        public AuthenticationService(
            IJiraService jiraService,
            IUserDataSource userDataSource,
            IUserRepository userRepository)
        {
            _jiraService = jiraService;
            _userDataSource = userDataSource;
            _userRepository = userRepository;
        }

        public async Task<LoginResponse> LoginAsync(BasicLoginRequest request)
        {
            var result = await _jiraService.LoginAsync(request);
			await AddUserIfNeed(result);
			return result;
        }

        public async Task<LoginResponse> LoginAsync(BearerLoginRequest request)
        {
            var result = await _jiraService.LoginAsync(request);
			await AddUserIfNeed(result);
			return result;
        }

        private async Task AddUserIfNeed(LoginResponse loginResponse)
        {
            if (loginResponse.IsSuccess)
            {
                var username = loginResponse.Username;

				var user = await _userDataSource.GetUserAsync(username);
                if (user != null)
                {
					await _userRepository.AddAsync(
                        new CreateUserCommand(username));
                }
            }
        }
    }
}
