using System.Collections.Generic;

namespace Pet.Jira.Application.Extensions.YandexCalendar.Dto
{
    public record YandexCalendarSettingsDto(
        string Login,
        string AppPassword,
        IReadOnlyList<string> ExcludedPhrases);
}
