using Moq;
using NUnit.Framework;
using Pet.Jira.Application.Extensions;
using Pet.Jira.Application.Extensions.YandexCalendar.Commands;
using Pet.Jira.Application.Extensions.YandexCalendar.Dto;
using Pet.Jira.Application.Security;
using Pet.Jira.Domain.Entities.Extensions;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.UnitTests.Application.Extensions.YandexCalendar
{
    [TestFixture]
    public class UpsertYandexCalendarExtensionHandlerTests
    {
        private Mock<IUserExtensionRepository> _repoMock = null!;
        private Mock<ISecretProtector> _protectorMock = null!;

        [SetUp]
        public void SetUp()
        {
            _repoMock = new Mock<IUserExtensionRepository>();
            _protectorMock = new Mock<ISecretProtector>();
        }

        [Test]
        public async Task Handle_EncryptsPasswordBeforeSaving()
        {
            _protectorMock.Setup(p => p.Protect("plainpw")).Returns("encpw");

            UserExtension? saved = null;
            _repoMock.Setup(r => r.UpsertAsync(It.IsAny<UserExtension>(), CancellationToken.None))
                     .Callback<UserExtension, CancellationToken>((e, _) => saved = e)
                     .Returns(Task.CompletedTask);
            _repoMock.Setup(r => r.GetAsync("alice", ExtensionType.YandexCalendar, CancellationToken.None))
                     .ReturnsAsync((UserExtension?)null);

            var handler = new UpsertYandexCalendarExtension.Handler(_repoMock.Object, _protectorMock.Object);
            await handler.Handle(
                new UpsertYandexCalendarExtension.Command(
                    "alice",
                    new YandexCalendarSettingsDto("user@yandex.ru", "plainpw"),
                    IsEnabled: true),
                CancellationToken.None);

            Assert.That(saved, Is.Not.Null);
            var settings = JsonSerializer.Deserialize<StoredSettingsHelper>(saved!.Settings)!;
            Assert.That(settings.Login, Is.EqualTo("user@yandex.ru"));
            Assert.That(settings.AppPasswordEncrypted, Is.EqualTo("encpw"));
            Assert.That(saved.IsEnabled, Is.True);
            Assert.That(saved.Username, Is.EqualTo("alice"));
        }

        private record StoredSettingsHelper(string Login, string AppPasswordEncrypted);
    }
}
