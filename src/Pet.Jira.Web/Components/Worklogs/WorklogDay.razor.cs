using MediatR;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Pet.Jira.Application.Authentication;
using Pet.Jira.Application.Extensions.Dto;
using Pet.Jira.Application.Extensions.Queries;
using Pet.Jira.Application.Worklogs.Commands;
using Pet.Jira.Application.Worklogs.Dto;
using Pet.Jira.Web.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pet.Jira.Web.Components.Worklogs
{
    public partial class WorklogDay : ComponentBase
    {
        [Parameter] public WorkingDay Entity { get; set; } = default!;

        [Inject] private IMediator Mediator { get; set; } = default!;
        [Inject] private ISnackbar Snackbar { get; set; } = default!;
        [Inject] private IDialogService DialogService { get; set; } = default!;
        [Inject] private IIdentityService IdentityService { get; set; } = default!;

        [CascadingParameter] public ErrorHandler ErrorHandler { get; set; } = default!;

        public Color Color => Entity.IsWeekend ? Color.Error : Color.Default;

        private List<DayRow> _dayRows = new();

        protected override async Task OnInitializedAsync()
        {
            await RebuildDayRows();
        }

        private async Task RebuildDayRows()
        {
            var worklogRows = Entity.ActualWorklogs
                .Where(w => w.Parent == null)
                .Select(w => new DayRow(w.RawStartDate, Worklog: w));

            IEnumerable<DayRow> calRows = Array.Empty<DayRow>();
            try
            {
                var username = IdentityService.CurrentUser?.Username;
                if (!string.IsNullOrEmpty(username))
                {
                    var events = await Mediator.Send(
                        new GetCalendarEvents.Query(username, DateOnly.FromDateTime(Entity.Date)));
                    calRows = events.Select(e => new DayRow(e.StartLocal, CalendarEvent: e));
                }
            }
            catch
            {
                // Calendar unavailable — show worklogs only, silently
            }

            _dayRows = worklogRows
                .Concat(calRows)
                .OrderBy(r => r.Time)
                .ToList();
        }

        private async Task AddWorklogAsync(WorkingDayWorklog entity)
        {
            try
            {
                await Mediator.Send(new AddWorklog.Command(AddedWorklogDto.Create(entity)));
                Entity.AddWorklog(entity);
                Snackbar.Add(
                    $"Worklog {entity.Issue.Key} added successfully!",
                    Severity.Success,
                    config => { config.ActionColor = Color.Success; });
                await RebuildDayRows();
                StateHasChanged();
            }
            catch (Exception e)
            {
                ErrorHandler.ProcessError(e);
            }
        }

        private async Task OpenCalendarWorklogDialog(CalendarEventDto calEvent)
        {
            var template = new WorkingDayWorklog
            {
                StartDate = calEvent.StartLocal,
                CompleteDate = calEvent.EndLocal,
                RawStartDate = calEvent.StartLocal,
                RawCompleteDate = calEvent.EndLocal,
                Comment = calEvent.Summary,
                Type = Domain.Models.Worklogs.WorklogType.Actual,
                Source = Domain.Models.Worklogs.WorklogSource.Calendar
            };

            var parameters = new DialogParameters
            {
                { nameof(WorklogDayItemDialog.WorkingDay), Entity },
                { nameof(WorklogDayItemDialog.WorklogTemplate), template }
            };
            var dialog = await DialogService.ShowAsync<WorklogDayItemDialog>("Add worklog", parameters);
            var result = await dialog.Result;
            if (result.Data is WorkingDayWorklog worklog)
            {
                await AddWorklogAsync(worklog);
            }
        }

        private record DayRow(
            DateTime Time,
            WorkingDayWorklog? Worklog = null,
            CalendarEventDto? CalendarEvent = null);
    }
}
