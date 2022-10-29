using Microsoft.AspNetCore.Components.Authorization;
using Pet.Jira.Application.Authentication;
using Pet.Jira.Domain.Models.Users;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Pet.Jira.Web.Authentication
{
    public class IdentityService : IIdentityService
    {
        private readonly AuthenticationStateProvider _authenticationStateProvider;

        public IdentityService(AuthenticationStateProvider authenticationStateProvider)
        {
            _authenticationStateProvider = authenticationStateProvider;
        }

        public User CurrentUser => GetCurrentUserAsync().GetAwaiter().GetResult();

        public async Task<User> GetCurrentUserAsync()
        {
            var authenticationState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            var user = authenticationState.User;
            return user.Identity.IsAuthenticated
                ? new User
                {
                    Username = user.Identity.Name,
                    Password = user.Claims
                        .FirstOrDefault(claim => claim.Type == ClaimTypes.UserData)?
                        .Value
                }
                : null;
        }
    }
}
