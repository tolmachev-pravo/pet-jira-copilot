using Pet.Jira.Application.Users.Dto;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Application.Users
{
	public interface ICurrentAppUserService
	{
		Task<UserDto> GetOrCreateCurrentAsync(CancellationToken cancellationToken = default);
	}
}
