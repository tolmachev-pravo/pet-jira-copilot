using Pet.Jira.Application.Authentication;
using System.Threading.Tasks;

namespace Pet.Jira.Infrastructure.Mock
{
    internal class MockAuthenticationService : IAuthenticationService
    {
        public Task<LoginResponse> LoginAsync(BasicLoginRequest request)
        {
            return Task.FromResult(new LoginResponse(true));
        }

        public Task<LoginResponse> LoginAsync(BearerLoginRequest request)
        {
            return Task.FromResult(new LoginResponse(true) { Username = request.Token });
        }
    }
}
