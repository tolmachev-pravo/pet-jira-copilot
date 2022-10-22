using Pet.Jira.Application.Storage;
using Pet.Jira.Domain.Models.Users;

namespace Pet.Jira.Application.Users
{
    public class UserProfileMemoryCache : BaseMemoryCache<string, UserProfile>
    {
    }
}
