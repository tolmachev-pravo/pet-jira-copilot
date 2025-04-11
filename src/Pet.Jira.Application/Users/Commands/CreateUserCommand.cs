using MediatR;
using System;

namespace Pet.Jira.Application.Users.Commands
{
	public class CreateUserCommand : IRequest<Guid>
	{
		public CreateUserCommand(string username)
		{
			Username = username;
		}

		public string Username { get; }
	}
}
