using System;

namespace Pet.Jira.Application.Extensions.YandexCalendar.Dto
{
    public record YandexCalendarEventDto(
        string Summary,
        DateTime Start,
        DateTime End,
        string? JiraIssueKeyHint,
        string? Uid,
        string? Description,
        Uri? Url);
}
