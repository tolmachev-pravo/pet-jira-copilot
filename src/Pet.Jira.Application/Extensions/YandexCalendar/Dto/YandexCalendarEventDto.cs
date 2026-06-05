using System;

namespace Pet.Jira.Application.Extensions.YandexCalendar.Dto
{
    public record YandexCalendarEventDto(
        string Summary,
        DateTime StartLocal,
        DateTime EndLocal,
        string? JiraIssueKeyHint);
}
