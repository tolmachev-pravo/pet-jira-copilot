using Pet.Jira.Application.Authentication;
using Pet.Jira.Application.Events;
using Pet.Jira.Application.Extensions;
using Pet.Jira.Application.Extensions.YandexCalendar;
using Pet.Jira.Application.Storage;
using Pet.Jira.Domain.Entities.Extensions;
using Pet.Jira.Domain.Models.Events;
using Pet.Jira.Domain.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Infrastructure.Events
{
    public class YandexCalendarEventDataSource : IEventDataSource
    {
        private readonly IYandexCalendarService _calendarService;
        private readonly IYandexCalendarSettingsProvider _settingsProvider;
        private readonly IUserExtensionRepository _extensionRepository;
        private readonly IIdentityService _identityService;
        private readonly IStorage<string, UserProfile> _userProfileStorage;

        public YandexCalendarEventDataSource(
            IYandexCalendarService calendarService,
            IYandexCalendarSettingsProvider settingsProvider,
            IUserExtensionRepository extensionRepository,
            IIdentityService identityService,
            IStorage<string, UserProfile> userProfileStorage)
        {
            _calendarService = calendarService;
            _settingsProvider = settingsProvider;
            _extensionRepository = extensionRepository;
            _identityService = identityService;
            _userProfileStorage = userProfileStorage;
        }

        public async Task<IReadOnlyList<Domain.Models.Events.Event>> GetEventsAsync(
            DateOnly from,
            DateOnly to,
            CancellationToken cancellationToken)
        {
            var user = await _identityService.GetCurrentUserAsync();

            var extension = await _extensionRepository.GetAsync(user.Key, ExtensionType.YandexCalendar, cancellationToken);
            if (extension is null || !extension.IsEnabled)
                return Array.Empty<Domain.Models.Events.Event>();

            var settings = await _settingsProvider.GetSettingsAsync(user.Key, cancellationToken);
            if (settings is null)
                return Array.Empty<Domain.Models.Events.Event>();

            var userProfile = await _userProfileStorage.GetValueAsync(user.Key, cancellationToken);
            var userTimeZone = userProfile?.TimeZoneInfo ?? TimeZoneInfo.Local;

            var credentials = new YandexCalendarCredentials(settings.Login, settings.AppPassword);
            var events = new List<Domain.Models.Events.Event>();

            for (var date = from; date <= to; date = date.AddDays(1))
            {
                var calendarEvents = await _calendarService.GetEventsAsync(
                    credentials, date, userTimeZone, cancellationToken);

                foreach (var calEvent in calendarEvents)
                {
                    if (settings.ExcludedPhrases.Any(p =>
                            calEvent.Summary.Contains(p, StringComparison.OrdinalIgnoreCase)))
                        continue;

                    events.Add(new Domain.Models.Events.Event(
                        Start: TimeZoneInfo.ConvertTimeFromUtc(calEvent.Start, userTimeZone),
                        End: TimeZoneInfo.ConvertTimeFromUtc(calEvent.End, userTimeZone),
                        Title: calEvent.Summary,
                        Key: calEvent.Uid,
                        Description: calEvent.Description,
                        Link: calEvent.Url,
                        Issue: null,
                        Source: EventSource.Calendar));
                }
            }

            return events;
        }
    }
}
