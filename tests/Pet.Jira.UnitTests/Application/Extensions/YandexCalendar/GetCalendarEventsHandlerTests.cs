using Moq;
using NUnit.Framework;
using Pet.Jira.Application.Extensions;
using Pet.Jira.Application.Extensions.YandexCalendar;
using Pet.Jira.Application.Extensions.YandexCalendar.Dto;
using Pet.Jira.Application.Extensions.YandexCalendar.Queries;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.UnitTests.Application.Extensions.YandexCalendar
{
    [TestFixture]
    public class GetCalendarEventsHandlerTests
    {
        private Mock<IUserExtensionRepository> _repoMock = null!;
        private Mock<ICalendarService> _calMock = null!;

        [SetUp]
        public void SetUp()
        {
            _repoMock = new Mock<IUserExtensionRepository>();
            _calMock = new Mock<ICalendarService>();
        }

        [Test]
        public async Task Handle_ReturnsEvents_WhenExtensionEnabled()
        {
            var creds = new YandexCalendarSettingsDto("user@yandex.ru", "pw");
            _repoMock.Setup(r => r.GetYandexSettingsAsync("alice", CancellationToken.None))
                     .ReturnsAsync(creds);

            var date = new DateOnly(2026, 6, 4);
            var events = new List<CalendarEventDto>
            {
                new("Standup PROJ-42", new DateTime(2026, 6, 4, 10, 0, 0), new DateTime(2026, 6, 4, 11, 0, 0), "PROJ-42")
            };
            _calMock.Setup(c => c.GetEventsAsync(
                        It.Is<CalendarCredentials>(cr => cr.Login == "user@yandex.ru"),
                        date,
                        CancellationToken.None))
                    .ReturnsAsync(events);

            var handler = new GetCalendarEvents.Handler(_repoMock.Object, _calMock.Object);
            var result = await handler.Handle(
                new GetCalendarEvents.Query("alice", date), CancellationToken.None);

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].JiraIssueKeyHint, Is.EqualTo("PROJ-42"));
        }

        [Test]
        public async Task Handle_ReturnsEmpty_WhenExtensionNotConfigured()
        {
            _repoMock.Setup(r => r.GetYandexSettingsAsync("alice", CancellationToken.None))
                     .ReturnsAsync((YandexCalendarSettingsDto?)null);

            var handler = new GetCalendarEvents.Handler(_repoMock.Object, _calMock.Object);
            var result = await handler.Handle(
                new GetCalendarEvents.Query("alice", new DateOnly(2026, 6, 4)),
                CancellationToken.None);

            Assert.That(result, Is.Empty);
            _calMock.Verify(c => c.GetEventsAsync(
                It.IsAny<CalendarCredentials>(),
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
