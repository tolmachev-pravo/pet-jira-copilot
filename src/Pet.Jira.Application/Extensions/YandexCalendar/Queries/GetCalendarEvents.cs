using MediatR;
using Pet.Jira.Application.Extensions.YandexCalendar.Dto;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Application.Extensions.YandexCalendar.Queries
{
    public class GetCalendarEvents
    {
        public record Query(string Username, DateOnly Date)
            : IRequest<IReadOnlyList<CalendarEventDto>>;

        public class Handler : IRequestHandler<Query, IReadOnlyList<CalendarEventDto>>
        {
            private readonly IUserExtensionRepository _repository;
            private readonly ICalendarService _calendar;

            public Handler(IUserExtensionRepository repository, ICalendarService calendar)
            {
                _repository = repository;
                _calendar = calendar;
            }

            public async Task<IReadOnlyList<CalendarEventDto>> Handle(Query request, CancellationToken ct)
            {
                var settings = await _repository.GetYandexSettingsAsync(request.Username, ct);
                if (settings is null)
                    return Array.Empty<CalendarEventDto>();

                return await _calendar.GetEventsAsync(
                    new CalendarCredentials(settings.Login, settings.AppPassword),
                    request.Date,
                    ct);
            }
        }
    }
}
