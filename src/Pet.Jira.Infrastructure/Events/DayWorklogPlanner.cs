using Pet.Jira.Application.Events;
using Pet.Jira.Domain.Models.Events;
using Pet.Jira.Domain.Models.Planning;
using System;
using System.Collections.Generic;

namespace Pet.Jira.Infrastructure.Events
{
    public class DayWorklogPlanner : IDayWorklogPlanner
    {
        public IReadOnlyList<ProposedWorklog> Plan(DateOnly day, IReadOnlyList<Event> dayEvents)
        {
            return new List<ProposedWorklog>();
        }
    }
}
