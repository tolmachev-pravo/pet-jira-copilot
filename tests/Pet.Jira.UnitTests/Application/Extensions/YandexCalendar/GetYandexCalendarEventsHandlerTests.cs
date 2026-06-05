using Moq;
using NUnit.Framework;
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
    public class GetYandexCalendarEventsHandlerTests
    {
        private Mock<IYandexCalendarSettingsProvider> _settingsMock = null!;
        private Mock<IYandexCalendarService> _calMock = null!;

        [SetUp]
        public void SetUp()
        {
            _settingsMock = new Mock<IYandexCalendarSettingsProvider>();
            _calMock = new Mock<IYandexCalendarService>();
        }

        [Test]
        public async Task Handle_ReturnsEvents_WhenExtensionEnabled()
        {
            var dto = new YandexCalendarSettingsDto("user@yandex.ru", "pw");
            _settingsMock.Setup(s => s.GetSettingsAsync("alice", CancellationToken.None))
                         .ReturnsAsync(dto);

            var date = new DateOnly(2026, 6, 4);
            var events = new List<YandexCalendarEventDto>
            {
                new("Standup PROJ-42", new DateTime(2026, 6, 4, 10, 0, 0), new DateTime(2026, 6, 4, 11, 0, 0), "PROJ-42")
            };
            _calMock.Setup(c => c.GetEventsAsync(
                        It.Is<YandexCalendarCredentials>(cr => cr.Login == "user@yandex.ru"),
                        date,
                        CancellationToken.None))
                    .ReturnsAsync(events);

            var handler = new GetYandexCalendarEvents.Handler(_settingsMock.Object, _calMock.Object);
            var result = await handler.Handle(
                new GetYandexCalendarEvents.Query("alice", date), CancellationToken.None);

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].JiraIssueKeyHint, Is.EqualTo("PROJ-42"));
        }

        [Test]
        public async Task Handle_ReturnsEmpty_WhenExtensionNotConfigured()
        {
            _settingsMock.Setup(s => s.GetSettingsAsync("alice", CancellationToken.None))
                         .ReturnsAsync((YandexCalendarSettingsDto?)null);

            var handler = new GetYandexCalendarEvents.Handler(_settingsMock.Object, _calMock.Object);
            var result = await handler.Handle(
                new GetYandexCalendarEvents.Query("alice", new DateOnly(2026, 6, 4)),
                CancellationToken.None);

            Assert.That(result, Is.Empty);
            _calMock.Verify(c => c.GetEventsAsync(
                It.IsAny<YandexCalendarCredentials>(),
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
