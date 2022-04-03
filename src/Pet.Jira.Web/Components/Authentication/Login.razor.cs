using Microsoft.AspNetCore.Components;
using MudBlazor;
using Pet.Jira.Application.Authentication;
using Pet.Jira.Web.Authentication;
using System;
using System.Threading.Tasks;

namespace Pet.Jira.Web.Components.Authentication
{
    public partial class Login : ComponentBase
    {
        [Inject] 
        private NavigationManager _navigationManager { get; set; }

        [Inject] 
        private IAuthenticationService _authenticationService { get; set; }

        [Inject]
        public ISnackbar _snackbar { get; set; }

        public LoginRequest LoginRequest = new LoginRequest();

        private async Task LoginUser()
        {
            var loginResponse = await _authenticationService.LoginAsync(LoginRequest);
            if (loginResponse.IsSuccess)
            {
                Guid authenticationKey = Guid.NewGuid();
                AuthenticationMiddleware.Logins[authenticationKey] = LoginRequest;
                _navigationManager.NavigateTo($"/login?key={authenticationKey}", true);
            }
            else
            {
                _snackbar.Add(
                    "Authentication error",
                    Severity.Error,
                    config => { config.ActionColor = Color.Error; });
            }
        }
    }
}
