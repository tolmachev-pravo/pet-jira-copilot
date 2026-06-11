using MediatR;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Pet.Jira.Application.Events;
using Pet.Jira.Application.Events.Queries;
using Pet.Jira.Domain.Models.Events;
using Pet.Jira.Domain.Models.Planning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pet.Jira.Web.Pages
{
    public partial class Events : ComponentBase
    {
        [Inject] private IMediator Mediator { get; set; } = default!;
        [Inject] private IDayWorklogPlanner Planner { get; set; } = default!;

        private IReadOnlyDictionary<DateOnly, IReadOnlyList<Event>> _events =
            new Dictionary<DateOnly, IReadOnlyList<Event>>();
        private Dictionary<Event, ProposedWorklog> _proposed =
            new(ReferenceEqualityComparer.Instance);
        private bool _loading = true;

        protected override async Task OnInitializedAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var daysFromMonday = today.DayOfWeek == DayOfWeek.Sunday ? 6 : (int)today.DayOfWeek - 1;
            var monday = today.AddDays(-daysFromMonday);
            var sunday = monday.AddDays(6);

            try
            {
                _events = await Mediator.Send(new GetEvents.Query { From = monday, To = sunday });
                // Reference equality: Event is a record (value equality), so value-equal
                // duplicates would otherwise collide as dictionary keys.
                var comparer = (IEqualityComparer<Event>)ReferenceEqualityComparer.Instance;
                _proposed = _events
                    .SelectMany(kvp => Planner.Plan(kvp.Key, kvp.Value))
                    .ToDictionary(p => p.Event, p => p, comparer);
            }
            finally
            {
                _loading = false;
            }
        }

        private ProposedWorklog? GetProposed(Event e) =>
            _proposed.TryGetValue(e, out var proposed) ? proposed : null;

        private static string FormatProposed(ProposedWorklog p) =>
            FormattableString.Invariant($"{p.Start:HH:mm} – {p.End:HH:mm} ({p.Duration.TotalHours:0.##}h)");

        private static string FormatTime(Event e) =>
            e.Start == e.End
                ? e.Start.ToString("dd.MM HH:mm")
                : e.Start.Date == e.End.Date
                    ? $"{e.Start:dd.MM HH:mm} – {e.End:HH:mm}"
                    : $"{e.Start:dd.MM HH:mm} – {e.End:dd.MM HH:mm}";

        private static Color GetSourceColor(EventSource source) => source switch
        {
            EventSource.Calendar => Color.Info,
            EventSource.Task     => Color.Primary,
            EventSource.Comment  => Color.Default,
            _                    => Color.Default
        };
    }
}
