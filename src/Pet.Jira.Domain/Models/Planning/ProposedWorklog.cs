using Pet.Jira.Domain.Models.Events;
using System;

namespace Pet.Jira.Domain.Models.Planning
{
    /// <summary>
    /// A proposed loggable time slot for an <see cref="Event"/>. The seed of the
    /// future worklog row on the Event pipeline.
    /// </summary>
    public record ProposedWorklog(Event Event, DateTime Start, DateTime End)
    {
        public TimeSpan Duration => End - Start;
    }
}
