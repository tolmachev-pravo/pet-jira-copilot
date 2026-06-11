using System;

namespace Pet.Jira.Application.Worklogs.Dto
{
    /// <summary>
    /// A calendar event without a Jira key. It blocks day time but cannot be logged;
    /// shown in the day view for context only.
    /// </summary>
    public record BlockedCalendarEvent(DateTime Start, DateTime End, string Title)
    {
        public TimeSpan Duration => End - Start;
    }
}
