using MediatR;
using Pet.Jira.Application.Users.Dto;

namespace Pet.Jira.Application.Users.Queries.GetUser
{
	internal class GetUserQuery : IRequest<UserDto>
	{
		public string Username { get; set; }
	}
}
