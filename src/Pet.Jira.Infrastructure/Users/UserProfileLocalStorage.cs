using Blazored.LocalStorage;
using Pet.Jira.Domain.Models.Users;
using Pet.Jira.Infrastructure.Storage;

namespace Pet.Jira.Infrastructure.Users
{
    public class UserProfileLocalStorage : BaseLocalStorage<UserProfile>
    {
        public UserProfileLocalStorage(ILocalStorageService localStorage) : base(localStorage)
        {
        }
    }
}
