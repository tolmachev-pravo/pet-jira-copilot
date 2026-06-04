using MediatR;
using Pet.Jira.Application.Extensions.Dto;
using Pet.Jira.Domain.Entities.Extensions;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Application.Extensions.Queries
{
    public class GetExtension
    {
        public record Query(string Username, ExtensionType Type)
            : IRequest<YandexCalendarSettingsDto?>;

        public class Handler : IRequestHandler<Query, YandexCalendarSettingsDto?>
        {
            private readonly IUserExtensionRepository _repo;

            public Handler(IUserExtensionRepository repo) => _repo = repo;

            public Task<YandexCalendarSettingsDto?> Handle(Query request, CancellationToken ct)
                => _repo.GetYandexSettingsAsync(request.Username, ct);
        }
    }
}
