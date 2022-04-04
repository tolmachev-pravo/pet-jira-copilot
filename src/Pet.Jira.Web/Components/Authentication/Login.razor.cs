using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
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
        private ComponentModel _componentModel = new ComponentModel();

        public async Task OnKeyUp(KeyboardEventArgs keyboardEventArgs)
        {
            switch (keyboardEventArgs.Code)
            {
                case "Enter":
                case "NumpadEnter":
                    await LoginUser();
                    break;
            }
        }

        private async Task LoginUser()
        {
            _componentModel.State = ComponentState.InProgress;

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

            _componentModel.State = ComponentState.Success;
        }

        private class ComponentModel
        {
            public ComponentState State { get; set; }
            public bool InProgress => State == ComponentState.InProgress;
        }

        private enum ComponentState
        {
            Unknown,
            InProgress,
            Success
        }
    }
}
