using Pet.Jira.Application.Extensions;
using Pet.Jira.Application.Extensions.YandexCalendar;
using Pet.Jira.Application.Extensions.YandexCalendar.Dto;
using Pet.Jira.Application.Security;
using Pet.Jira.Domain.Entities.Extensions;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Infrastructure.Extensions.YandexCalendar
{
    public class YandexCalendarSettingsProvider : IYandexCalendarSettingsProvider
    {
        private readonly IUserExtensionRepository _repository;
        private readonly ISecretProtector _protector;

        public YandexCalendarSettingsProvider(IUserExtensionRepository repository, ISecretProtector protector)
        {
            _repository = repository;
            _protector = protector;
        }

        public async Task<YandexCalendarSettingsDto?> GetSettingsAsync(string username, CancellationToken ct = default)
        {
            var entity = await _repository.GetAsync(username, ExtensionType.YandexCalendar, ct);
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
