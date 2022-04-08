using MediatR;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Pet.Jira.Application.Worklogs.Commands;
using Pet.Jira.Application.Worklogs.Queries;
using Pet.Jira.Domain.Models.Worklogs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pet.Jira.Web.Components.Worklogs
{
    public partial class WorklogList : ComponentBase
    {
        private readonly ComponentModel Model = ComponentModel.Create();

        [Inject] private IMediator Mediator { get; set; }
        [Inject] private ISnackbar Snackbar { get; set; }
        //[Parameter] public IEnumerable<DailyWorklogSummary> Items { get; set; }

        protected async Task SearchAsync(GetDailyWorklogSummaries.Query filter)
        {
            try
            {
                Model.State = ComponentModelState.IsProgress;
                var worklogs = await Mediator.Send(filter);
                Model.DayUserWorklogs = worklogs.Worklogs;
                StateHasChanged();
            }
            catch (Exception e)
            {
                Snackbar.Add(
                    e.Message,
                    Severity.Error,
                    config => { config.ActionColor = Color.Error; });
            }
            finally
            {
                Model.State = ComponentModelState.Success;
            }
        }

        private async Task AddWorklogAsync(EstimatedWorklog entity)
        {
            try
            {
                await Mediator.Send(new AddWorklog.Command(new Application.Worklogs.Dto.AddedWorklogDto
                {
                    StartedAt = entity.CompletedAt,
                    IssueKey = entity.Issue.Key,
                    ElapsedTime = entity.RestTime
                }));
                Snackbar.Add(
                    $"Worklog {entity.Issue.Key} added successfully!",
                    Severity.Normal,
                    config => { config.ActionColor = Color.Info; });
            }
            catch (Exception e)
            {
                Snackbar.Add(
                    e.Message,
                    Severity.Error,
                    config => { config.ActionColor = Color.Error; });
            }
        }

        private class ComponentModel
        {
            public static ComponentModel Create()
            {
                return new ComponentModel
                {
                    State = ComponentModelState.Unknown
                };
            }

            public IEnumerable<DailyWorklogSummary> DayUserWorklogs { get; set; }
            public ComponentModelState State { get; set; }
            public bool InProgress => State == ComponentModelState.IsProgress;
        }

        private enum ComponentModelState
        {
            Unknown,
            IsProgress,
            Success
        }
    }
}
