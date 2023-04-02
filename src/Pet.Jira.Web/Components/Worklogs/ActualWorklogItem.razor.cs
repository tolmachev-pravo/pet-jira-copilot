using Microsoft.AspNetCore.Components;
using Pet.Jira.Application.Worklogs.Dto;

namespace Pet.Jira.Web.Components.Worklogs
{
    public partial class ActualWorklogItem : ComponentBase
    {
        [Parameter] public WorkingDayWorklog Entity { get; set; }
    }
}
