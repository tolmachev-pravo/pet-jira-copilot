using MediatR;
using Pet.Jira.Application.Extensions.YandexCalendar.Dto;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Application.Extensions.YandexCalendar.Queries
{
    public class GetYandexCalendarSettings
    {
        public record Query(string Username) : IRequest<YandexCalendarSettingsDto?>;

        public class Handler : IRequestHandler<Query, YandexCalendarSettingsDto?>
        {
            private readonly IYandexCalendarSettingsProvider _settings;

            public Handler(IYandexCalendarSettingsProvider settings) => _settings = settings;

            public Task<YandexCalendarSettingsDto?> Handle(Query request, CancellationToken ct)
                => _settings.GetSettingsAsync(request.Username, ct);
        }
    }
}
