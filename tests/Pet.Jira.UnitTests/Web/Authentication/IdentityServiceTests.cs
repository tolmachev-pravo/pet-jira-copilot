using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Pet.Jira.Web.Authentication;
using System.Security.Claims;

namespace Pet.Jira.UnitTests.Web.Authentication
{
    [TestFixture]
    public class IdentityServiceTests
    {
        [Test]
        public async Task GetCurrentUserAsync_Should_ReturnUser_FromHttpContext()
        {
            var httpContextAccessor = new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext
                {
                    User = CreatePrincipal("jira.user", "secret", "token")
                }
            };

            var provider = new StubAuthenticationStateProvider(CreatePrincipal("component.user"));
            var service = new IdentityService(provider, httpContextAccessor);

            var result = await service.GetCurrentUserAsync();

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Username, Is.EqualTo("jira.user"));
                Assert.That(result.Password, Is.EqualTo("secret"));
                Assert.That(result.PersonalAccessToken, Is.EqualTo("token"));
            });
        }

        [Test]
        public async Task GetCurrentUserAsync_Should_Fallback_ToAuthenticationStateProvider()
        {
            var httpContextAccessor = new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext()
            };

            var provider = new StubAuthenticationStateProvider(CreatePrincipal("component.user"));
            var service = new IdentityService(provider, httpContextAccessor);

            var result = await service.GetCurrentUserAsync();

            Assert.That(result?.Username, Is.EqualTo("component.user"));
        }

        [Test]
        public void CurrentUser_Should_ReadOnly_HttpContext()
        {
            var httpContextAccessor = new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext
                {
                    User = CreatePrincipal("jira.user")
                }
            };

            var provider = new StubAuthenticationStateProvider(CreatePrincipal("component.user"));
            var service = new IdentityService(provider, httpContextAccessor);

            var result = service.CurrentUser;

            Assert.That(result?.Username, Is.EqualTo("jira.user"));
        }

        [Test]
        public void CurrentUser_Should_ReturnNull_When_HttpContextUserUnavailable()
        {
            var httpContextAccessor = new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext()
            };

            var provider = new StubAuthenticationStateProvider(CreatePrincipal("component.user"));
            var service = new IdentityService(provider, httpContextAccessor);

            var result = service.CurrentUser;

            Assert.That(result, Is.Null);
        }

        private static ClaimsPrincipal CreatePrincipal(
            string username,
            string? password = null,
            string? personalAccessToken = null)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username)
            };

            if (password != null)
            {
                claims.Add(new Claim(ClaimTypes.UserData, password));
            }

            if (personalAccessToken != null)
            {
                claims.Add(new Claim("PersonalAccessToken", personalAccessToken));
            }

            var identity = new ClaimsIdentity(claims, "TestAuth");
            return new ClaimsPrincipal(identity);
        }

        private sealed class StubAuthenticationStateProvider : AuthenticationStateProvider
        {
            private readonly AuthenticationState _authenticationState;

            public StubAuthenticationStateProvider(ClaimsPrincipal user)
            {
                _authenticationState = new AuthenticationState(user);
            }

            public override Task<AuthenticationState> GetAuthenticationStateAsync()
            {
                return Task.FromResult(_authenticationState);
            }
        }
    }
}
