using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Pet.Jira.Application.Authentication;
using Pet.Jira.Application.Authentication.Dto;
using Pet.Jira.Application.Storage;
using Pet.Jira.Domain.Models.Users;
using Pet.Jira.Web.Components.Common;
using Pet.Jira.Web.Shared;
using System;
using System.Threading.Tasks;

namespace Pet.Jira.Web.Components.Authentication
{
    public partial class Login : ComponentBase
    {
        [Inject] private NavigationManager NavigationManager { get; set; }
        [Inject] private IMediator Mediator { get; set; }
        [Inject] private ILoginMemoryCache LoginMemoryCache { get; set; }
        [Inject] private IStorage<string, UserProfile> _userProfileStorage { get; set; }
        [Inject] private IDataSource<string, UserProfile> _userProfileDataSource { get; set; }
        [Inject] private IIdentityService _identityService { get; set; }
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
                Model.StateTo(ComponentModelState.InProgress);
                await Mediator.Send(new Application.Worklogs.Commands.Login.Command(Model.LoginRequest));
                var loginDto = LoginDto.Create(Model.LoginRequest);
                LoginMemoryCache.TryAdd(loginDto);
                NavigationManager.NavigateTo($"/login?key={loginDto.Id}", true);
            }
            catch (Exception e)
            {
                ErrorHandler.ProcessError(e);
            }
            finally
            {
                Model.StateTo(ComponentModelState.Success);
            }
        }

        private class ComponentModel : BaseStateComponentModel
        {
            public static ComponentModel Create()
            {
                return new ComponentModel();
            }

            public LoginRequest LoginRequest { get; set; } = new LoginRequest();
        }
    }
}
