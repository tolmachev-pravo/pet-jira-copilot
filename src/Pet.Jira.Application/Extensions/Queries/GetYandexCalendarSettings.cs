using MediatR;
using Pet.Jira.Application.Extensions.Dto;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Application.Extensions.Queries
{
    public class GetYandexCalendarSettings
    {
        public record Query(string Username) : IRequest<YandexCalendarSettingsDto?>;

        public class Handler : IRequestHandler<Query, YandexCalendarSettingsDto?>
        {
            private readonly IUserExtensionRepository _repository;

            public Handler(IUserExtensionRepository repository) => _repository = repository;

            public Task<YandexCalendarSettingsDto?> Handle(Query request, CancellationToken ct)
                => _repository.GetYandexSettingsAsync(request.Username, ct);
        }
    }
}
