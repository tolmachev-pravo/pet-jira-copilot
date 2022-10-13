using Pet.Jira.Application.Users;
using Pet.Jira.Domain.Models.Users;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Infrastructure.Mock
{
    internal class MockUserDateSource : IUserDataSource
    {
        public Task<User> GetCurrentUserAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new User
            {
                Username = "user",
                Password = "password",
                TimeZoneInfo = TimeZoneInfo.Local
            });
        }
    }
}
