using Pet.Jira.Application.Storage;
using Pet.Jira.Domain.Models.Abstract;
using Pet.Jira.Domain.Models.Users;

namespace Pet.Jira.Application.Users
{
    public interface IUserStorage
    {
        bool TryAdd(User entity);
        bool TryGetValue(string key, out User entity);
        bool TryRemove(string key, out User entity);
    }
}
