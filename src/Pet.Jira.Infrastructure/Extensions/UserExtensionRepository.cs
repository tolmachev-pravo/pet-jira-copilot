using Microsoft.EntityFrameworkCore;
using Pet.Jira.Application.Extensions;
using Pet.Jira.Application.Extensions.Dto;
using Pet.Jira.Application.Security;
using Pet.Jira.Domain.Entities.Extensions;
using Pet.Jira.Infrastructure.Data.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Infrastructure.Extensions
{
    public class UserExtensionRepository : IUserExtensionRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ISecretProtector _protector;

        public UserExtensionRepository(ApplicationDbContext db, ISecretProtector protector)
        {
            _db = db;
            _protector = protector;
        }

        public Task<UserExtension?> GetAsync(string username, ExtensionType type, CancellationToken ct = default)
            => _db.Set<UserExtension>()
                  .FirstOrDefaultAsync(e => e.Username == username && e.Type == type, ct);

        public async Task<IReadOnlyList<UserExtension>> GetAllAsync(string username, CancellationToken ct = default)
            => await _db.Set<UserExtension>()
                        .Where(e => e.Username == username)
                        .ToListAsync(ct);

        public async Task UpsertAsync(UserExtension extension, CancellationToken ct = default)
        {
            var exists = await _db.Set<UserExtension>()
                .AnyAsync(e => e.Username == extension.Username && e.Type == extension.Type, ct);

            if (exists)
                _db.Set<UserExtension>().Update(extension);
            else
                _db.Set<UserExtension>().Add(extension);

            await _db.SaveChangesAsync(ct);
        }

        public async Task<YandexCalendarSettingsDto?> GetYandexSettingsAsync(
            string username, CancellationToken ct = default)
        {
            var entity = await GetAsync(username, ExtensionType.YandexCalendar, ct);
            if (entity is null || !entity.IsEnabled || string.IsNullOrEmpty(entity.Settings))
                return null;

            var stored = JsonSerializer.Deserialize<StoredSettings>(entity.Settings);
            if (stored is null) return null;

            var plainPassword = _protector.Unprotect(stored.AppPasswordEncrypted);
            return new YandexCalendarSettingsDto(stored.Login, plainPassword);
        }

        private record StoredSettings(string Login, string AppPasswordEncrypted);
    }
}
