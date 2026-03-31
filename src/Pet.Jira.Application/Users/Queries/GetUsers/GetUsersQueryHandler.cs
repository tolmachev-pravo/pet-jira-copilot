using MediatR;
using Pet.Jira.Application.Users.Dto;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Application.Users.Queries.GetUsers
{
	internal class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, IEnumerable<UserDto>>
	{
		private readonly IUserDataSource _dataSource;

		public GetUsersQueryHandler(
			IUserDataSource dataSource)
		{
			_dataSource = dataSource;
		}

		public async Task<IEnumerable<UserDto>> Handle(
			GetUsersQuery request,
			CancellationToken cancellationToken)
		{
			return await _dataSource.GetUsersAsync(cancellationToken);
		}
	}
}
