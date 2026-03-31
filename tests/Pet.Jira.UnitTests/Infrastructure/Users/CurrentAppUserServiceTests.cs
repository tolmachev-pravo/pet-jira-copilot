using Moq;
using Pet.Jira.Application.Authentication;
using Pet.Jira.Application.Users;
using Pet.Jira.Domain.Models.Users;
using Pet.Jira.Infrastructure.Users;

namespace Pet.Jira.UnitTests.Infrastructure.Users
{
    [TestFixture]
    public class CurrentAppUserServiceTests
    {
        private Mock<IIdentityService> _identityService;
        private Mock<IUserRepository> _userRepository;
        private CurrentAppUserService _service;

        [SetUp]
        public void SetUp()
        {
            _identityService = new Mock<IIdentityService>();
            _userRepository = new Mock<IUserRepository>();
            _service = new CurrentAppUserService(_identityService.Object, _userRepository.Object);
        }

        [Test]
        public async Task GetOrCreateCurrentAsync_Should_ReturnNull_When_UserIsAnonymous()
        {
            _identityService
                .Setup(service => service.GetCurrentUserAsync())
                .ReturnsAsync((User?)null);

            var result = await _service.GetOrCreateCurrentAsync();

            Assert.That(result, Is.Null);
            _userRepository.Verify(
                repository => repository.GetOrCreateByUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public void GetOrCreateCurrentAsync_Should_Throw_When_UsernameIsMissing()
        {
            _identityService
                .Setup(service => service.GetCurrentUserAsync())
                .ReturnsAsync(new User());

            Assert.ThrowsAsync<InvalidOperationException>(() => _service.GetOrCreateCurrentAsync());
        }

        [Test]
        public async Task GetOrCreateCurrentAsync_Should_ReturnRepositoryUser()
        {
            var userId = Guid.NewGuid();

            _identityService
                .Setup(service => service.GetCurrentUserAsync())
                .ReturnsAsync(new User
                {
                    Username = "jira.user"
                });

            _userRepository
                .Setup(repository => repository.GetOrCreateByUsernameAsync("jira.user", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Pet.Jira.Domain.Entities.Users.User
                {
                    Id = userId,
                    Username = "jira.user"
                });

            var result = await _service.GetOrCreateCurrentAsync();

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Id, Is.EqualTo(userId));
                Assert.That(result.Username, Is.EqualTo("jira.user"));
            });
        }
    }
}
