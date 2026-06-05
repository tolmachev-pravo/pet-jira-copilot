using Moq;
using NUnit.Framework;
using Pet.Jira.Application.Extensions.YandexCalendar;
using Pet.Jira.Application.Extensions.YandexCalendar.Dto;
using Pet.Jira.Application.Extensions.YandexCalendar.Queries;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.UnitTests.Application.Extensions.YandexCalendar
{
    [TestFixture]
    public class GetYandexCalendarSettingsHandlerTests
    {
        private Mock<IYandexCalendarSettingsProvider> _settingsMock = null!;

        [SetUp]
        public void SetUp()
        {
            _settingsMock = new Mock<IYandexCalendarSettingsProvider>();
        }

        [Test]
        public async Task Handle_ReturnsSettings_WhenExtensionExists()
        {
            var settings = new YandexCalendarSettingsDto("user@yandex.ru", "secret");
            _settingsMock.Setup(s => s.GetSettingsAsync("alice", CancellationToken.None))
                         .ReturnsAsync(settings);

            var handler = new GetYandexCalendarSettings.Handler(_settingsMock.Object);
            var result = await handler.Handle(
                new GetYandexCalendarSettings.Query("alice"),
                CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Login, Is.EqualTo("user@yandex.ru"));
            Assert.That(result.AppPassword, Is.EqualTo("secret"));
        }

        [Test]
        public async Task Handle_ReturnsNull_WhenExtensionNotFound()
        {
            _settingsMock.Setup(s => s.GetSettingsAsync("alice", CancellationToken.None))
                         .ReturnsAsync((YandexCalendarSettingsDto?)null);

            var handler = new GetYandexCalendarSettings.Handler(_settingsMock.Object);
            var result = await handler.Handle(
                new GetYandexCalendarSettings.Query("alice"),
                CancellationToken.None);

            Assert.That(result, Is.Null);
        }
    }
}
