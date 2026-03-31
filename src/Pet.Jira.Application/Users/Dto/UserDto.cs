using Pet.Jira.Application.Common.Mapping;
using Pet.Jira.Domain.Entities.Users;
using System;

namespace Pet.Jira.Application.Users.Dto
{
	public class UserDto : IMapFrom<User>
	{
		public Guid Id { get; set; }
		public string Username { get; set; }
	}
}
