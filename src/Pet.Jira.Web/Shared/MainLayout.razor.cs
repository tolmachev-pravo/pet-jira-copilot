using Microsoft.AspNetCore.Components;
using Pet.Jira.Application.Authentication;
using Pet.Jira.Application.Storage;
using Pet.Jira.Domain.Models.Users;
using Pet.Jira.Infrastructure.Jira;
using Pet.Jira.Infrastructure.Storage;
using System.Threading.Tasks;

namespace Pet.Jira.Web.Shared
{
    public partial class MainLayout : LayoutComponentBase
    {
        private ComponentModel _model { get; set; }
        private bool _drawerOpen = true;

        [Inject] private IJiraService _jiraService { get; set; }
        [Inject] private IMemoryCache<string, UserProfile> _userProfileMemoryCache { get; set; }
        [Inject] private ILocalStorage<UserProfile> _userProfileLocalStorage { get; set; }
        [Inject] private IStorage<string, UserProfile> _userProfileStorage { get; set; }
        [Inject] private IIdentityService _identityService { get; set; }

        protected async Task ToggleThemeAsync(bool value)
        {
            _model.Theme.IsDarkMode = value;

            var user = await _identityService.GetCurrentUserAsync();
            string key = user != null ? user.Key : default;

            var userProfile = await _userProfileStorage.GetValueAsync(key);
            userProfile ??= UserProfile.Create();
            userProfile.UseDarkMode = _model.Theme.IsDarkMode;
            await _userProfileStorage.UpdateAsync(key, userProfile);
        }

        void ToggleDrawer()
        {
            _drawerOpen = !_drawerOpen;
        }

        protected override async Task OnInitializedAsync()
        {
            _model = ComponentModel.Create();
            var user = await _identityService.GetCurrentUserAsync();
            if (user != null && _userProfileMemoryCache.TryGetValue(user.Key, out var userProfile))
            {
                _model.Initialize(userProfile);
            }         
        }

        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await TryInitModelAsync();
            }
            await base.OnAfterRenderAsync(firstRender);
        }

        private async Task TryInitModelAsync()
        {
            if (_model.IsInitialized)
            {
                return;
            }
            var userProfile = await _userProfileLocalStorage.GetValueAsync();
            var user = await _identityService.GetCurrentUserAsync();
            if (userProfile != null)
            {
                _model.Initialize(userProfile);
                if (user != null)
                {
                    _userProfileMemoryCache.TryUpdate(user.Key, userProfile);
                }                
            }
            else
            {                
                if (user != null)
                {
                    var jiraUser = await _jiraService.GetCurrentUserAsync();
                    userProfile = jiraUser.ConvertToUserProfile();
                    _model.Initialize(userProfile);
                    await _userProfileStorage.UpdateAsync(user.Key, userProfile);
                }
            }
            StateHasChanged();
        }

        public class ComponentModel
        {
            public static ComponentModel Create()
            {
                return new ComponentModel();
            }

            public Theme Theme { get; set; } = new Theme();
            public string Avatar { get; set; } = string.Empty;
            public bool IsInitialized { get; set; }
            public string Username { get; set; }

            public void Initialize(UserProfile userProfile)
            {
                Theme.IsDarkMode = userProfile.UseDarkMode;
                Avatar = userProfile.Avatar;
                Username = userProfile.Username;
                IsInitialized = true;
            }
        }

        public class Theme
        {
            public bool IsDarkMode { get; set; }
        }
    }
}
