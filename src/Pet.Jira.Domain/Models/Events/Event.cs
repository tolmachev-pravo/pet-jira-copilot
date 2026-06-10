using Pet.Jira.Domain.Models.Issues;
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
        Issue? Issue,
        EventSource Source
    );
}
