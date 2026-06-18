using Pet.Jira.Domain.Models.Events;
using Pet.Jira.Domain.Models.Planning;
using System;
using System.Collections.Generic;

namespace Pet.Jira.Application.Events
{
    public interface IDayWorklogPlanner
    {
        IReadOnlyList<ProposedWorklog> Plan(DateOnly day, IReadOnlyList<Event> dayEvents);
    }
}
