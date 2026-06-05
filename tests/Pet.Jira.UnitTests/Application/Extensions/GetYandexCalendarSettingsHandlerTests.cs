using Moq;
using NUnit.Framework;
using Pet.Jira.Application.Extensions;
using Pet.Jira.Application.Extensions.Dto;
using Pet.Jira.Application.Extensions.Queries;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.UnitTests.Application.Extensions
{
    [TestFixture]
    public class GetYandexCalendarSettingsHandlerTests
    {
        private Mock<IUserExtensionRepository> _repoMock = null!;

        [SetUp]
        public void SetUp()
        {
            _repoMock = new Mock<IUserExtensionRepository>();
        }

        [Test]
        public async Task Handle_ReturnsSettings_WhenExtensionExists()
        {
            var settings = new YandexCalendarSettingsDto("user@yandex.ru", "secret");
            _repoMock.Setup(r => r.GetYandexSettingsAsync("alice", CancellationToken.None))
                     .ReturnsAsync(settings);

            var handler = new GetYandexCalendarSettings.Handler(_repoMock.Object);
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
            _repoMock.Setup(r => r.GetYandexSettingsAsync("alice", CancellationToken.None))
                     .ReturnsAsync((YandexCalendarSettingsDto?)null);

            var handler = new GetYandexCalendarSettings.Handler(_repoMock.Object);
            var result = await handler.Handle(
                new GetYandexCalendarSettings.Query("alice"),
                CancellationToken.None);

            Assert.That(result, Is.Null);
        }
    }
}
