using Pet.Jira.Application.Authentication;
using Pet.Jira.Application.Events;
using Pet.Jira.Application.Extensions.YandexCalendar;
using Pet.Jira.Application.Storage;
using Pet.Jira.Domain.Models.Events;
using Pet.Jira.Domain.Models.Users;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Infrastructure.Events
{
    public class YandexCalendarEventDataSource : IEventDataSource
    {
        private readonly IYandexCalendarService _calendarService;
        private readonly IYandexCalendarSettingsProvider _settingsProvider;
        private readonly IIdentityService _identityService;
        private readonly IStorage<string, UserProfile> _userProfileStorage;

        public YandexCalendarEventDataSource(
            IYandexCalendarService calendarService,
            IYandexCalendarSettingsProvider settingsProvider,
            IIdentityService identityService,
            IStorage<string, UserProfile> userProfileStorage)
        {
            _calendarService = calendarService;
            _settingsProvider = settingsProvider;
            _identityService = identityService;
            _userProfileStorage = userProfileStorage;
        }

        public async Task<IReadOnlyList<Domain.Models.Events.Event>> GetEventsAsync(
            DateOnly from,
            DateOnly to,
            CancellationToken ct)
        {
            var user = await _identityService.GetCurrentUserAsync();
            var userProfile = await _userProfileStorage.GetValueAsync(user.Key, ct);
            var settings = await _settingsProvider.GetSettingsAsync(user.Key, ct);

            if (settings is null)
                return Array.Empty<Domain.Models.Events.Event>();

            var credentials = new YandexCalendarCredentials(settings.Login, settings.AppPassword);
            var events = new List<Domain.Models.Events.Event>();

            for (var date = from; date <= to; date = date.AddDays(1))
            {
                var calendarEvents = await _calendarService.GetEventsAsync(
                    credentials, date, userProfile!.TimeZoneInfo, ct);

                foreach (var calEvent in calendarEvents)
                {
                    events.Add(new Domain.Models.Events.Event(
                        Start: calEvent.Start,
                        End: calEvent.End,
                        Title: calEvent.Summary,
                        Description: null,
                        Link: null,
                        Issue: null,
                        Source: EventSource.Calendar));
                }
            }

            return events;
        }
    }
}
