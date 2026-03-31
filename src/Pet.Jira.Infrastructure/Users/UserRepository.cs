using Microsoft.EntityFrameworkCore;
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
			var entity = CreateEntity(user.Username);
			_dbContext.Users.Add(entity);
			await _dbContext.SaveChangesAsync(cancellationToken);

			return entity;
		}

		public Task<User> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
		{
			EnsureUsername(username);
			return _dbContext.Users.FirstOrDefaultAsync(item => item.Username == username, cancellationToken);
		}

		public async Task<User> GetOrCreateByUsernameAsync(string username, CancellationToken cancellationToken = default)
		{
			EnsureUsername(username);

			var user = await GetByUsernameAsync(username, cancellationToken);
			if (user != null)
			{
				return user;
			}

			var entity = CreateEntity(username);
			_dbContext.Users.Add(entity);

			try
			{
				await _dbContext.SaveChangesAsync(cancellationToken);
				return entity;
			}
			catch (DbUpdateException)
			{
				_dbContext.Entry(entity).State = EntityState.Detached;

				user = await GetByUsernameAsync(username, cancellationToken);
				if (user != null)
				{
					return user;
				}

				throw;
			}
		}

		private static User CreateEntity(string username)
		{
			EnsureUsername(username);

			return new User
			{
				Username = username,
				CreatedAt = DateTime.UtcNow
			};
		}

		private static void EnsureUsername(string username)
		{
			if (string.IsNullOrWhiteSpace(username))
			{
				throw new ArgumentException("Username is required.", nameof(username));
			}
		}
	}
}
