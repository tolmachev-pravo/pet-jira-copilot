using Blazored.LocalStorage;
using Pet.Jira.Application.Users;
using Pet.Jira.Infrastructure.Storage;

namespace Pet.Jira.Infrastructure.Users
{
    public class UserThemeLocalStorage : BaseLocalStorage<UserTheme>
    {
        public UserThemeLocalStorage(ILocalStorageService localStorage) : base(localStorage)
        {
        }
    }
}
