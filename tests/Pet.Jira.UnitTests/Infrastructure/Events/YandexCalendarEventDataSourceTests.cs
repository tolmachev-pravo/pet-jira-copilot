using Moq;
using NUnit.Framework;
using Pet.Jira.Application.Authentication;
using Pet.Jira.Application.Events;
using Pet.Jira.Application.Extensions;
using Pet.Jira.Application.Extensions.YandexCalendar;
using Pet.Jira.Application.Extensions.YandexCalendar.Dto;
using Pet.Jira.Application.Storage;
using Pet.Jira.Domain.Entities.Extensions;
using Pet.Jira.Domain.Models.Events;
using Pet.Jira.Domain.Models.Users;
using Pet.Jira.Infrastructure.Events;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.UnitTests.Infrastructure.Events
{
    [TestFixture]
    public class YandexCalendarEventDataSourceTests
    {
        private Mock<IYandexCalendarService> _calendarServiceMock;
        private Mock<IYandexCalendarSettingsProvider> _settingsProviderMock;
        private Mock<IUserExtensionRepository> _extensionRepositoryMock;
        private Mock<IIdentityService> _identityServiceMock;
        private Mock<IStorage<string, UserProfile>> _userProfileStorageMock;
        private IEventDataSource _sut;
        private CancellationToken _ct;

        [SetUp]
        public void Setup()
        {
            _calendarServiceMock = new Mock<IYandexCalendarService>();
            _settingsProviderMock = new Mock<IYandexCalendarSettingsProvider>();
            _extensionRepositoryMock = new Mock<IUserExtensionRepository>();
            _identityServiceMock = new Mock<IIdentityService>();
            _userProfileStorageMock = new Mock<IStorage<string, UserProfile>>();
            _sut = new YandexCalendarEventDataSource(
                _calendarServiceMock.Object,
                _settingsProviderMock.Object,
                _extensionRepositoryMock.Object,
                _identityServiceMock.Object,
                _userProfileStorageMock.Object);
            _ct = CancellationToken.None;

            _identityServiceMock
                .Setup(x => x.GetCurrentUserAsync())
                .ReturnsAsync(new Pet.Jira.Domain.Models.Users.User { Username = "user1" });

            _userProfileStorageMock
                .Setup(x => x.GetValueAsync("user1", _ct))
                .ReturnsAsync(new UserProfile { Username = "user1", TimeZoneId = "UTC" });

            _extensionRepositoryMock
                .Setup(x => x.GetAsync("user1", ExtensionType.YandexCalendar, _ct))
                .ReturnsAsync(new UserExtension { IsEnabled = true });
        }

        [Test]
        public async Task GetEventsAsync_WhenExtensionDisabled_ReturnsEmpty()
        {
            _extensionRepositoryMock
                .Setup(x => x.GetAsync("user1", ExtensionType.YandexCalendar, _ct))
                .ReturnsAsync(new UserExtension { IsEnabled = false });

            var result = await _sut.GetEventsAsync(
                new DateOnly(2026, 6, 1),
                new DateOnly(2026, 6, 1),
                _ct);

            Assert.That(result, Is.Empty);
            _settingsProviderMock.Verify(x => x.GetSettingsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task GetEventsAsync_WhenExtensionNotConfigured_ReturnsEmpty()
        {
            _extensionRepositoryMock
                .Setup(x => x.GetAsync("user1", ExtensionType.YandexCalendar, _ct))
                .ReturnsAsync((UserExtension?)null);

            var result = await _sut.GetEventsAsync(
                new DateOnly(2026, 6, 1),
                new DateOnly(2026, 6, 1),
                _ct);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetEventsAsync_WhenSettingsNull_ReturnsEmpty()
        {
            _settingsProviderMock
                .Setup(x => x.GetSettingsAsync("user1", _ct))
                .ReturnsAsync((YandexCalendarSettingsDto?)null);

            var result = await _sut.GetEventsAsync(
                new DateOnly(2026, 6, 1),
                new DateOnly(2026, 6, 1),
                _ct);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetEventsAsync_WhenCalendarHasEvents_ReturnsMappedEvents()
        {
            var settings = new YandexCalendarSettingsDto("login", "pass", Array.Empty<string>(), Array.Empty<YandexCalendarIssueMapping>());
            _settingsProviderMock
                .Setup(x => x.GetSettingsAsync("user1", _ct))
                .ReturnsAsync(settings);

            var calEvent = new YandexCalendarEventDto(
                Summary: "Daily standup",
                Start: new DateTime(2026, 6, 1, 9, 0, 0, DateTimeKind.Utc),
                End: new DateTime(2026, 6, 1, 9, 30, 0, DateTimeKind.Utc),
                JiraIssueKeyHint: null,
                Uid: null,
                Description: null,
                Url: null);

            _calendarServiceMock
                .Setup(x => x.GetEventsAsync(
                    It.IsAny<YandexCalendarCredentials>(),
                    new DateOnly(2026, 6, 1),
                    It.IsAny<TimeZoneInfo>(),
                    _ct))
                .ReturnsAsync(new List<YandexCalendarEventDto> { calEvent });

            var result = await _sut.GetEventsAsync(
                new DateOnly(2026, 6, 1),
                new DateOnly(2026, 6, 1),
                _ct);

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Title, Is.EqualTo("Daily standup"));
            Assert.That(result[0].Start, Is.EqualTo(new DateTime(2026, 6, 1, 9, 0, 0)));
            Assert.That(result[0].End, Is.EqualTo(new DateTime(2026, 6, 1, 9, 30, 0)));
            Assert.That(result[0].Source, Is.EqualTo(EventSource.Calendar));
            Assert.That(result[0].IssueKey, Is.Null);
        }

        [Test]
        public async Task GetEventsAsync_WhenEventHasJiraIssueKeyHint_PopulatesIssueKey()
        {
            var settings = new YandexCalendarSettingsDto("login", "pass", Array.Empty<string>(), Array.Empty<YandexCalendarIssueMapping>());
            _settingsProviderMock
                .Setup(x => x.GetSettingsAsync("user1", _ct))
                .ReturnsAsync(settings);

            var calEvent = new YandexCalendarEventDto(
                Summary: "PROJ-42 planning",
                Start: new DateTime(2026, 6, 1, 9, 0, 0, DateTimeKind.Utc),
                End: new DateTime(2026, 6, 1, 9, 30, 0, DateTimeKind.Utc),
                JiraIssueKeyHint: "PROJ-42",
                Uid: null,
                Description: null,
                Url: null);

            _calendarServiceMock
                .Setup(x => x.GetEventsAsync(
                    It.IsAny<YandexCalendarCredentials>(),
                    new DateOnly(2026, 6, 1),
                    It.IsAny<TimeZoneInfo>(),
                    _ct))
                .ReturnsAsync(new List<YandexCalendarEventDto> { calEvent });

            var result = await _sut.GetEventsAsync(
                new DateOnly(2026, 6, 1),
                new DateOnly(2026, 6, 1),
                _ct);

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].IssueKey, Is.EqualTo("PROJ-42"));
        }

        [Test]
        public async Task GetEventsAsync_WhenEventMatchesExcludedPhrase_EventIsFiltered()
        {
            var settings = new YandexCalendarSettingsDto(
                "login", "pass",
                new[] { "Обед", "Lunch" },
                Array.Empty<YandexCalendarIssueMapping>());
            _settingsProviderMock
                .Setup(x => x.GetSettingsAsync("user1", _ct))
                .ReturnsAsync(settings);

            var utcNow = new DateTime(2026, 6, 1, 9, 0, 0, DateTimeKind.Utc);
            _calendarServiceMock
                .Setup(x => x.GetEventsAsync(
                    It.IsAny<YandexCalendarCredentials>(),
                    new DateOnly(2026, 6, 1),
                    It.IsAny<TimeZoneInfo>(),
                    _ct))
                .ReturnsAsync(new List<YandexCalendarEventDto>
                {
                    new("Standup",        utcNow,              utcNow.AddMinutes(30), null, null, null, null),
                    new("Обед с командой", utcNow.AddHours(3), utcNow.AddHours(4),    null, null, null, null),
                    new("Team Lunch",     utcNow.AddHours(4), utcNow.AddHours(5),    null, null, null, null),
                });

            var result = await _sut.GetEventsAsync(
                new DateOnly(2026, 6, 1),
                new DateOnly(2026, 6, 1),
                _ct);

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Title, Is.EqualTo("Standup"));
        }
    }
}
