using MediatR;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Pet.Jira.Application.Worklogs.Commands;
using Pet.Jira.Application.Worklogs.Dto;
using Pet.Jira.Application.Worklogs.Queries;
using Pet.Jira.Web.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pet.Jira.Web.Components.Worklogs
{
    public partial class WorklogItem : ComponentBase
    {
        [Parameter] public WorklogCollectionItem Entity { get; set; }
        [Parameter] public Color Color { get; set; } = Color.Default;
        [Parameter] public EventCallback<WorklogCollectionItem> OnAddPressed { get; set; }

        [CascadingParameter] public ErrorHandler ErrorHandler { get; set; }

        private readonly string _defaultTimeFormat = "HH:mm";

        private async Task AddAsync()
        {
            await OnAddPressed.InvokeAsync(Entity);
        }
    }
}
