using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Pet.Jira.Application.Authentication;
using Pet.Jira.Web.Authentication;
using Pet.Jira.Web.Shared;
using System;
using System.Threading.Tasks;

namespace Pet.Jira.Web.Components.Authentication
{
    public partial class Login : ComponentBase
    {
        [Inject] private NavigationManager NavigationManager { get; set; }
        [Inject] private IMediator Mediator { get; set; }
        [CascadingParameter] public ErrorHandler ErrorHandler { get; set; }

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
            try
            {
                Model.State = ComponentModelState.InProgress;
                await Mediator.Send(new Application.Worklogs.Commands.Login.Command(Model.LoginRequest));
                Guid authenticationKey = Guid.NewGuid();
                AuthenticationMiddleware.Logins[authenticationKey] = Model.LoginRequest;
                NavigationManager.NavigateTo($"/login?key={authenticationKey}", true);
            }
            catch (Exception e)
            {
                ErrorHandler.ProcessError(e);
            }
            finally
            {
                Model.State = ComponentModelState.Success;
            }
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
