using Pet.Jira.Application.Storage;
using Pet.Jira.Domain.Models.Users;
using Pet.Jira.Infrastructure.Storage;

namespace Pet.Jira.Infrastructure.Users
{
    public class UserProfileStorage : BaseStorage<string, UserProfile>
    {
        public UserProfileStorage(
            ILocalStorage<UserProfile> localStorage, 
            IMemoryCache<string, UserProfile> memoryCache,
            IDataSource<string, UserProfile> dataSource) : base(localStorage, memoryCache, dataSource)
        {
        }
    }
}
