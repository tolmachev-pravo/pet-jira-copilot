using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using Pet.Jira.Application.Worklogs.Commands;
using Pet.Jira.Application.Worklogs.Queries;
using Pet.Jira.Domain.Models.Worklogs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;

namespace Pet.Jira.Web.Pages
{
    public partial class JiraIssueList : ComponentBase
    {
        IssueQuery issueQuery = new IssueQuery();
        PageModel pageModel = new PageModel();

        [Inject]
        public IMediator _mediator { get; set; }
        [Inject]
        public ISnackbar _snackbar { get; set; }
        [Inject]
        AuthenticationStateProvider _authenticationStateProvider { get; set; }

        private async Task AddWorklog(EstimatedWorklog entity)
        {
            try
            {
                await _mediator.Send(new AddWorklog.Command(new Application.Worklogs.Dto.AddedWorklogDto
                {
                    StartedAt = entity.CompletedAt,
                    IssueKey = entity.Issue.Key,
                    ElapsedTime = entity.RestTime
                }));
                _snackbar.Add(
                    $"Worklog {entity.Issue.Key} added successfully!",
                    Severity.Normal,
                    config => { config.ActionColor = Color.Info; });
            }
            catch (Exception e)
            {
                _snackbar.Add(
                    e.Message,
                    Severity.Error,
                    config => { config.ActionColor = Color.Error; });
            }
        }

        private class PageModel
        {
            public IEnumerable<DailyWorklogSummary> DayUserWorklogs { get; set; }
            public ListState State { get; set; }
            public bool InProgress => State == ListState.IsProgress;
        }

        private enum ListState
        {
            Unknown,
            IsProgress,
            Success
        }

        public class IssueQuery
        {
            [Required]
            public DateRange DateRange { get; set; } = new DateRange(DateTime.Now.AddDays(-7).Date, DateTime.Now.Date);

            [Required]
            public DateTime? StartDate => DateRange.Start;

            [Required]
            public DateTime? EndDate => DateRange.End;

            [Required]
            [Range(1, 300)]
            public int? Count { get; set; } = 20;
        }

        private async Task Search()
        {
            try
            {
                pageModel.State = ListState.IsProgress;
                var worklogs = await _mediator.Send(new GetDailyWorklogSummaries.Query()
                {
                    StartDate = issueQuery.StartDate.Value,
                    EndDate = issueQuery.EndDate.Value,
                    Count = issueQuery.Count.Value
                });
                pageModel.DayUserWorklogs = worklogs.Worklogs;
                pageModel.State = ListState.Success;
                StateHasChanged();
            }
            catch (Exception e)
            {
                _snackbar.Add(
                    e.Message,
                    Severity.Error,
                    config => { config.ActionColor = Color.Error; });
            }
        }
    }
}
