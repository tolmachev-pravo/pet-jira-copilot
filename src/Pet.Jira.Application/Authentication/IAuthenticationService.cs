using System.Threading.Tasks;

namespace Pet.Jira.Application.Authentication
{
    public interface IAuthenticationService
    {
        Task<LoginResponse> LoginAsync(BasicLoginRequest request);
        Task<LoginResponse> LoginAsync(BearerLoginRequest request);
    }
}
