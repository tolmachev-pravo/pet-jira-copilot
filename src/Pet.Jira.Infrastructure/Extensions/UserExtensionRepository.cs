using Microsoft.EntityFrameworkCore;
using Pet.Jira.Application.Extensions;
using Pet.Jira.Domain.Entities.Extensions;
using Pet.Jira.Infrastructure.Data.Contexts;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Infrastructure.Extensions
{
    public class UserExtensionRepository : IUserExtensionRepository
    {
        private readonly ApplicationDbContext _db;

        public UserExtensionRepository(ApplicationDbContext db) => _db = db;

        public Task<UserExtension?> GetAsync(string username, ExtensionType type, CancellationToken cancellationToken = default)
            => _db.Set<UserExtension>()
                  .FirstOrDefaultAsync(e => e.Username == username && e.Type == type, cancellationToken);

        public async Task<IReadOnlyList<UserExtension>> GetAllAsync(string username, CancellationToken cancellationToken = default)
            => await _db.Set<UserExtension>()
                        .Where(e => e.Username == username)
                        .ToListAsync(cancellationToken);

        public async Task UpsertAsync(UserExtension extension, CancellationToken cancellationToken = default)
        {
            var exists = await _db.Set<UserExtension>()
                .AnyAsync(e => e.Username == extension.Username && e.Type == extension.Type, cancellationToken);

            if (exists)
                _db.Set<UserExtension>().Update(extension);
            else
                _db.Set<UserExtension>().Add(extension);

            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
