using MediatR;
using Pet.Jira.Application.Extensions.YandexCalendar.Dto;
using Pet.Jira.Domain.Entities.Extensions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Application.Extensions.YandexCalendar.Queries
{
    public class GetYandexCalendarEvents
    {
        public record Query(string Username, DateOnly Date)
            : IRequest<IReadOnlyList<YandexCalendarEventDto>>;

        public class Handler : IRequestHandler<Query, IReadOnlyList<YandexCalendarEventDto>>
        {
            private readonly IUserExtensionRepository _repository;
            private readonly IYandexCalendarSettingsProvider _settings;
            private readonly IYandexCalendarService _calendar;

            public Handler(IUserExtensionRepository repository, IYandexCalendarSettingsProvider settings, IYandexCalendarService calendar)
            {
                _repository = repository;
                _settings = settings;
                _calendar = calendar;
            }

            public async Task<IReadOnlyList<YandexCalendarEventDto>> Handle(Query request, CancellationToken ct)
            {
                var entity = await _repository.GetAsync(request.Username, ExtensionType.YandexCalendar, ct);
                if (entity is null || !entity.IsEnabled)
                    return Array.Empty<YandexCalendarEventDto>();

                var settings = await _settings.GetSettingsAsync(request.Username, ct);
                if (settings is null)
                    return Array.Empty<YandexCalendarEventDto>();

                return await _calendar.GetEventsAsync(
                    new YandexCalendarCredentials(settings.Login, settings.AppPassword),
                    request.Date,
                    ct);
            }
        }
    }
}
