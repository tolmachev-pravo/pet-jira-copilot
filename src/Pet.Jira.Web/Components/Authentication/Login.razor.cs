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
        [Inject] private NavigationManager NavigationManager { get; set; }
        [Inject] private IAuthenticationService AuthenticationService { get; set; }
        [Inject] public ISnackbar Snackbar { get; set; }

        private readonly ComponentModel Model = ComponentModel.Create();

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
            Model.State = ComponentModelState.InProgress;

            var loginResponse = await AuthenticationService.LoginAsync(Model.LoginRequest);
            if (loginResponse.IsSuccess)
            {
                Guid authenticationKey = Guid.NewGuid();
                AuthenticationMiddleware.Logins[authenticationKey] = Model.LoginRequest;
                NavigationManager.NavigateTo($"/login?key={authenticationKey}", true);
            }
            else
            {
                Snackbar.Add(
                    "Authentication error",
                    Severity.Error,
                    config => { config.ActionColor = Color.Error; });
            }

            Model.State = ComponentModelState.Success;
        }

        private class ComponentModel
        {
            public static ComponentModel Create()
            {
                return new ComponentModel();
            }

            public ComponentModelState State { get; set; } = ComponentModelState.Unknown;
            public bool InProgress => State == ComponentModelState.InProgress;
            public LoginRequest LoginRequest { get; set; } = new LoginRequest();
        }
    }
}
