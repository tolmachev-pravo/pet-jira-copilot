using Pet.Jira.Application.Users.Commands;
using Pet.Jira.Domain.Entities.Users;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Application.Users
{
	public interface IUserRepository
	{
		Task<User> AddAsync(CreateUserCommand user, CancellationToken cancellationToken = default);
		Task<User> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
		Task<User> GetOrCreateByUsernameAsync(string username, CancellationToken cancellationToken = default);
	}
}
