using Pet.Jira.Application.Storage;
using Pet.Jira.Domain.Models.Users;
using Pet.Jira.Infrastructure.Storage;
using Pet.Jira.Infrastructure.Users;

namespace Pet.Jira.Infrastructure.Mock
{
    internal class MockUserProfileStorage : UserProfileStorage
    {
        public MockUserProfileStorage(
            ILocalStorage<UserProfile> localStorage, 
            IMemoryCache<string, UserProfile> memoryCache) : base(localStorage, memoryCache, null)
        {
        }
    }
}
