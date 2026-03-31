using MediatR;
using Pet.Jira.Application.Users.Dto;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Application.Users.Queries.GetUser
{
	internal class GetUserQueryHandler : IRequestHandler<GetUserQuery, UserDto>
	{
		private readonly IUserDataSource _dataSource;

		public GetUserQueryHandler(
			IUserDataSource dataSource)
		{
			_dataSource = dataSource;
		}

		public Task<UserDto> Handle(
			GetUserQuery request,
			CancellationToken cancellationToken)
		{
			return _dataSource.GetUserAsync(request.Username, cancellationToken);
		}
	}
}
