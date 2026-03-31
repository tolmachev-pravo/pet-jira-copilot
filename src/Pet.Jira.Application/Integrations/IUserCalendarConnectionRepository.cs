using Pet.Jira.Domain.Entities.Integrations;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Application.Integrations
{
	public interface IUserCalendarConnectionRepository
	{
		Task<UserCalendarConnection> GetByUserIdAsync(
			Guid userId,
			CalendarProvider provider,
			CancellationToken cancellationToken = default);

		Task<UserCalendarConnection> UpsertAsync(
			UserCalendarConnection connection,
			CancellationToken cancellationToken = default);
	}
}
