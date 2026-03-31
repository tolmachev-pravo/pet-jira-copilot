using Pet.Jira.Application.Users.Dto;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Application.Users
{
	public interface IUserDataSource
	{
		Task<IEnumerable<UserDto>> GetUsersAsync(CancellationToken cancellationToken = default);
		Task<UserDto> GetUserAsync(string username, CancellationToken cancellationToken = default);
	}
}
