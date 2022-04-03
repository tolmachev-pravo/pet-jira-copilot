using System.Threading.Tasks;

namespace Pet.Jira.Application.Authentication
{
    public interface IAuthenticationService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);
    }
}
