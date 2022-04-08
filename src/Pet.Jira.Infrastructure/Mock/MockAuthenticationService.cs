using Pet.Jira.Application.Authentication;
using System.Threading.Tasks;

namespace Pet.Jira.Infrastructure.Mock
{
    internal class MockAuthenticationService : IAuthenticationService
    {
        public Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            return Task.FromResult(new LoginResponse(true));
        }
    }
}
