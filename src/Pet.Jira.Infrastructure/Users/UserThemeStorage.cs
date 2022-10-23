using Pet.Jira.Application.Storage;
using Pet.Jira.Application.Users;
using Pet.Jira.Infrastructure.Storage;

namespace Pet.Jira.Infrastructure.Users
{
    public class UserThemeStorage : BaseStorage<string, UserTheme>
    {
        public UserThemeStorage(
            ILocalStorage<UserTheme> localStorage,
            IMemoryCache<string, UserTheme> memoryCache) : base(localStorage, memoryCache)
        {
        }
    }
}
