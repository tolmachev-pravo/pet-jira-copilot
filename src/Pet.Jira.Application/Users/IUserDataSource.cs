using Pet.Jira.Domain.Models.Users;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Application.Users
{
    public interface IUserDataSource
    {
        Task<User> GetCurrentUserAsync(CancellationToken cancellationToken = default);
    }
}
