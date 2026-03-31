using Microsoft.EntityFrameworkCore;
using Pet.Jira.Application.Integrations;
using Pet.Jira.Domain.Entities.Integrations;
using Pet.Jira.Infrastructure.Data.Contexts;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Infrastructure.Integrations
{
	public class UserCalendarConnectionRepository : IUserCalendarConnectionRepository
	{
		private readonly ApplicationDbContext _dbContext;

		public UserCalendarConnectionRepository(ApplicationDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public async Task<UserCalendarConnection> GetByUserIdAsync(
			Guid userId,
			CalendarProvider provider,
			CancellationToken cancellationToken = default)
		{
			return await _dbContext.UserCalendarConnections
				.FirstOrDefaultAsync(
					item => item.UserId == userId && item.Provider == provider,
					cancellationToken);
		}

		public async Task<UserCalendarConnection> UpsertAsync(
			UserCalendarConnection connection,
			CancellationToken cancellationToken = default)
		{
			if (connection.Id == Guid.Empty)
			{
				connection.Id = Guid.NewGuid();
				_dbContext.UserCalendarConnections.Add(connection);
			}
			else
			{
				_dbContext.UserCalendarConnections.Update(connection);
			}

			await _dbContext.SaveChangesAsync(cancellationToken);
			return connection;
		}
	}
}
