using Microsoft.AspNetCore.Components;
using MudBlazor;
using Pet.Jira.Application.Worklogs.Dto;
using Pet.Jira.Domain.Models.Worklogs;
using Pet.Jira.Web.Shared;
using System.Threading.Tasks;

namespace Pet.Jira.Web.Components.Worklogs
{
    public partial class WorklogItem : ComponentBase
    {
        [Parameter] public WorkingDayWorklog Entity { get; set; }        
        [Parameter] public EventCallback<WorkingDayWorklog> OnAddPressed { get; set; }

        [CascadingParameter] public ErrorHandler ErrorHandler { get; set; }

        private async Task AddAsync()
        {
            var worklog = WorkingDayWorklog.CreateActualByEstimated(Entity);
            await OnAddPressed.InvokeAsync(worklog);
        }

        private async Task AddCustomAsync(WorkingDayWorklog worklog)
        {
            await OnAddPressed.InvokeAsync(worklog);
        }

        public Color Color => Entity.Source == WorklogSource.Assignee ? Color.Primary : Color.Info;
    }
}
