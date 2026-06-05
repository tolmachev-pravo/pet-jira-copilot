using Pet.Jira.Application.Extensions.YandexCalendar.Dto;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Application.Extensions.YandexCalendar
{
    public record CalendarCredentials(string Login, string AppPassword);

    public interface ICalendarService
    {
        Task<IReadOnlyList<CalendarEventDto>> GetEventsAsync(
            CalendarCredentials credentials,
            DateOnly date,
            CancellationToken ct = default);
    }
}
