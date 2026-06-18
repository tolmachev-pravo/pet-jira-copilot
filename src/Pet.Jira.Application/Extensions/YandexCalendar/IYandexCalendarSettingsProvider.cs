using Pet.Jira.Application.Extensions.YandexCalendar.Dto;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Application.Extensions.YandexCalendar
{
    public interface IYandexCalendarSettingsProvider
    {
        Task<YandexCalendarSettingsDto?> GetSettingsAsync(string username, CancellationToken cancellationToken = default);
    }
}
