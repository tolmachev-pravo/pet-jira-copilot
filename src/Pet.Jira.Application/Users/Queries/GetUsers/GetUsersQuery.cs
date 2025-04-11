using MediatR;
using Pet.Jira.Application.Users.Dto;
using System.Collections.Generic;

namespace Pet.Jira.Application.Users.Queries.GetUsers
{
	internal class GetUsersQuery : IRequest<IEnumerable<UserDto>>
	{
	}
}
