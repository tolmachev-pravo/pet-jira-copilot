using MediatR;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Pet.Jira.Application.Events.Queries;
using Pet.Jira.Domain.Models.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pet.Jira.Web.Pages
{
    public partial class Events : ComponentBase
    {
        [Inject] private IMediator Mediator { get; set; } = default!;

        private IReadOnlyDictionary<DateOnly, IReadOnlyList<Event>> _events =
            new Dictionary<DateOnly, IReadOnlyList<Event>>();
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
            }
            finally
            {
                _loading = false;
            }
        }

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
