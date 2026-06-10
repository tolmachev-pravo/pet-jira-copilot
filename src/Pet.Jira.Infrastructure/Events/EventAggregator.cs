using Pet.Jira.Application.Events;
using Pet.Jira.Domain.Models.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Infrastructure.Events
{
    public class EventAggregator : IEventAggregator
    {
        private readonly IEnumerable<IEventDataSource> _sources;

        public EventAggregator(IEnumerable<IEventDataSource> sources)
        {
            _sources = sources;
        }

        public async Task<IReadOnlyDictionary<DateOnly, IReadOnlyList<Event>>> GetEventsAsync(
            DateOnly from, DateOnly to, CancellationToken ct)
        {
            var tasks = _sources.Select(s => s.GetEventsAsync(from, to, ct));
            var results = await Task.WhenAll(tasks);
            var allEvents = results.SelectMany(r => r);

            var dict = new Dictionary<DateOnly, List<Event>>();
            for (var day = from; day <= to; day = day.AddDays(1))
                dict[day] = new List<Event>();

            foreach (var e in allEvents)
            {
                var startDay = DateOnly.FromDateTime(e.Start);
                var endDay = e.End.TimeOfDay == TimeSpan.Zero
                    ? DateOnly.FromDateTime(e.End).AddDays(-1)
                    : DateOnly.FromDateTime(e.End);
                var loopStart = startDay < from ? from : startDay;
                var loopEnd = endDay > to ? to : endDay;

                for (var day = loopStart; day <= loopEnd; day = day.AddDays(1))
                    dict[day].Add(e);
            }

            return dict.ToDictionary(
                kvp => kvp.Key,
                kvp => (IReadOnlyList<Event>)kvp.Value.OrderBy(e => e.Start).ToList());
        }
    }
}
