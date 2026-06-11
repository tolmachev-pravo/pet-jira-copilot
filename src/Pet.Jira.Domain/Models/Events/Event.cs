using System;

namespace Pet.Jira.Domain.Models.Events
{
    public record Event(
        DateTime Start,
        DateTime End,
        string Title,
        string? Key,
        string? Description,
        Uri? Link,
        string? IssueKey,
        EventSource Source
    );
}
