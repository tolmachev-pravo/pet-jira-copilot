using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Pet.Jira.Application.Authentication;
using Pet.Jira.Application.Authentication.Dto;
using Pet.Jira.Domain.Models.Users;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Pet.Jira.Web.Authentication
{
    public class IdentityService : IIdentityService
    {
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public IdentityService(
            AuthenticationStateProvider authenticationStateProvider,
            IHttpContextAccessor httpContextAccessor)
        {
            _authenticationStateProvider = authenticationStateProvider;
            _httpContextAccessor = httpContextAccessor;
        }

        public User CurrentUser => CreateUser(_httpContextAccessor.HttpContext?.User);

        public async Task<User> GetCurrentUserAsync()
        {
            var httpContextUser = CreateUser(_httpContextAccessor.HttpContext?.User);
            if (httpContextUser != null)
            {
                return httpContextUser;
            }

            var authenticationState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            return CreateUser(authenticationState.User);
        }

        private static User CreateUser(ClaimsPrincipal user)
        {
            return user?.Identity?.IsAuthenticated == true
                ? new User
                {
                    Username = user.Identity.Name,
                    Password = user.Claims
                        .FirstOrDefault(claim => claim.Type == ClaimTypes.UserData)?
                        .Value,
                    PersonalAccessToken = user.Claims
                        .FirstOrDefault(claim => claim.Type == nameof(LoginDto.PersonalAccessToken))?
                        .Value
                }
                : null;
        }
    }
}
