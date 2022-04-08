using Pet.Jira.Application.Authentication;
using System.Threading.Tasks;

namespace Pet.Jira.Infrastructure.Mock
{
    internal class MockAuthenticationService : IAuthenticationService
    {
        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            return new LoginResponse(true);
        }
    }
}
