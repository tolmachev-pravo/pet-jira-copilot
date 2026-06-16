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

            var regularEstimatedRows = Entity.EstimatedWorklogs
                .Where(w => w.Source != Domain.Models.Worklogs.WorklogSource.Calendar)
                .Select(w => new DayRow(w.RawStartDate, EstimatedWorklog: w));

            var calendarEstimatedRows = Entity.EstimatedWorklogs
                .Where(w => w.Source == Domain.Models.Worklogs.WorklogSource.Calendar)
                .Select(w => new DayRow(w.RawStartDate, CalendarWorklog: w));

            var blockedRows = Entity.BlockedCalendarEvents
                .Select(e => new DayRow(e.Start, CalendarWorklog: CreateBlockedTemplate(e), BlockedEventRef: e));

            _dayRows = worklogRows
                .Concat(regularEstimatedRows)
                .Concat(calendarEstimatedRows)
                .Concat(blockedRows)
                .OrderBy(r => r.Time)
                .ToList();
        }

        private static WorkingDayWorklog CreateBlockedTemplate(BlockedCalendarEvent e)
        {
            var template = new WorkingDayWorklog
            {
                RawStartDate = e.Start,
                RawCompleteDate = e.End,
                StartDate = e.Start,
                CompleteDate = e.End,
                Comment = e.Title,
                Issue = null,
                Type = Domain.Models.Worklogs.WorklogType.Actual,
                Source = Domain.Models.Worklogs.WorklogSource.Calendar
            };
            template.UpdateRemainingTimeSpent(e.Duration);
            return template;
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

        private async Task CalendarAddAsync(WorkingDayWorklog worklog)
        {
            if (worklog.Issue == null || string.IsNullOrEmpty(worklog.Issue.Key))
            {
                var parameters = new DialogParameters
                {
                    { nameof(WorklogDayItemDialog.WorkingDay), Entity },
                    { nameof(WorklogDayItemDialog.WorklogTemplate), worklog }
                };
                var dialog = await DialogService.ShowAsync<WorklogDayItemDialog>("Add worklog", parameters);
                var result = await dialog.Result;
                if (result.Data is WorkingDayWorklog created)
                    await AddWorklogAsync(created);
            }
            else
            {
                await AddWorklogAsync(worklog);
            }
        }

        private record DayRow(
            DateTime Time,
            WorkingDayWorklog? Worklog = null,
            WorkingDayWorklog? EstimatedWorklog = null,
            WorkingDayWorklog? CalendarWorklog = null,
            BlockedCalendarEvent? BlockedEventRef = null);
    }
}
