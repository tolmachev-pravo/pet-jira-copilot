using Microsoft.AspNetCore.Components;
using Pet.Jira.Application.Worklogs.Dto;

namespace Pet.Jira.Web.Components.Worklogs
{
    public partial class EstimatedActualWorklogItem : ComponentBase
    {
        [Parameter] public WorkingDayWorklog Entity { get; set; }
    }
}
