using Microsoft.AspNetCore.Components;
using Pet.Jira.Application.Worklogs.Dto;
using System.Threading.Tasks;

namespace Pet.Jira.Web.Components.Worklogs
{
    public partial class CalendarWorklogItem : ComponentBase
    {
        [Parameter] public WorkingDayWorklog Entity { get; set; } = default!;
        [Parameter] public bool IsLogged { get; set; }
        [Parameter] public EventCallback<WorkingDayWorklog> OnAddPressed { get; set; }

        private string Title => Entity.Issue?.Summary ?? Entity.Comment ?? string.Empty;

        private bool IsReadyToLog =>
            Entity.Issue != null &&
            !string.IsNullOrEmpty(Entity.Issue.Key) &&
            !Entity.IsEmpty;

        private async Task AddAsync()
        {
            var worklog = IsReadyToLog
                ? WorkingDayWorklog.CreateActualByEstimated(Entity)
                : Entity;
            await OnAddPressed.InvokeAsync(worklog);
        }
    }
}
