using Pet.Jira.Application.Authentication;
using Pet.Jira.Application.Users;
using Pet.Jira.Infrastructure.Jira;
using System.Threading.Tasks;

namespace Pet.Jira.Infrastructure.Authentication
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IJiraService _jiraService;
        private readonly IUserRepository _userRepository;

        public AuthenticationService(
            IJiraService jiraService,
            IUserRepository userRepository)
        {
            _jiraService = jiraService;
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
                await _userRepository.GetOrCreateByUsernameAsync(loginResponse.Username);
            }
        }
    }
}
