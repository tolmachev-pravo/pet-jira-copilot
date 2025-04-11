using Pet.Jira.Application.Users;
using Pet.Jira.Application.Users.Commands;
using Pet.Jira.Domain.Entities.Users;
using Pet.Jira.Infrastructure.Data.Contexts;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Infrastructure.Users
{
	public class UserRepository : IUserRepository
	{
		private readonly ApplicationDbContext _dbContext;

		public UserRepository(
			ApplicationDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public async Task<User> AddAsync(CreateUserCommand user, CancellationToken cancellationToken = default)
		{
			var entity = new User
			{
				Username = user.Username,
				CreatedAt = DateTime.UtcNow
			};

			_dbContext.Users.Add(entity);
			await _dbContext.SaveChangesAsync(cancellationToken);

			return entity;
		}
	}
}
