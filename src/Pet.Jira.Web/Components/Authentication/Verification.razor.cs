using Microsoft.AspNetCore.Components;
using Pet.Jira.Application.Storage;
using Pet.Jira.Domain.Models.Users;
using Pet.Jira.Infrastructure.Jira;
using System.Threading.Tasks;

namespace Pet.Jira.Web.Components.Authentication
{
    public partial class Verification : ComponentBase
    {
        [Inject] private NavigationManager NavigationManager { get; set; }
        [Inject] private IJiraService _jiraService { get; set; }
        [Inject] private IStorage<string, UserProfile> _userProfileStorage { get; set; }

        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                var user = await _jiraService.GetCurrentUserAsync();
                if (user != null)
                {
                    var userProfile = await _userProfileStorage.GetValueAsync(user.Username);
                    if (userProfile == null)
                    {
                        await _userProfileStorage.AddAsync(user.ConvertToUserProfile());
                    }
                    else
                    {
                        userProfile.UpdateUserInfo(user.ConvertToUserProfile());
                        await _userProfileStorage.UpdateAsync(userProfile.Key, userProfile);
                    }
                }
            }
            await base.OnAfterRenderAsync(firstRender);
            NavigationManager.NavigateTo($"/", true); 
        }
    }
}
