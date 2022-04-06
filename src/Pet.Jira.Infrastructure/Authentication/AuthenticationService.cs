using Pet.Jira.Application.Authentication;
using Pet.Jira.Infrastructure.Jira;
using System.Threading.Tasks;

namespace Pet.Jira.Infrastructure.Authentication
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IJiraService _jiraService;

        public AuthenticationService(IJiraService jiraService)
        {
            _jiraService = jiraService;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            return await _jiraService.LoginAsync(request);
        }
    }
}
