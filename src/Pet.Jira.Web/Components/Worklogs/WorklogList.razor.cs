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
    public partial class WorklogList : ComponentBase
    {
        private readonly ComponentModel Model = ComponentModel.Create();

        [Inject] private IMediator Mediator { get; set; }
        [Inject] private ISnackbar Snackbar { get; set; }
        [Parameter] public IEnumerable<WorklogCollectionDay> Items { get; set; }
        [CascadingParameter] public ErrorHandler ErrorHandler { get; set; }

        private string DefaultTimeFormat = "HH:mm";

        public void Refresh(IEnumerable<WorklogCollectionDay> items)
        {
            Items = Items;
            StateHasChanged();
        }

        private async Task AddWorklogAsync(WorklogCollectionItem entity)
        {
            try
            {
                var actualEntityClone = entity.Clone(WorklogCollectionItemType.Actual);
                await Mediator.Send(new AddWorklog.Command(AddedWorklogDto.Create(actualEntityClone)));
                var day = Items.Where(record => record.Date == entity.CompleteDate.Date).First();
                day.AddActualItem(actualEntityClone);
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
