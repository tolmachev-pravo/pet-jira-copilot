using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Application.Users.Commands
{
	public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
	{
		private readonly IUserRepository _repository;

		public CreateUserCommandHandler(
			IUserRepository repository)
		{
			_repository = repository;
		}

		public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
		{
			var entity = await _repository.AddAsync(request, cancellationToken);
			return entity.Id;
		}
	}
}
