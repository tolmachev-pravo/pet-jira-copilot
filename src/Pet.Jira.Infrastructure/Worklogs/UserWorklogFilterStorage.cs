using Pet.Jira.Application.Storage;
using Pet.Jira.Application.Worklogs.Dto;
using Pet.Jira.Infrastructure.Storage;

namespace Pet.Jira.Infrastructure.Worklogs
{
    public class UserWorklogFilterStorage : BaseStorage<string, UserWorklogFilter>
    {
        public UserWorklogFilterStorage(
            ILocalStorage<UserWorklogFilter> localStorage,
            IMemoryCache<string, UserWorklogFilter> memoryCache) : base(localStorage, memoryCache)
        {
        }
    }
}
