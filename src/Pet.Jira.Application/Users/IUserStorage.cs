using Pet.Jira.Application.Storage;
using Pet.Jira.Domain.Models.Users;

namespace Pet.Jira.Application.Users
{
    public interface IUserStorage : IStorage<string, User>
    {
    }
}
