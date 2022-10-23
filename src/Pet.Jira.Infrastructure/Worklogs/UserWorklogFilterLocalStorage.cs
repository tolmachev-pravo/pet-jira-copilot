using Blazored.LocalStorage;
using Pet.Jira.Application.Worklogs.Dto;
using Pet.Jira.Infrastructure.Storage;

namespace Pet.Jira.Infrastructure.Worklogs
{
    public class UserWorklogFilterLocalStorage : BaseLocalStorage<UserWorklogFilter>
    {
        public UserWorklogFilterLocalStorage(ILocalStorageService localStorage) : base(localStorage)
        {
        }
    }
}
