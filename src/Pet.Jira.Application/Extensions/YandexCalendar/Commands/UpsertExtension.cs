using MediatR;
using Pet.Jira.Application.Extensions.YandexCalendar.Dto;
using Pet.Jira.Application.Security;
using Pet.Jira.Domain.Entities.Extensions;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Application.Extensions.YandexCalendar.Commands
{
    public class UpsertExtension
    {
        public record Command(
            string Username,
            YandexCalendarSettingsDto Settings,
            bool IsEnabled) : IRequest;

        public class Handler : IRequestHandler<Command>
        {
            private readonly IUserExtensionRepository _repository;
            private readonly ISecretProtector _protector;

            public Handler(IUserExtensionRepository repository, ISecretProtector protector)
            {
                _repository = repository;
                _protector = protector;
            }

            public async Task<Unit> Handle(Command request, CancellationToken ct)
            {
                var stored = new StoredSettings(
                    request.Settings.Login,
                    _protector.Protect(request.Settings.AppPassword));

                var existing = await _repository.GetAsync(request.Username, ExtensionType.YandexCalendar, ct);

                var entity = existing ?? new UserExtension
                {
                    Username = request.Username,
                    Type = ExtensionType.YandexCalendar,
                    CreatedAt = DateTime.UtcNow
                };

                entity.IsEnabled = request.IsEnabled;
                entity.Settings = JsonSerializer.Serialize(stored);
                entity.UpdatedAt = DateTime.UtcNow;

                await _repository.UpsertAsync(entity, ct);
                return Unit.Value;
            }

            private record StoredSettings(string Login, string AppPasswordEncrypted);
        }
    }
}
