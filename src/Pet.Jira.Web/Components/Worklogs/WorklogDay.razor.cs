using MediatR;
using Microsoft.AspNetCore.Components;
using MudBlazor;
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

        [CascadingParameter] public ErrorHandler ErrorHandler { get; set; } = default!;

        public Color Color => Entity.IsWeekend ? Color.Error : Color.Default;

        private List<DayRow> _dayRows = new();
        private WorkingDay? _previousEntity;

        protected override void OnParametersSet()
        {
            if (_previousEntity == Entity) return;
            _previousEntity = Entity;
            RebuildDayRows();
        }

        private void RebuildDayRows()
        {
            var worklogRows = Entity.ActualWorklogs
                .Where(w => w.Parent == null)
                .Select(w => new DayRow(w.RawStartDate, Worklog: w));

            var estimatedRows = Entity.EstimatedWorklogs
                .Select(w => new DayRow(w.RawStartDate, EstimatedWorklog: w));

            var blockedRows = Entity.BlockedCalendarEvents
                .Select(e => new DayRow(e.Start, BlockedEvent: e));

            _dayRows = worklogRows
                .Concat(estimatedRows)
                .Concat(blockedRows)
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
                RebuildDayRows();
                StateHasChanged();
            }
            catch (Exception e)
            {
                ErrorHandler.ProcessError(e);
            }
        }

        private async Task OpenBlockedEventDialog(BlockedCalendarEvent calendarEvent)
        {
            var template = new WorkingDayWorklog
            {
                StartDate = calendarEvent.Start,
                CompleteDate = calendarEvent.End,
                RawStartDate = calendarEvent.Start,
                RawCompleteDate = calendarEvent.End,
                Comment = calendarEvent.Title,
                Issue = null,
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
            WorkingDayWorklog? EstimatedWorklog = null,
            BlockedCalendarEvent? BlockedEvent = null);
    }
}
