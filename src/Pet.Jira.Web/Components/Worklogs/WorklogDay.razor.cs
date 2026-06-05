using MediatR;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Pet.Jira.Application.Authentication;
using Pet.Jira.Application.Extensions.YandexCalendar.Dto;
using Pet.Jira.Application.Extensions.YandexCalendar.Queries;
using Pet.Jira.Application.Worklogs.Commands;
using Pet.Jira.Application.Worklogs.Dto;
using Pet.Jira.Domain.Models.Issues;
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
        private bool _isLoadingCalendar = true;
        private WorkingDay? _previousEntity;

        protected override async Task OnParametersSetAsync()
        {
            if (_previousEntity == Entity) return;
            _previousEntity = Entity;
            _isLoadingCalendar = true;
            await RebuildDayRows();
        }

        private async Task RebuildDayRows()
        {
            // Remove virtual calendar placeholders from a previous call
            var oldVirtual = Entity.Worklogs.Where(w => w.IsVirtualCalendar).ToList();
            foreach (var v in oldVirtual) Entity.Worklogs.Remove(v);

            IReadOnlyList<YandexCalendarEventDto> loadedEvents = Array.Empty<YandexCalendarEventDto>();
            Entity.CalendarBlockedTime = TimeSpan.Zero;
            try
            {
                var username = IdentityService.CurrentUser?.Username;
                if (!string.IsNullOrEmpty(username))
                {
                    loadedEvents = await Mediator.Send(
                        new GetYandexCalendarEvents.Query(username, DateOnly.FromDateTime(Entity.Date)));

                    var jiraWorklogs = Entity.ActualWorklogs
                        .Where(w => !w.IsVirtualCalendar)
                        .ToList();

                    foreach (var e in loadedEvents)
                    {
                        bool isLogged = jiraWorklogs.Any(w => w.StartDate < e.End && w.CompleteDate > e.Start);
                        if (!isLogged)
                        {
                            if (e.JiraIssueKeyHint is not null)
                            {
                                // Virtual actual worklog — participates in WorklogMatching with estimated worklogs
                                Entity.Worklogs.Add(new WorkingDayWorklog
                                {
                                    StartDate = e.Start,
                                    CompleteDate = e.End,
                                    RawStartDate = e.Start,
                                    RawCompleteDate = e.End,
                                    Issue = new Issue { Key = e.JiraIssueKeyHint, Identifier = e.JiraIssueKeyHint },
                                    Type = Domain.Models.Worklogs.WorklogType.Actual,
                                    Source = Domain.Models.Worklogs.WorklogSource.Calendar,
                                    IsVirtualCalendar = true
                                });
                            }
                            else
                            {
                                // No issue key — block the time unconditionally
                                Entity.CalendarBlockedTime += e.End - e.Start;
                            }
                        }
                    }
                }
            }
            catch
            {
                // Calendar unavailable — show worklogs only, silently
            }
            finally
            {
                Entity.Refresh();
                _isLoadingCalendar = false;
            }

            // Build calendar rows after Refresh so Parent assignments from WorklogMatching are final.
            // CalendarChildren: worklogs that overlap the event but were NOT matched to an estimated worklog.
            // Worklogs matched to an estimated (Parent != null) stay under their estimated parent instead.
            var allJiraWorklogs = Entity.ActualWorklogs.Where(w => !w.IsVirtualCalendar).ToList();

            var calRows = loadedEvents.Select(e =>
            {
                var children = allJiraWorklogs
                    .Where(w => w.Parent == null && w.StartDate < e.End && w.CompleteDate > e.Start)
                    .ToList();
                return new DayRow(
                    e.Start,
                    CalendarEvent: e,
                    IsCalendarEventLogged: allJiraWorklogs.Any(w => w.StartDate < e.End && w.CompleteDate > e.Start),
                    CalendarChildren: children);
            }).ToList();

            var calendarChildSet = calRows.SelectMany(r => r.CalendarChildren).ToHashSet();

            // Exclude worklogs already shown as children of a calendar event row
            var worklogRows = Entity.ActualWorklogs
                .Where(w => w.Parent == null && !w.IsVirtualCalendar && !calendarChildSet.Contains(w))
                .Select(w => new DayRow(w.RawStartDate, Worklog: w));

            var estimatedRows = Entity.EstimatedWorklogs
                .Select(w => new DayRow(w.RawStartDate, EstimatedWorklog: w));

            _dayRows = worklogRows
                .Concat(estimatedRows)
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

        private async Task OpenCalendarWorklogDialog(YandexCalendarEventDto calEvent)
        {
            var template = new WorkingDayWorklog
            {
                StartDate = calEvent.Start,
                CompleteDate = calEvent.End,
                RawStartDate = calEvent.Start,
                RawCompleteDate = calEvent.End,
                Comment = calEvent.Summary,
                Issue = calEvent.JiraIssueKeyHint is not null
                    ? new Issue { Key = calEvent.JiraIssueKeyHint, Identifier = calEvent.JiraIssueKeyHint }
                    : null,
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
            YandexCalendarEventDto? CalendarEvent = null,
            bool IsCalendarEventLogged = false,
            IReadOnlyList<WorkingDayWorklog>? CalendarChildren = null);
    }
}
