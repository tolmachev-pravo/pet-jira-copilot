using Pet.Jira.Application.Extensions.YandexCalendar.Dto;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Application.Extensions.YandexCalendar
{
    public record YandexCalendarCredentials(string Login, string AppPassword);

    public interface IYandexCalendarService
    {
        Task<IReadOnlyList<YandexCalendarEventDto>> GetEventsAsync(
            YandexCalendarCredentials credentials,
            DateOnly date,
            CancellationToken ct = default);
    }
}
