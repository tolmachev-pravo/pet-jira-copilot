using Pet.Jira.Application.Integrations.Dto;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Application.Integrations
{
	public interface IUserCalendarConnectionService
	{
		Task<UserCalendarConnectionDto> GetCurrentAsync(CancellationToken cancellationToken = default);

		Task<string> BuildConnectUrlAsync(CancellationToken cancellationToken = default);

		Task<YandexConnectionResult> HandleCallbackAsync(
			string code,
			string state,
			string error,
			string errorDescription,
			CancellationToken cancellationToken = default);

		Task DisconnectAsync(CancellationToken cancellationToken = default);
	}
}
