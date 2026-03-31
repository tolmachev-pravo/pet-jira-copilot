using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Pet.Jira.Application.Integrations;
using Pet.Jira.Application.Users;
using Pet.Jira.Application.Users.Dto;
using Pet.Jira.Infrastructure.Integrations.Yandex;
using System.Web;

namespace Pet.Jira.UnitTests.Infrastructure.Integrations
{
    [TestFixture]
    public class YandexOAuthUserCalendarConnectionServiceTests
    {
        [Test]
        public async Task BuildConnectUrlAsync_Should_UseAbsoluteRedirectUri_AsIs()
        {
            var service = CreateService(
                redirectUri: "https://localhost:44310/profile/integrations/yandex/callback",
                httpContext: CreateHttpContext("https", "localhost:44310"));

            var result = await service.BuildConnectUrlAsync();
            var query = HttpUtility.ParseQueryString(new Uri(result).Query);

            Assert.That(
                query["redirect_uri"],
                Is.EqualTo("https://localhost:44310/profile/integrations/yandex/callback"));
        }

        [Test]
        public async Task BuildConnectUrlAsync_Should_ResolveRelativeRedirectUri_FromHttpContext()
        {
            var service = CreateService(
                redirectUri: "/profile/integrations/yandex/callback",
                httpContext: CreateHttpContext("https", "localhost:44310"));

            var result = await service.BuildConnectUrlAsync();
            var query = HttpUtility.ParseQueryString(new Uri(result).Query);

            Assert.That(
                query["redirect_uri"],
                Is.EqualTo("https://localhost:44310/profile/integrations/yandex/callback"));
        }

        [Test]
        public async Task BuildConnectUrlAsync_Should_NormalizeRelativeRedirectUri_WithoutLeadingSlash()
        {
            var service = CreateService(
                redirectUri: "profile/integrations/yandex/callback",
                httpContext: CreateHttpContext("https", "localhost:44310"));

            var result = await service.BuildConnectUrlAsync();
            var query = HttpUtility.ParseQueryString(new Uri(result).Query);

            Assert.That(
                query["redirect_uri"],
                Is.EqualTo("https://localhost:44310/profile/integrations/yandex/callback"));
        }

        [Test]
        public void BuildConnectUrlAsync_Should_Throw_When_RedirectUriIsEmpty()
        {
            var service = CreateService(
                redirectUri: "",
                httpContext: CreateHttpContext("https", "localhost:44310"));

            Assert.ThrowsAsync<InvalidOperationException>(() => service.BuildConnectUrlAsync());
        }

        private static YandexOAuthUserCalendarConnectionService CreateService(string redirectUri, HttpContext httpContext)
        {
            var currentAppUserService = new Mock<ICurrentAppUserService>();
            currentAppUserService
                .Setup(service => service.GetOrCreateCurrentAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UserDto
                {
                    Id = Guid.NewGuid(),
                    Username = "jira.user"
                });

            var connectionRepository = new Mock<IUserCalendarConnectionRepository>();
            var httpClientFactory = new Mock<IHttpClientFactory>();
            var httpContextAccessor = new HttpContextAccessor
            {
                HttpContext = httpContext
            };

            return new YandexOAuthUserCalendarConnectionService(
                currentAppUserService.Object,
                connectionRepository.Object,
                httpClientFactory.Object,
                httpContextAccessor,
                new EphemeralDataProtectionProvider(),
                Options.Create(new YandexOAuthConfiguration
                {
                    ClientId = "client-id",
                    ClientSecret = "client-secret",
                    RedirectUri = redirectUri,
                    Scope = "calendar:all"
                }),
                NullLogger<YandexOAuthUserCalendarConnectionService>.Instance);
        }

        private static HttpContext CreateHttpContext(string scheme, string host)
        {
            var context = new DefaultHttpContext();
            context.Request.Scheme = scheme;
            context.Request.Host = new HostString(host);
            return context;
        }
    }
}
