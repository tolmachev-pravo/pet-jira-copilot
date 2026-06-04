using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Application.Users
{
    public interface IUserRepository
    {
        Task EnsureUserExistsAsync(string username, CancellationToken cancellationToken = default);
    }
}
