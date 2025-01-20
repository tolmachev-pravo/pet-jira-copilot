using MediatR;
using System;

namespace Pet.Jira.Application.Users.Commands
{
	public class CreateUserCommand : IRequest<Guid>
	{
		public string Username { get; set; }
	}
}
