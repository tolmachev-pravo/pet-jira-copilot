using Pet.Jira.Application.Events;
using Pet.Jira.Domain.Models.Events;
using Pet.Jira.Domain.Models.Planning;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pet.Jira.Infrastructure.Events
{
    public class DayWorklogPlanner : IDayWorklogPlanner
    {
        private static readonly TimeSpan Target = TimeSpan.FromHours(8);
        private static readonly TimeOnly WindowStart = new(10, 0);

        public IReadOnlyList<ProposedWorklog> Plan(DateOnly day, IReadOnlyList<Event> dayEvents)
        {
            var tasks = dayEvents
                .Where(e => e.Source == EventSource.Task)
                .OrderBy(e => e.Start)
                .ToList();

            var result = new List<ProposedWorklog>();

            var budget = Target;
            var totalTaskDuration = Sum(tasks);

            if (budget > TimeSpan.Zero && totalTaskDuration > TimeSpan.Zero)
            {
                var cursor = day.ToDateTime(WindowStart);
                var remaining = budget;
                for (var i = 0; i < tasks.Count; i++)
                {
                    var duration = i == tasks.Count - 1
                        ? remaining
                        : budget * ((double)(tasks[i].End - tasks[i].Start).Ticks / totalTaskDuration.Ticks);
                    remaining -= duration;

                    var start = cursor;
                    var end = cursor + duration;
                    result.Add(new ProposedWorklog(tasks[i], start, end));
                    cursor = end;
                }
            }

            return result;
        }

        private static TimeSpan Sum(IEnumerable<Event> events) =>
            events.Aggregate(TimeSpan.Zero, (acc, e) => acc + (e.End - e.Start));
    }
}
