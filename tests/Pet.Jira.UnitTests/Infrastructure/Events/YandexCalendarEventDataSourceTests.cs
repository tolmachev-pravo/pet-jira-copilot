using Moq;
using NUnit.Framework;
using Pet.Jira.Application.Authentication;
using Pet.Jira.Application.Events;
using Pet.Jira.Application.Extensions.YandexCalendar;
using Pet.Jira.Application.Extensions.YandexCalendar.Dto;
using Pet.Jira.Application.Storage;
using Pet.Jira.Domain.Models.Events;
using Pet.Jira.Domain.Models.Users;
using Pet.Jira.Infrastructure.Events;
using Pet.Jira.Infrastructure.Jira;
using Pet.Jira.Infrastructure.Jira.Dto;
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
        private Mock<IIdentityService> _identityServiceMock;
        private Mock<IStorage<string, UserProfile>> _userProfileStorageMock;
        private Mock<IJiraService> _jiraServiceMock;
        private IEventDataSource _sut;
        private CancellationToken _ct;

        [SetUp]
        public void Setup()
        {
            _calendarServiceMock = new Mock<IYandexCalendarService>();
            _settingsProviderMock = new Mock<IYandexCalendarSettingsProvider>();
            _identityServiceMock = new Mock<IIdentityService>();
            _userProfileStorageMock = new Mock<IStorage<string, UserProfile>>();
            _jiraServiceMock = new Mock<IJiraService>();
            _sut = new YandexCalendarEventDataSource(
                _calendarServiceMock.Object,
                _settingsProviderMock.Object,
                _identityServiceMock.Object,
                _userProfileStorageMock.Object,
                _jiraServiceMock.Object);
            _ct = CancellationToken.None;

            _identityServiceMock
                .Setup(x => x.GetCurrentUserAsync())
                .ReturnsAsync(new Pet.Jira.Domain.Models.Users.User { Username = "user1" });

            _userProfileStorageMock
                .Setup(x => x.GetValueAsync("user1", _ct))
                .ReturnsAsync(new UserProfile { Username = "user1", TimeZoneId = "UTC" });
        }

        [Test]
        public async Task GetEventsAsync_WhenSettingsNull_ReturnsEmptyList()
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
                Start: new DateTime(2026, 6, 1, 9, 0, 0),
                End: new DateTime(2026, 6, 1, 9, 30, 0),
                JiraIssueKeyHint: null);

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
            Assert.That(result[0].Issue, Is.Null);
        }

        [Test]
        public async Task GetEventsAsync_WhenEventHasIssueKeyHint_SetsIssue()
        {
            var settings = new YandexCalendarSettingsDto("login", "pass", Array.Empty<string>(), Array.Empty<YandexCalendarIssueMapping>());
            _settingsProviderMock
                .Setup(x => x.GetSettingsAsync("user1", _ct))
                .ReturnsAsync(settings);

            var calEvent = new YandexCalendarEventDto(
                Summary: "Worked on PROJ-42",
                Start: new DateTime(2026, 6, 1, 10, 0, 0),
                End: new DateTime(2026, 6, 1, 11, 0, 0),
                JiraIssueKeyHint: "PROJ-42");

            _calendarServiceMock
                .Setup(x => x.GetEventsAsync(
                    It.IsAny<YandexCalendarCredentials>(),
                    new DateOnly(2026, 6, 1),
                    It.IsAny<TimeZoneInfo>(),
                    _ct))
                .ReturnsAsync(new List<YandexCalendarEventDto> { calEvent });

            _jiraServiceMock
                .Setup(x => x.GetIssueAsync("PROJ-42", _ct))
                .ReturnsAsync(new IssueDto { Key = "PROJ-42", Summary = "Fix the thing", Link = "http://jira/PROJ-42" });

            var result = await _sut.GetEventsAsync(
                new DateOnly(2026, 6, 1),
                new DateOnly(2026, 6, 1),
                _ct);

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Issue, Is.Not.Null);
            Assert.That(result[0].Issue!.Key, Is.EqualTo("PROJ-42"));
        }
    }
}
