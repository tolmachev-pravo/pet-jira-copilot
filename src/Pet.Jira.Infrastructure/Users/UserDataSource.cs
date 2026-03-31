using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Pet.Jira.Application.Users;
using Pet.Jira.Application.Users.Dto;
using Pet.Jira.Infrastructure.Data.Contexts;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Infrastructure.Users
{
	internal class UserDataSource : IUserDataSource
	{
		private readonly ApplicationDbContext _dbContext;
		private readonly IMapper _mapper;

		public UserDataSource(
			ApplicationDbContext dbContext,
			IMapper mapper)
		{
			_dbContext = dbContext;
			_mapper = mapper;
		}

		public async Task<UserDto> GetUserAsync(string username, CancellationToken cancellationToken = default)
		{
			return await _dbContext.Users
				.Where(user => user.Username == username)
				.ProjectTo<UserDto>(_mapper.ConfigurationProvider)
				.FirstOrDefaultAsync(cancellationToken: cancellationToken);
		}

		public async Task<IEnumerable<UserDto>> GetUsersAsync(CancellationToken cancellationToken = default)
		{
			return await _dbContext.Users
				.ProjectTo<UserDto>(_mapper.ConfigurationProvider)
				.ToListAsync(cancellationToken);
		}
	}
}
