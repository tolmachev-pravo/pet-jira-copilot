using Microsoft.EntityFrameworkCore;
using Pet.Jira.Application.Users;
using Pet.Jira.Domain.Entities.Users;
using Pet.Jira.Infrastructure.Data.Contexts;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Infrastructure.Users
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public UserRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task EnsureUserExistsAsync(string username, CancellationToken cancellationToken = default)
        {
            var exists = await _dbContext.Users
                .AnyAsync(user => user.Username == username, cancellationToken);
            if (exists)
            {
                return;
            }

            _dbContext.Users.Add(new User
            {
                Username = username,
                CreatedAt = DateTime.UtcNow
            });

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                // Запись создана параллельным запросом — уникальный индекс отклонил дубль. Это не ошибка.
            }
        }
    }
}
