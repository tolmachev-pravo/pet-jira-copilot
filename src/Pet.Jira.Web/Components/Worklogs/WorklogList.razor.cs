using MediatR;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Pet.Jira.Application.Worklogs.Commands;
using Pet.Jira.Domain.Models.Worklogs;
using Pet.Jira.Web.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pet.Jira.Web.Components.Worklogs
{
    public partial class WorklogList : ComponentBase
    {
        private readonly ComponentModel Model = ComponentModel.Create();

        [Inject] private IMediator Mediator { get; set; }
        [Inject] private ISnackbar Snackbar { get; set; }
        [Parameter] public IList<DailyWorklogSummary> Items { get; set; }
        [CascadingParameter] public ErrorHandler ErrorHandler { get; set; }

        public void Refresh(IList<DailyWorklogSummary> items)
        {
            Items = items;
            StateHasChanged();
        }

        private async Task AddWorklogAsync(EstimatedWorklog entity)
        {
            try
            {
                await Mediator.Send(new AddWorklog.Command(Application.Worklogs.Dto.AddedWorklogDto.Create(entity)));
                var day = Items.Where(record => record.Date == entity.CompletedAt.Date).First();
                day.ActualWorklogs.Add(ActualWorklog.Create(entity));
                entity.RestTime = TimeSpan.Zero;
                Snackbar.Add(
                    $"Worklog {entity.Issue.Key} added successfully!",
                    Severity.Success,
                    config => { config.ActionColor = Color.Success; });
                StateHasChanged();
            }
            catch (Exception e)
            {
                ErrorHandler.ProcessError(e);
            }
        }

        private class ComponentModel
        {
            public static ComponentModel Create()
            {
                return new ComponentModel();
            }
        }
    }
}
