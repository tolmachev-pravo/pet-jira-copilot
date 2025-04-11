using Pet.Jira.Application.Common.Mapping;
using Pet.Jira.Domain.Entities.Users;

namespace Pet.Jira.Application.Users.Dto
{
	public class UserDto : IMapFrom<User>
	{
		public string Username { get; set; }
	}
}
